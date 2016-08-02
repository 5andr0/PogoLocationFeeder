using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MangaChecker.ViewModels;
using Newtonsoft.Json;
using PogoLocationFeeder.GUI.Models;
using PogoLocationFeeder.GUI.Properties;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.Repository;
using PoGo.LocationFeeder.Settings;
using POGOProtos.Enums;
using PropertyChanged;
//using POGOProtos.Enums;

namespace PogoLocationFeeder.GUI.ViewModels {
    [ImplementPropertyChanged]
    public class MainWindowViewModel {
        public int TransitionerIndex { get; set; } = 0;
        
        private string _customIp = "localhost";
        private int _customPort = 16969;

        public MainWindowViewModel() {
            Pokemons = new ReadOnlyObservableCollection<SniperInfoModel>(GlobalVariables.PokemonsInternal);
            SettingsComand = new ActionCommand(ShowSettings);
            StartStopCommand = new ActionCommand(StartStop);
            DebugComand = new ActionCommand(ShowDebug);

            var x = Directory.GetCurrentDirectory();
            var poke = new SniperInfo {
                Id = PokemonId.Missingno,
                Latitude = 45.99999,
                Longitude = 66.6677,
                ExpirationTimestamp = DateTime.Now,
            };
            var y = new SniperInfoModel() {
                Info = poke,
                Icon = new BitmapImage(new Uri(x + $"\\icons\\{(int)poke.Id}.png"))
            };
            GlobalVariables.PokemonsInternal.Add(y);
            Thread a = new Thread(new ThreadStart(Start)) {IsBackground = true, ApartmentState = ApartmentState.STA};
            //Start();
            a.Start();
        }

        private void writeDebug(string text) {
            Settings.Default.DebugOutput += $"\n{text}";
        }

        public void setStatus(string status) {
            Status = status;
        }
        public ReadOnlyObservableCollection<SniperInfoModel> Pokemons { get; }

        public ICommand SettingsComand { get; }
        public ICommand DebugComand { get; }
        public ICommand StartStopCommand { get; }

        public string CustomIp {
            get { return _customIp; }
            set { _customIp = value; }
        }

        public int CustomPort {
            get { return _customPort; }
            set { _customPort = value; }
        }

        public string Status { get; set; }

        public void ShowSettings() {
            if (TransitionerIndex != 0) {
                TransitionerIndex = 0;
                return;
            }
            TransitionerIndex = 1;
        }

        public void ShowDebug() {
            if (TransitionerIndex != 0) {
                TransitionerIndex = 0;
                return;
            }
            TransitionerIndex = 2;
        }

        private void StartStop() {
            //todo
        }

        private TcpListener listener;
        private List<TcpClient> arrSocket = new List<TcpClient>();
        private MessageParser parser = new MessageParser();
        private DiscordChannelParser channel_parser = new DiscordChannelParser();
        private MessageCache messageCache = new MessageCache();

        // A socket is still connected if a nonblocking, zero-byte Send call either:
        // 1) returns successfully or 
        // 2) throws a WAEWOULDBLOCK error code(10035)
        public static bool IsConnected(Socket client) {
            // This is how you can determine whether a socket is still connected.
            bool blockingState = client.Blocking;

            try {
                byte[] tmp = new byte[1];

                client.Blocking = false;
                client.Send(tmp, 0, 0);
                return true;
            } catch(SocketException e) {
                // 10035 == WSAEWOULDBLOCK
                return (e.NativeErrorCode.Equals(10035));
            } finally {
                client.Blocking = blockingState;
            }
        }

        public void StartNet(int port) {

            writeDebug("PogoLocationFeeder is brought to you via https://github.com/5andr0/PogoLocationFeeder");
            writeDebug("This software is 100% free and open-source.\n");

            setStatus("Application starting...");
            try {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
            } catch(Exception e) {
                writeDebug($"Could open port {port} {e}");
                throw e;
            }


            setStatus("Connecting to feeder service pogo-feed.mmoex.com");

            StartAccept();
        }

        private void StartAccept() {
            listener.BeginAcceptTcpClient(HandleAsyncConnection, listener);
        }
        private void HandleAsyncConnection(IAsyncResult res) {
            StartAccept();
            TcpClient client = listener.EndAcceptTcpClient(res);
            if(client != null && IsConnected(client.Client)) {
                arrSocket.Add(client);
                setStatus($"New connection from {getIp(client.Client)}");
            }
        }

        private string getIp(Socket s) {
            IPEndPoint remoteIpEndPoint = s.RemoteEndPoint as IPEndPoint;
            return remoteIpEndPoint.ToString();
        }

