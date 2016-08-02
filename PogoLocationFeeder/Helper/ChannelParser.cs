using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using POGOProtos.Enums;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PogoLocationFeeder.Helper
{
    public class DiscordChannelParser
    {
        public int Port = 16969;
        public bool usePokeSnipers = false;

        public static DiscordChannelParser Default => new DiscordChannelParser();
        public List<DiscordChannels> settings = null;
        private Dictionary<string, DiscordChannels> idToChannels = null;

        public List<DiscordChannels> Init()
        {

            var configFile = Path.Combine(Directory.GetCurrentDirectory(), "Config", "discord_channels.json");
            idToChannels = new Dictionary<string, DiscordChannels>();

            if (File.Exists(configFile))
            {
                //if the file exists, load the settings
                var input = File.ReadAllText(configFile);

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                jsonSettings.DefaultValueHandling = DefaultValueHandling.Populate;

                settings = JsonConvert.DeserializeObject<List<DiscordChannels>>(input, jsonSettings);
                foreach (var setting in settings)
                {
                    idToChannels[setting.id] = setting;
                }
            }
            else
            {
                settings = new List<DiscordChannels>();
                Log.Error($"Channel file \"{configFile}\" not found!");
            }

            return settings;
        }

        public string ToName(string id)
        {
            DiscordChannels channel;
            idToChannels.TryGetValue(id, out channel);
            if (channel != null)
            {
                return $"Server: {channel.Server}, Channel: {channel.Name}";
            }
            return "UNKNOWN_SOURCE: ";
        }

        public bool IsKnownChannel(string id)
        {
            return idToChannels.ContainsKey(id);
        }

        public class DiscordChannels
        {
            public string id;
            public string Server;
            public string Name;
        }
    }
}