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
        private DiscordWebReader _discordWebReader;

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

            _clientWriter.StartNet(GlobalSettings.Port);
            Log.Info($"Starting with Port: {GlobalSettings.Port}");

            WebSourcesManager(settings);

            Console.Read();
        }

        private void WebSourcesManager(GlobalSettings settings)
        {
            var rarePokemonRepositories = RarePokemonRepositoryFactory.CreateRepositories(settings);

            var repoTasks = rarePokemonRepositories.Select(rarePokemonRepository => StartPollRarePokemonRepository(settings, rarePokemonRepository)).Cast<Task>().ToList();

            var discordTask = TryStartDiscordReader();

            while (true)
            {
                if (!_clientWriter.Listener.Server.IsBound)
                {
                    Log.Info("Server has lost connection. Restarting...");
                    _clientWriter.StartNet(GlobalSettings.Port);
                }
                try
                {
                    // Manage repo tasks
                    for (var i = 0; i < repoTasks.Count; ++i)
                    {
                        var t = repoTasks[i];
                        if (t.Status != TaskStatus.Running && t.Status != TaskStatus.WaitingToRun && t.Status != TaskStatus.WaitingForActivation)
                        {
                            // Replace broken tasks with a new one
                            repoTasks[i].Dispose();
                            repoTasks[i] = StartPollRarePokemonRepository(settings, rarePokemonRepositories[i]);
                        }
                    }

                    // Manage Discord task
                    if (discordTask.Status != TaskStatus.Running && discordTask.Status != TaskStatus.WaitingToRun && discordTask.Status != TaskStatus.WaitingForActivation)
                    {
                        // Replace broken task with a new one
                        discordTask.Dispose();
                        discordTask = TryStartDiscordReader();
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Exception in thread manager: {e}");
                    throw;
                }
                Thread.Sleep(20 * 1000);
            }
        }

        private async Task<Task> TryStartDiscordReader()
        {
            while (true)
            {
                _discordWebReader = new DiscordWebReader();

                try
                {
                    return await StartPollDiscordFeed(_discordWebReader.stream);
                }
                catch (WebException)
                {
                    Log.Warn($"Experiencing connection issues. Throttling...");
                    Thread.Sleep(30 * 1000);
                    _discordWebReader.InitializeWebClient();
                }
                catch (Exception e)
                {
                    Log.Warn($"Unknown exception", e);
                    continue;
                }
                finally
                {
                    Thread.Sleep(20 * 1000);
                }
            }
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

        private async Task DiscordThread(Stream stream)
        {
            const int delay = 10 * 1000;
            while (true)
            {
                for (var retrys = 0; retrys <= 3; retrys++)
                {
                    foreach (var line in ReadLines(new StreamReader(stream)))
                    {
                        try
                        {
                            var splitted = line.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

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
                    Thread.Sleep(delay);
                }
            }
        }

        private async Task RareRepoThread(IRarePokemonRepository rarePokemonRepository)
        {
            const int delay = 30 * 1000;
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
                            if(rarePokemonRepository is SkiplaggedPokemonRepository)
                            {
                                foreach (var item in pokeSniperList.Cast<SkiplaggedSniperInfo>().GroupBy(p=>p.RegionName))
                                {
                                    channelInfo.channel = item.Key;
                                    await _clientWriter.FeedToClients(item.Cast<SniperInfo>().ToList(), channelInfo);
                                }

                            }
                            else await _clientWriter.FeedToClients(pokeSniperList, channelInfo);
                        }
                        else
                        {
                            Log.Debug("No new pokemon on {0}", rarePokemonRepository.GetChannel());
                        }
                        break;
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        private async Task<Task> StartPollDiscordFeed(Stream stream)
        {
            return await Task.Factory.StartNew(async () => await DiscordThread(stream), TaskCreationOptions.LongRunning);
        }

        private async Task<Task> StartPollRarePokemonRepository(GlobalSettings globalSettings, IRarePokemonRepository rarePokemonRepository)
        {
            return await Task.Factory.StartNew(async () => await RareRepoThread(rarePokemonRepository), TaskCreationOptions.LongRunning);
        }
    }

    public class ThreadManager
    {
        // TODO: Refactor
    }
}