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
using PogoLocationFeeder.Writers;
using static PogoLocationFeeder.DiscordWebReader;
using PoGoLocationFeeder.Helper;

namespace PogoLocationFeeder
{
    public class Program
    {

        static void Main(string[] args)
        {
            Console.Title = "PogoLocationFeeder";
            log4net.Config.XmlConfigurator.Configure(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("PogoLocationFeeder.App.config"));
            try
            {
                new Program().Start();
            } catch(Exception e)
            {
                Log.Fatal("Error during startup", e);
            }
            
        }
        private MessageParser parser = new MessageParser();
        private ChannelParser channel_parser = new ChannelParser();
        private ClientWriter clientWriter = new ClientWriter();


        public async Task relayMessageToClients(string message, ChannelInfo channelInfo)
        {
            var snipeList = parser.parseMessage(message);
            await clientWriter.FeedToClients(snipeList, channelInfo);
        }


        public void Start()
        {
            var settings = GlobalSettings.Load();
            channel_parser.Init();

            if (settings == null) return;

            VersionCheckState.Execute(new CancellationToken());

            clientWriter.StartNet(settings.Port);

            PollRarePokemonRepositories(settings);

            var discordWebReader = new DiscordWebReader();

            while (true)
            {
                try
                {
                    PollDiscordFeed(discordWebReader.stream);
                }
                catch (WebException)
                {
                    Log.Warn($"Experiencing connection issues. Throttling...");
                    Thread.Sleep(30*1000);
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

        private void PollDiscordFeed(Stream stream)
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
                                    Log.Debug("Discord message received: {0}: {1}", result.channel_id, result.content);
                                    var channelInfo = channel_parser.ToChannelInfo(result.channel_id);
                                    if (channelInfo.isValid)
                                    {
                                        await relayMessageToClients(result.content, channelInfo);
                                    }
                                    else
                                    {
                                        Log.Debug("Channelid {0} was not found the discord_channels.json", result.channel_id);
                                    }
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
                            var channelInfo = new ChannelInfo();
                            channelInfo.server = rarePokemonRepository.GetChannel();
                            if (pokeSniperList != null)
                            {
                                if (pokeSniperList.Any())
                                {
                                    await clientWriter.FeedToClients(pokeSniperList, channelInfo);
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
