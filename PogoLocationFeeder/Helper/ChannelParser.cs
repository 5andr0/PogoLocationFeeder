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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PogoLocationFeeder.Helper
{
    public class ChannelParser
    {
        public List<DiscordChannels> Settings;

        public static ChannelParser Default => new ChannelParser();

        public void LoadChannelSettings()
        {
            var configFile = Path.Combine(Directory.GetCurrentDirectory(), "Config", "discord_channels.json");

            if (File.Exists(configFile))
            {
                //if the file exists, load the Settings
                var input = File.ReadAllText(configFile);

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Converters.Add(new StringEnumConverter {CamelCaseText = true});
                jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                jsonSettings.DefaultValueHandling = DefaultValueHandling.Populate;

                Settings = JsonConvert.DeserializeObject<List<DiscordChannels>>(input, jsonSettings);
            }
            else
            {
                Settings = new List<DiscordChannels>();
                Log.Error($"Channel file \"{configFile}\" not found!");
            }

        }

        public ChannelInfo ToChannelInfo(string channelId)
        {
            var channelInfo = new ChannelInfo();
            if (channelId != null)
            {
                foreach (var channel in Settings)
                {
                    if (Object.Equals(channelId, channel.id))
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
            public string Name;
            public string Server;
        }
    }

    public class SourceInfo
    {
        public string server { get; set; }
        public string channelId { get; set; }
    }
}
