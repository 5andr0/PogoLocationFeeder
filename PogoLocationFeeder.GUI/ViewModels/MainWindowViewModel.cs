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
using MaterialDesignThemes.Wpf;
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
        private static readonly string assetPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        private static readonly string iconPath = Path.Combine(assetPath, "icons");

        private readonly List<TcpClient> arrSocket = new List<TcpClient>();
        private readonly DiscordChannelParser channel_parser = new DiscordChannelParser();

        private TcpListener listener;
        private readonly MessageCache messageCache = new MessageCache();
        private readonly MessageParser parser = new MessageParser();

        public MainWindowViewModel() {
            Pokemons = new ReadOnlyObservableCollection<SniperInfoModel>(GlobalVariables.PokemonsInternal);
            SettingsComand = new ActionCommand(ShowSettings);
            StartStopCommand = new ActionCommand(Startstop);
            DebugComand = new ActionCommand(ShowDebug);

            Settings.Default.DebugOutput = "Debug stuff in here!";
            //var poke = new SniperInfo {
            //    Id = PokemonId.Missingno,
            //    Latitude = 45.99999,
            //    Longitude = 66.6677,
            //    ExpirationTimestamp = DateTime.Now
            //};
            //var y = new SniperInfoModel {
            //    Info = poke,
            //    Icon = new BitmapImage(new Uri(Path.Combine(iconPath, $"{(int) poke.Id}.png")))
            //};
            //GlobalVariables.PokemonsInternal.Add(y);

            var thread = new Thread(Start) {IsBackground = true};
            thread.Start();
        }

        public int TransitionerIndex { get; set; } = 0;

        //public PackIconKind PausePlayButtonIcon { get; set; } = PackIconKind.Pause;
        public ReadOnlyObservableCollection<SniperInfoModel> Pokemons { get; }

        public ICommand SettingsComand { get; }
        public ICommand DebugComand { get; }
        public ICommand StartStopCommand { get; }

        public string CustomIp { get; set; } = "localhost";

        public int CustomPort { get; set; } = 16969;

        public string Status { get; set; }

        public string ThreadStatus { get; set; } = "[Running]";

        public int ShowLimit {
            get {
                if (Settings.Default.ShowLimit.Equals(0)) return 1;
                return Settings.Default.ShowLimit;
            }
            set {
                if (value <= 0) value = 1;
                Settings.Default.ShowLimit = value;
                Settings.Default.Save();
            }
        }

        private void writeDebug(string text) {
            Settings.Default.DebugOutput += $"\n{text}";
        }

        public void setStatus(string status) {
            Status = status;
        }

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

        private void Startstop() {
            //todo
        }

        // A socket is still connected if a nonblocking, zero-byte Send call either:
        // 1) returns successfully or 
        // 2) throws a WAEWOULDBLOCK error code(10035)
        public static bool IsConnected(Socket client) {
            // This is how you can determine whether a socket is still connected.
            var blockingState = client.Blocking;

            try {
                var tmp = new byte[1];

                client.Blocking = false;
                client.Send(tmp, 0, 0);
                return true;
            } catch (SocketException e) {
                // 10035 == WSAEWOULDBLOCK
                return e.NativeErrorCode.Equals(10035);
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
            } catch (Exception e) {
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
            var client = listener.EndAcceptTcpClient(res);
            if (client != null && IsConnected(client.Client)) {
                arrSocket.Add(client);
                setStatus($"New connection from {getIp(client.Client)}");
            }
        }

        private string getIp(Socket s) {
            var remoteIpEndPoint = s.RemoteEndPoint as IPEndPoint;
            return remoteIpEndPoint.ToString();
        }

        private async Task feedToClients(List<SniperInfo> snipeList, string source) {
            // Remove any clients that have disconnected
            arrSocket.RemoveAll(x => !IsConnected(x.Client));
            var unsentMessages = messageCache.findUnSentMessages(snipeList);
            foreach (var target in unsentMessages) {
                foreach (var socket in arrSocket) // Repeat for each connected client (socket held in a dynamic array)
                {
                    try {
                        var networkStream = socket.GetStream();
                        var s = new StreamWriter(networkStream);

                        s.WriteLine(JsonConvert.SerializeObject(target));
                        s.Flush();
                    } catch (Exception e) {
                        writeDebug($"Caught exception: {e}");
                    }
                }
                // debug output
                await Application.Current.Dispatcher.BeginInvoke((Action) delegate {
                    var info = new SniperInfoModel {
                        Info = target,
                        Icon = new BitmapImage(new Uri(Path.Combine(iconPath, $"{(int) target.Id}.png"))),
                        Source = source
                    };
                    InsertToList(info);
                });
                var timeFormat = "HH:mm:ss";
                writeDebug($"{source}: {target.Id} at {target.Latitude.ToString(CultureInfo.InvariantCulture)},{target.Longitude.ToString(CultureInfo.InvariantCulture)}"
                           + " with " + (target.IV != default(double) ? $"{target.IV}% IV" : "unknown IV")
                           +
                           (target.ExpirationTimestamp != default(DateTime)
                               ? $" until {target.ExpirationTimestamp.ToString(timeFormat)}"
                               : ""));
            }
        }

        public void InsertToList(SniperInfoModel info) {
            var pokes = GlobalVariables.PokemonsInternal;
            if (pokes.Count > ShowLimit) {
                var diff = pokes.Count - ShowLimit;
                for (int i = 0; i < diff; i++) {
                    pokes.Remove(pokes.Last());
                }
            }

            if (pokes.Count >= ShowLimit)
                pokes.Remove(pokes.Last());
            pokes.Insert(0, info);
        }

        public async Task relayMessageToClients(string message, string channel) {
            var snipeList = parser.parseMessage(message);
            await feedToClients(snipeList, channel);
        }


        public async void Start() {
            var settings = GlobalSettings.Load();
            channel_parser.Init();

            if (settings == null)
                return;

            StartNet(settings.Port);

            PollRarePokemonRepositories(settings);

            var discordWebReader = new DiscordWebReader();

            while (true) {
                try {
                    pollDiscordFeed(discordWebReader.stream);
                } catch (WebException e) {
                    setStatus($"Experiencing connection issues. Throttling...");
                    Thread.Sleep(30*1000);
                    discordWebReader.InitializeWebClient();
                } catch (Exception e) {
                    writeDebug($"Unknown exception {e}");
                    break;
                } finally {
                    Thread.Sleep(20*1000);
                }
            }

            Console.ReadKey(true);
        }

        private static IEnumerable<string> ReadLines(StreamReader stream) {
            var sb = new StringBuilder();

            var symbol = stream.Peek();
            while (symbol != -1) {
                symbol = stream.Read();
                sb.Append((char) symbol);
                if (stream.Peek() == 10) {
                    stream.Read();
                    var line = sb.ToString();
                    sb.Clear();

                    yield return line;
                }
            }

            yield return sb.ToString();
        }

        private void pollDiscordFeed(Stream stream) {
            var delay = 10*1000;
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            Task.Factory.StartNew(async () => {
                while (true) {
                    for (var retrys = 0; retrys <= 3; retrys++) {
                        foreach (var line in ReadLines(new StreamReader(stream))) {
                            try {
                                var splitted = line.Split(new[] {':'}, 2, StringSplitOptions.RemoveEmptyEntries);

                                if (splitted.Length == 2 && splitted[0] == "data") {
                                    var jsonPayload = splitted[1];
                                    //Log.Debug($"JSON: {jsonPayload}");

                                    var result =
                                        JsonConvert.DeserializeObject<DiscordWebReader.DiscordMessage>(jsonPayload);
                                    if (result != null) {
                                        //Console.WriteLine($"Discord message received: {result.channel_id}: {result.content}");
                                        writeDebug($"Discord message received: {result.channel_id}: {result.content}");
                                        await
                                            relayMessageToClients(result.content,
                                                channel_parser.ToName(result.channel_id));
                                    }
                                }
                            } catch (Exception e) {
                                writeDebug($"Exception: {e}");
                            }
                        }
                        if (token.IsCancellationRequested)
                            break;
                        Thread.Sleep(delay);
                    }
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void PollRarePokemonRepositories(GlobalSettings globalSettings) {
            var rarePokemonRepositories = RarePokemonRepositoryFactory.createRepositories(globalSettings);

            var delay = 30*1000;
            foreach (var rarePokemonRepository in rarePokemonRepositories) {
                var cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;
                var listener = Task.Factory.StartNew(async () => {
                    Thread.Sleep(5*1000);
                    while (true) {
                        Thread.Sleep(delay);
                        for (var retrys = 0; retrys <= 3; retrys++) {
                            var pokeSniperList = rarePokemonRepository.FindAll();
                            if (pokeSniperList != null) {
                                if (pokeSniperList.Any()) {
                                    await feedToClients(pokeSniperList, rarePokemonRepository.GetChannel());
                                } else {
                                    setStatus($"No new pokemon on {rarePokemonRepository.GetChannel()}");
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