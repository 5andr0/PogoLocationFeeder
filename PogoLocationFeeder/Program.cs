using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using PoGo.LocationFeeder.Settings;
using static PogoLocationFeeder.DiscordWebReader;

namespace PogoLocationFeeder
{
    class Program
    {
        static void Main(string[] args) => new Program().Start();

        private TcpListener listener;
        private List<TcpClient> arrSocket = new List<TcpClient>();
        private MessageParser parser = new MessageParser();
        private DiscordChannelParser channel_parser = new DiscordChannelParser();
        private PokeSniperReader pokeSniperReader = new PokeSniperReader();
        private MessageCache messageCache = new MessageCache();

        // A socket is still connected if a nonblocking, zero-byte Send call either:
        // 1) returns successfully or 
        // 2) throws a WAEWOULDBLOCK error code(10035)
        public static bool IsConnected(Socket client)
        {
            // This is how you can determine whether a socket is still connected.
            bool blockingState = client.Blocking;

            try
            {
                byte[] tmp = new byte[1];

                client.Blocking = false;
                client.Send(tmp, 0, 0);
                return true;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                return (e.NativeErrorCode.Equals(10035));
            }
            finally
            {
                client.Blocking = blockingState;
            }
        }

        public void StartNet(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("PogoLocationFeeder is brought to you via https://github.com/5andr0/PogoLocationFeeder/wiki");
            Console.WriteLine("This software is 100% free and open-source.\n");
            Console.WriteLine("Connecting to feeder service pogo-feed.mmoex.com");
            StartAccept();
        }
        private void StartAccept()
        {
            listener.BeginAcceptTcpClient(HandleAsyncConnection, listener);
        }
        private void HandleAsyncConnection(IAsyncResult res)
        {
            StartAccept();
            TcpClient client = listener.EndAcceptTcpClient(res);
            if (client != null && IsConnected(client.Client))
            {
                arrSocket.Add(client);
                Console.WriteLine($"New connection from {getIp(client.Client)}");
            }
        }

        private string getIp(Socket s)
        {
            IPEndPoint remoteIpEndPoint = s.RemoteEndPoint as IPEndPoint;
            return remoteIpEndPoint.ToString();
        }

        private async Task feedToClients(List<SniperInfo> snipeList, string source)
        {
            // Remove any clients that have disconnected
            arrSocket.RemoveAll(x => !IsConnected(x.Client));
            List<SniperInfo> unsentMessages = messageCache.findUnSentMessages(snipeList);
            foreach (var target in unsentMessages)
            {
                foreach (var socket in arrSocket) // Repeat for each connected client (socket held in a dynamic array)
                {
                    try
                    {
                        NetworkStream networkStream = socket.GetStream();
                        StreamWriter s = new StreamWriter(networkStream);

                        s.WriteLine(JsonConvert.SerializeObject(target));
                        s.Flush();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Caught exception: {e.ToString()}");
                    }
                }
                // debug output
                Console.WriteLine($"{source} ID: {target.Id}, Lat:{target.Latitude}, Lng:{target.Longitude}, IV:{target.IV}");
                if (target.ExpirationTimestamp != default(DateTime))
                    Console.WriteLine($"Expires: {target.ExpirationTimestamp}");
            }
        }

        public async Task relayMessageToClients(string message, string channel)
        {
            var snipeList = parser.parseMessage(message);
            await feedToClients(snipeList, channel);
        }

        public async void Start()
        {
            var settings = GlobalSettings.Load();
            channel_parser.Init();

            if (settings == null) return;

            StartNet(settings.Port);
            if (settings.usePokeSnipers)
            {
                pollPokesniperFeed();
            }

            var discordWebReader = new DiscordWebReader();

            while(true)
            {
                try
                {
                    Console.WriteLine($"Connection issues. Retrying...");
                    discordWebReader.InitializeWebClient();
                    Thread.Sleep(10 * 1000);
                    Console.WriteLine($"Connection established. Waiting for data...");
                    pollDiscordFeed(discordWebReader.stream);
                }
                catch (WebException e)
                {
                    Console.WriteLine($"Experiencing connection issues. Throttling...");
                    Thread.Sleep(30 * 1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unknown exception: {e.ToString()}\n\n\n");
                    break;
                }
            }
            
            Console.ReadKey(true);

        }

        private void pollDiscordFeed(Stream stream)
        {
            int delay = 30 * 1000;
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            var listener = Task.Factory.StartNew(async () =>
            {
                Thread.Sleep(5 * 1000);
                while (true)
                {
                    var encoder = new UTF8Encoding();
                    var buffer = new byte[2048];

                    for (int retrys = 0; retrys <= 3; retrys++)
                    {
                        if (stream.CanRead)
                        {
                            int len = stream.Read(buffer, 0, 2048);
                            if (len > 0)
                            {
                                var serverPayload = encoder.GetString(buffer, 0, len);
                                if (serverPayload == null) continue;
                                //Console.WriteLine("text={0}", serverPayload);

                                try
                                {   
                                    var split = serverPayload.Split(new[] { '\r', '\n' });
                                    if (split.Length < 3) continue;

                                    var message = split[2];
                                    if (message.Length == 0) continue;

                                    var jsonPayload = message.Substring(5);
                                    //Console.WriteLine($"JSON: {jsonPayload}");

                                    var result = JsonConvert.DeserializeObject<DiscordMessage>(jsonPayload);
                                    if (result != null)
                                    {
                                        //Console.WriteLine($"Discord message received: {result.channel_id}: {result.content}");
                                        var pokeSniperList = pokeSniperReader.readAll();
                                        await relayMessageToClients(result.content, channel_parser.ToName(result.channel_id));
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"Exception: {e.ToString()}\n\n\n");
                                }
                            }
                        }
                        if (token.IsCancellationRequested)
                            break;
                        Thread.Sleep(delay);
                    }
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);   
        }

        private void pollPokesniperFeed()
        {
            int delay = 30 * 1000;
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            var listener = Task.Factory.StartNew(async () =>
            {
                Thread.Sleep(5 * 1000);
                while (true)
                {
                    Thread.Sleep(delay);
                    for (int retrys = 0; retrys <= 3; retrys++)
                    {
                        var pokeSniperList = pokeSniperReader.readAll();
                        if (pokeSniperList != null)
                        {
                            if (pokeSniperList.Any())
                            {
                                await feedToClients(pokeSniperList, "PokeSnipers");
                            }
                            else
                            {
                                Console.WriteLine("No new pokemon on PokeSnipers");
                            }
                            break;
                        }
                        if (token.IsCancellationRequested)
                            break;
                        Thread.Sleep(1000);
                    }
                }

            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
