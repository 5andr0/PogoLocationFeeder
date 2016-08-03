using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net.Config;
using Newtonsoft.Json;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.Readers;
using PogoLocationFeeder.Repository;
using PogoLocationFeeder.Writers;
using PoGoLocationFeeder.Helper;

namespace PogoLocationFeeder
{
    public class Program
    {
        private readonly ChannelParser _channelParser = new ChannelParser();
        private readonly ClientWriter _clientWriter = new ClientWriter();
        private readonly MessageParser _parser = new MessageParser();

        private static void Main(string[] args)
        {
            Console.Title = "PogoLocationFeeder";
            XmlConfigurator.Configure(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("PogoLocationFeeder.App.config"));
            try
            {
                new Program().Start();
            }
            catch (Exception e)
            {
                Log.Fatal("Error during startup", e);
            }
        }


        public async Task RelayMessageToClients(string message, ChannelInfo channelInfo)
        {
            var snipeList = _parser.parseMessage(message);
            await _clientWriter.FeedToClients(snipeList, channelInfo);
        }


        public void Start()
        {
            var settings = GlobalSettings.Load();
            _channelParser.LoadChannelSettings();

            if (settings == null) return;

            VersionCheckState.Execute(new CancellationToken());

            _clientWriter.StartNet(settings.Port);

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
                    Thread.Sleep(20*1000);
                }
            }

            Console.ReadKey(true);
        }

        private static IEnumerable<string> ReadLines(StreamReader stream)
        {
            var sb = new StringBuilder();

            var symbol = stream.Peek();
            while (symbol != -1)
            {
                symbol = stream.Read();
                sb.Append((char) symbol);

                if (stream.Peek() != 10) continue;

                stream.Read();
                var line = sb.ToString();
                sb.Clear();

                yield return line;
            }

            yield return sb.ToString();
        }

        private void PollDiscordFeed(Stream stream)
        {
            const int delay = 10*1000;
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    for (var retrys = 0; retrys <= 3; retrys++)
                    {
                        foreach (var line in ReadLines(new StreamReader(stream)))
                        {
                            try
                            {
                                var splitted = line.Split(new[] {':'}, 2, StringSplitOptions.RemoveEmptyEntries);

                                if (splitted.Length != 2 || splitted[0] != "data") continue;

                                var jsonPayload = splitted[1];
                                //Log.Debug($"JSON: {jsonPayload}");

                                var result = JsonConvert.DeserializeObject<DiscordWebReader.DiscordMessage>(jsonPayload);

                                if (result == null) continue;

                                //Console.WriteLine($"Discord message received: {result.channel_id}: {result.content}");
                                Log.Debug("Discord message received: {0}: {1}", result.channel_id,
                                    result.content);
                                var channelInfo = _channelParser.ToChannelInfo(result.channel_id);
                                if (channelInfo.isValid)
                                {
                                    await RelayMessageToClients(result.content, channelInfo);
                                }
                                else
                                {
                                    Log.Debug("Channelid {0} was not found in discord_channels.json",
                                        result.channel_id);
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
            var rarePokemonRepositories = RarePokemonRepositoryFactory.CreateRepositories(globalSettings);

            const int delay = 30*1000;
            foreach (var rarePokemonRepository in rarePokemonRepositories)
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;
                Task.Factory.StartNew(async () =>
                {
                    Thread.Sleep(5*1000);
                    while (true)
                    {
                        Thread.Sleep(delay);
                        for (var retrys = 0; retrys <= 3; retrys++)
                        {
                            var pokeSniperList = rarePokemonRepository.FindAll();
                            var channelInfo = new ChannelInfo {server = rarePokemonRepository.GetChannel()};
                            if (pokeSniperList != null)
                            {
                                if (pokeSniperList.Any())
                                {
                                    await _clientWriter.FeedToClients(pokeSniperList, channelInfo);
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