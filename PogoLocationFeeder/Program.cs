using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using PoGo.LocationFeeder.Settings;
using System.Globalization;
using PogoLocationFeeder.Repository;
using static PogoLocationFeeder.DiscordWebReader;

namespace PogoLocationFeeder
{
    public class Program
    {

        static void Main(string[] args)
        {
            Console.Title = "PogoLocationFeeder";
            try
            {
                new Program().Start();
            } catch(Exception e)
            {
                Log.Fatal("Error during startup", e);
            }
            
        }
        private TcpListener listener;
        private List<TcpClient> arrSocket = new List<TcpClient>();
        private MessageParser parser = new MessageParser();
        private DiscordChannelParser channel_parser = new DiscordChannelParser();
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

            Log.Plain("PogoLocationFeeder is brought to you via https://github.com/5andr0/PogoLocationFeeder");
            Log.Plain("This software is 100% free and open-source.\n");

            Log.Info("Application starting...");
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
            } catch(Exception e)
            {
                Log.Fatal($"Could open port {port}", e);
                throw e;
            }


            Log.Info("Connecting to feeder service pogo-feed.mmoex.com");

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
                Log.Info($"New connection from {getIp(client.Client)}");
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
                        Log.Error($"Caught exception: {e.ToString()}");
                    }
                }
                // debug output
                String timeFormat = "HH:mm:ss";
                Log.Pokemon($"{source}: {target.Id} at {target.Latitude.ToString(CultureInfo.InvariantCulture)},{target.Longitude.ToString(CultureInfo.InvariantCulture)}"
                    + " with " + (target.IV != default(double) ? $"{target.IV}% IV" : "unknown IV")
                    + (target.ExpirationTimestamp != default(DateTime) ? $" until {target.ExpirationTimestamp.ToString(timeFormat)}" : ""));
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

            PollRarePokemonRepositories(settings);

            var discordWebReader = new DiscordWebReader();

            while (true)
            {
                try
                {
                    pollDiscordFeed(discordWebReader.stream);
                }
                catch (WebException e)
                {
                    Log.Warn($"Experiencing connection issues. Throttling...");
                    discordWebReader.InitializeWebClient();
                }
                catch (Exception e)
                {
                    Log.Warn($"Unknown exception", e);
                    break;
                }
                finally
                {
                    Thread.Sleep(20 * 1000);
                }
            }

            Console.ReadKey(true);
        }

        private static IEnumerable<string> ReadLines(StreamReader stream)
        {
            StringBuilder sb = new StringBuilder();

            int symbol = stream.Peek();
            while (symbol != -1)
            {
                symbol = stream.Read();
                sb.Append((char)symbol);
                if (stream.Peek() == 10)
                {
                    stream.Read();
                    string line = sb.ToString();
                    sb.Clear();

                    yield return line;
                }
            }

            yield return sb.ToString();
        }

        private void pollDiscordFeed(Stream stream)
        {
            int delay = 10 * 1000;
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            Task.Factory.StartNew(async () =>
            {
            while (true)
            {
                for (int retrys = 0; retrys <= 3; retrys++)
                {
                    foreach (string line in ReadLines(new StreamReader(stream)))
                    {
                        try
                        {
                            string[] splitted = line.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

                            if (splitted.Length == 2 && splitted[0] == "data")
                            {
                                var jsonPayload = splitted[1];
                                //Log.Debug($"JSON: {jsonPayload}");

                                var result = JsonConvert.DeserializeObject<DiscordMessage>(jsonPayload);
                                if (result != null)
                                {
                                    //Console.WriteLine($"Discord message received: {result.channel_id}: {result.content}");
                                    Log.Debug($"Discord message received: {result.channel_id}: {result.content}");
                                    await relayMessageToClients(result.content, channel_parser.ToName(result.channel_id));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Warn($"Exception:", e);
                        }

                    }
                    if (token.IsCancellationRequested)
                        break;
                    Thread.Sleep(delay);
                }
            }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void PollRarePokemonRepositories(GlobalSettings globalSettings)
        {
            List<RarePokemonRepository> rarePokemonRepositories = RarePokemonRepositoryFactory.createRepositories(globalSettings);

            int delay = 30 * 1000;
            foreach(RarePokemonRepository rarePokemonRepository in rarePokemonRepositories) {
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
                            var pokeSniperList = rarePokemonRepository.FindAll();
                            if (pokeSniperList != null)
                            {
                                if (pokeSniperList.Any())
                                {
                                    await feedToClients(pokeSniperList, rarePokemonRepository.GetChannel());
                                }
                                else
                                {
                                    Log.Debug("No new pokemon on {0}", rarePokemonRepository.GetChannel());
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
}
