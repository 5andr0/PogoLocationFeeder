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

        public List<DiscordChannels> Init()
        {
            
            var configFile = Path.Combine(Directory.GetCurrentDirectory(), "discord_channels.json");

            if (File.Exists(configFile))
            {
                //if the file exists, load the settings
                var input = File.ReadAllText(configFile);

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                jsonSettings.DefaultValueHandling = DefaultValueHandling.Populate;

                settings = JsonConvert.DeserializeObject<List<DiscordChannels>>(input, jsonSettings);
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
            foreach (var e in settings)
            {
                if (String.Compare(id, e.id) == 0)
                    return ($"Server: {e.Server}, Channel: {e.Name}");
            }
            return "UNKNOWN_SOURCE: ";
        }

        public class DiscordChannels
        {
            public string id;
            public string Server;
            public string Name;
        }
    }
}