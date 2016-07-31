using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord;
using POGOProtos.Enums;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using PoGo.LocationFeeder.Settings;
using System.Globalization;

namespace PogoLocationFeeder
{
    class Program
    {
        static void Main(string[] args) => new Program().Start();

        private TcpListener listener;
        private List<TcpClient> arrSocket = new List<TcpClient>();
        private MessageParser parser = new MessageParser();
        private PokeSniperReader pokeSniperReader = new PokeSniperReader();
        private List<SniperInfo> sentMessages = new List<SniperInfo>();

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
            Console.WriteLine("Listening...");
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

        private DiscordClient _client;

        private async Task feedToClients(List<SniperInfo> snipeList, string channel)
        {
            // Remove any clients that have disconnected
            arrSocket.RemoveAll(x => !IsConnected(x.Client));

            foreach (var target in snipeList)
            {
                var logMessage = $"Channel: {channel} ID: {target.id}, Lat:{target.latitude}, Lng:{target.longitude}, IV:{target.iv}";

                if (sentMessages.Any(x => x.latitude == target.latitude && x.longitude == target.longitude && x.id == target.id))
                {
                    logMessage += " DUPLICATE";
                }
                else
                {
                    sentMessages.Add(target);

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
                }
                // debug output
                if (target.expirationTime != default(DateTime)) logMessage += $" Expires: {target.expirationTime}";

                Console.WriteLine(logMessage);

                // remove old log entries (older than 1 minute)
                sentMessages.RemoveAll(x => x.creationTime <= DateTime.Now.AddMinutes(-1));
            }
        }

        private async Task relayMessageToClients(string message, string channel)
        {
            var snipeList = parser.parseMessage(message);
            await feedToClients(snipeList, channel);
        }

        public async void Start()
        {
            var settings = GlobalSettings.Load();

            if (settings == null) return;


            _client = new DiscordClient();

            StartNet(settings.Port);
            if (settings.usePokeSnipers)
            {
                pollPokesniperFeed();
            }
            _client.MessageReceived += async (s, e) =>
            {
                if (settings.ServerChannels.Any(x => x.Equals(e.Channel.Name.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    await relayMessageToClients(e.Message.Text, e.Channel.Name.ToString());
                }
            };

            _client.ExecuteAndWait(async () =>
            {
                if (settings.useToken && settings.DiscordToken != null)
                    await _client.Connect(settings.DiscordToken);
                else if (settings.DiscordUser != null && settings.DiscordPassword != null)
                {
                    try
                    {
                        await _client.Connect(settings.DiscordUser, settings.DiscordPassword);
                    }
                    catch
                    {
                        Console.WriteLine("Failed to authroize Discord user! Check your config.json and try again.");
                        Console.ReadKey();
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Please set your logins in the config.json first");
                }
            });
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
                        var pokeSniperList = await pokeSniperReader.readAll();
                        if (pokeSniperList != null)
                        {
                            if(pokeSniperList.Any()) {
                                await feedToClients(pokeSniperList, "PokeSnipers");
                            } else
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
