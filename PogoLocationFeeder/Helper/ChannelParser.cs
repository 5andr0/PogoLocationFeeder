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
    public class ChannelParser
    {
        public int Port = 16969;
        public bool usePokeSnipers = false;

        public static ChannelParser Default => new ChannelParser();
        public List<DiscordChannels> settings = null;

        public List<DiscordChannels> Init()
        {
            var configFile = Path.Combine(Directory.GetCurrentDirectory(), "Config", "discord_channels.json");

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

        public ChannelInfo ToChannelInfo(string channelId)
        {
            ChannelInfo channelInfo = new ChannelInfo();
            if (channelId != null)
            {
                foreach (var channel in settings)
                {
                    if (String.Compare(channelId, channel.id) == 0)
                    {
                        channelInfo.server = channel.Server;
                        channelInfo.channel = channel.Name;
                        channelInfo.isValid = true;
                        return channelInfo;
                    }
                }
            }
            channelInfo.server = "Unknown";
            channelInfo.channel = "Unknown";

            return channelInfo;

        }

        public class DiscordChannels
        {
            public string id;
            public string Server;
            public string Name;
        }
    }

    public class SourceInfo
    {
        public string server { get; set; }
        public string channelId { get; set; }
    }

}