/*
PogoLocationFeeder gathers pokemon data from various sources and serves it to connected clients
Copyright (C) 2016  PogoLocationFeeder Development Team <admin@pokefeeder.live>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
using PogoLocationFeeder.Bot;
using PogoLocationFeeder.Client;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.Readers;
using PogoLocationFeeder.Repository;
using PogoLocationFeeder.Server;
using PogoLocationFeeder.Writers;
using PoGoLocationFeeder.Helper;

namespace PogoLocationFeeder
{
    public class Program
    {
        private readonly ChannelParser _channelParser = new ChannelParser();
        private readonly PogoServer _server = new PogoServer();
        private DiscordWebReader _discordWebReader;
        private readonly PogoClient _pogoClient = new PogoClient();
        private static void Main(string[] args)
        {
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

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

        public Program()
        {
            _server.ReceivedViaClients += SniperInfoReceived;
            _pogoClient._receivedViaServer += SniperInfoReceived;
        }

        private void SniperInfoReceived(object sender, SniperInfo sniperInfo)
        {
            WriteOutListeners(new List<SniperInfo>() { sniperInfo});
        }
        private void SniperInfoReceived(object sender, List<SniperInfo> sniperInfo)
        {
            WriteOutListeners( sniperInfo);
        }

        public async Task RelayMessageToClients(string message, ChannelInfo channelInfo)
        {
            var snipeList = MessageParser.ParseMessage(message);
            snipeList.ForEach(s=>s.ChannelInfo = channelInfo);
            WriteOutListeners(snipeList);
        }


        public void Start()
        {
            var settings = GlobalSettings.Load();
            _channelParser.LoadChannelSettings();

            if (settings == null) return;
            GlobalSettings.Output?.SetStatus("Connecting...");

            VersionCheckState.Execute(new CancellationToken());
            if (GlobalSettings.IsServer)
            {
                Task.Run(() =>
                {
                   _server.Start();
                });
            } else 
            {
                ClientWriter.Instance.StartNet(GlobalSettings.Port);
                Log.Info($"Starting with Port: {GlobalSettings.Port}");
            }
            if (GlobalSettings.IsManaged)
            {
				Task.Run(() =>
				{
					_pogoClient.Start(_channelParser.Settings);
				});
				StartBotListeners();
            } else if(GlobalSettings.VerifyOnSkiplagged)
			{
				SkipLaggedPokemonLocationValidator.Instance.StartVerifierThread();
			}
            WebSourcesManager(settings);

            Console.Read();
        }

        private void StartBotListeners()
        {
            if (GlobalSettings.ShareBotCaptures && !GlobalSettings.IsServer)
            {
                List<int> ports = new List<int>(GlobalSettings.BotWebSocketPorts);
                if (ports.Any())
                {
                    foreach (int port in ports)
                    {
                        Task.Run(() =>
                        {
                            new BotListener().Start(port);
                        });
                    }
                }
            }
        }
        private void WebSourcesManager(GlobalSettings settings)
        {
            var rarePokemonRepositories = RarePokemonRepositoryFactory.CreateRepositories(settings);

            List<Task> repoTasks = new List<Task>();
            Task discordTask = null;
            if (!GlobalSettings.IsManaged)
            {
                discordTask = TryStartDiscordReader();
                repoTasks =
                    rarePokemonRepositories.Select(
                        rarePokemonRepository => StartPollRarePokemonRepository(settings, rarePokemonRepository))
                        .Cast<Task>()
                        .ToList();
            }

            while (true)
            {
                if (!GlobalSettings.IsServer)
                {
                    if (!ClientWriter.Instance.Listener.Server.IsBound)
                    {
                        Log.Info("Server has lost connection. Restarting...");
                        ClientWriter.Instance.StartNet(GlobalSettings.Port);
                    }
                }
                if (!GlobalSettings.IsManaged)
                {
                    try
                    {
                        // Manage repo tasks
                        for (var i = 0; i < repoTasks.Count; ++i)
                        {
                            var t = repoTasks[i];
                            if (t.Status != TaskStatus.Running && t.Status != TaskStatus.WaitingToRun &&
                                t.Status != TaskStatus.WaitingForActivation)
                            {
                                // Replace broken tasks with a new one
                                repoTasks[i].Dispose();
                                repoTasks[i] = StartPollRarePokemonRepository(settings, rarePokemonRepositories[i]);
                            }
                        }

                        // Manage Discord task
                        if (discordTask.Status != TaskStatus.Running && discordTask.Status != TaskStatus.WaitingToRun &&
                            discordTask.Status != TaskStatus.WaitingForActivation)
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
                    await Task.Delay(30 * 1000);
                    _discordWebReader.InitializeWebClient();
                }
                catch (Exception e)
                {
                    Log.Warn($"Unknown exception", e);
                    continue;
                }
                finally
                {
                    await Task.Delay(20 * 1000);
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
                sb.Append((char)symbol);

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
                            //Ignore KimCORDashian
                            if ("210568514484961280".Equals(result.author.id) && !"218165818574241793".Equals(result.channel_id)) continue;

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
                    await Task.Delay(delay);
                }
            }
        }

        private async void WriteOutListeners(List<SniperInfo> sniperInfos)
        {
            List<SniperInfo> sniperInfosToSend = sniperInfos;
            if (!GlobalSettings.IsManaged)
            {
                sniperInfosToSend = SkipLaggedPokemonLocationValidator.Instance.FilterNonAvailableAndUpdateMissingPokemonId(sniperInfosToSend);
                var filter = FilterFactory.Create(_channelParser.Settings);
                sniperInfosToSend = SniperInfoFilter.FilterUnmanaged(sniperInfosToSend, filter);
            }
            sniperInfosToSend = sniperInfosToSend.OrderBy(m => m.ExpirationTimestamp).ToList();

            if (!GlobalSettings.IsServer)
            {
                sniperInfosToSend = MessageCache.Instance.FindUnSentMessages(sniperInfosToSend);
            }
            if (sniperInfosToSend.Any())
            {
                if (GlobalSettings.IsServer)
                {
                    _server.QueueAll(sniperInfosToSend);
                }
                else
                {
                    await ClientWriter.Instance.FeedToClients(sniperInfosToSend);
                }
            }
        }

        private async Task RareRepoThread(IRarePokemonRepository rarePokemonRepository)
        {
            while (true)
            {
                for (var retrys = 0; retrys <= 2; retrys++)
                {
                    var pokeSniperList = rarePokemonRepository.FindAll();
                    if (pokeSniperList != null)
                    {
                        if (pokeSniperList.Any())
                        {
                            WriteOutListeners(pokeSniperList);
                        }
                        break;
                    }
                    await Task.Delay(1000);
                }
                Thread.Sleep(30000);
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

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("Uncaught fatal error", e.ExceptionObject.ToString());
            Console.ReadKey();
            Environment.Exit(1);
        }
    }

    public class ThreadManager
    {
        // TODO: Refactor
    }
}