        private async Task feedToClients(List<SniperInfo> snipeList, string source) {
            // Remove any clients that have disconnected
            arrSocket.RemoveAll(x => !IsConnected(x.Client));
            List<SniperInfo> unsentMessages = messageCache.findUnSentMessages(snipeList);
            foreach(var target in unsentMessages) {
                foreach(var socket in arrSocket) // Repeat for each connected client (socket held in a dynamic array)
                {
                    try {
                        NetworkStream networkStream = socket.GetStream();
                        StreamWriter s = new StreamWriter(networkStream);

                        s.WriteLine(JsonConvert.SerializeObject(target));
                        s.Flush();
                    } catch(Exception e) {
                        writeDebug($"Caught exception: {e}");
                    }
                }
                // debug output
                await Application.Current.Dispatcher.BeginInvoke((Action)delegate () {
                    var info = new SniperInfoModel {
                        Info = target,
                        Icon = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + $"\\icons\\{(int)target.Id}.png")),
                        Source = source
                    };
                    GlobalVariables.PokemonsInternal.Insert(0, info);
                });
                String timeFormat = "HH:mm:ss";
                writeDebug($"{source}: {target.Id} at {target.Latitude.ToString(CultureInfo.InvariantCulture)},{target.Longitude.ToString(CultureInfo.InvariantCulture)}"
                    + " with " + (target.IV != default(double) ? $"{target.IV}% IV" : "unknown IV")
                    + (target.ExpirationTimestamp != default(DateTime) ? $" until {target.ExpirationTimestamp.ToString(timeFormat)}" : ""));
            }
        }

        public async Task relayMessageToClients(string message, string channel) {
            var snipeList = parser.parseMessage(message);
            await feedToClients(snipeList, channel);
        }


        public async void Start() {
            var settings = GlobalSettings.Load();
            channel_parser.Init();

            if(settings == null)
                return;

            StartNet(settings.Port);

            PollRarePokemonRepositories(settings);

            var discordWebReader = new DiscordWebReader();

            while(true) {
                try {
                    pollDiscordFeed(discordWebReader.stream);
                } catch(WebException e) {
                    setStatus($"Experiencing connection issues. Throttling...");
                    Thread.Sleep(30 * 1000);
                    discordWebReader.InitializeWebClient();
                } catch(Exception e) {
                    writeDebug($"Unknown exception {e}");
                    break;
                } finally {
                    Thread.Sleep(20 * 1000);
                }
            }

            Console.ReadKey(true);
        }

        private static IEnumerable<string> ReadLines(StreamReader stream) {
            StringBuilder sb = new StringBuilder();

            int symbol = stream.Peek();
            while(symbol != -1) {
                symbol = stream.Read();
                sb.Append((char)symbol);
                if(stream.Peek() == 10) {
                    stream.Read();
                    string line = sb.ToString();
                    sb.Clear();

                    yield return line;
                }
            }

            yield return sb.ToString();
        }

        private void pollDiscordFeed(Stream stream) {
            int delay = 10 * 1000;
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            Task.Factory.StartNew(async () => {
                while(true) {
                    for(int retrys = 0; retrys <= 3; retrys++) {
                        foreach(string line in ReadLines(new StreamReader(stream))) {
                            try {
                                string[] splitted = line.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

                                if(splitted.Length == 2 && splitted[0] == "data") {
                                    var jsonPayload = splitted[1];
                                    //Log.Debug($"JSON: {jsonPayload}");

                                    var result = JsonConvert.DeserializeObject<DiscordWebReader.DiscordMessage>(jsonPayload);
                                    if(result != null) {
                                        //Console.WriteLine($"Discord message received: {result.channel_id}: {result.content}");
                                        writeDebug($"Discord message received: {result.channel_id}: {result.content}");
                                        await relayMessageToClients(result.content, channel_parser.ToName(result.channel_id));
                                    }
                                }
                            } catch(Exception e) {
                                writeDebug($"Exception: {e}");
                            }

                        }
                        if(token.IsCancellationRequested)
                            break;
                        Thread.Sleep(delay);
                    }
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void PollRarePokemonRepositories(GlobalSettings globalSettings) {
            List<RarePokemonRepository> rarePokemonRepositories = RarePokemonRepositoryFactory.createRepositories(globalSettings);

            int delay = 30 * 1000;
            foreach(RarePokemonRepository rarePokemonRepository in rarePokemonRepositories) {
                var cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;
                var listener = Task.Factory.StartNew(async () => {
                    Thread.Sleep(5 * 1000);
                    while(true) {
                        Thread.Sleep(delay);
                        for(int retrys = 0; retrys <= 3; retrys++) {
                            var pokeSniperList = rarePokemonRepository.FindAll();
                            if(pokeSniperList != null) {
                                if(pokeSniperList.Any()) {
                                    await feedToClients(pokeSniperList, rarePokemonRepository.GetChannel());
                                } else {
                                    setStatus($"No new pokemon on {rarePokemonRepository.GetChannel()}");
                                }
                                break;
                            }
                            if(token.IsCancellationRequested)
                                break;
                            Thread.Sleep(1000);
                        }
                    }

                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }

        }
    }
}
