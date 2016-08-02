#region using directives

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using POGOProtos.Enums;

#endregion

namespace PoGo.LocationFeeder.Settings
{
    public class GlobalSettings
    {
        //public ulong ServerId = 206065054846681088;
        public List<string> ServerChannels = new List<string> { "coord-bot", "coords_bot", "coordsbots", "90_plus_iv", "90plus_ivonly", "rare_spottings", "high_iv_pokemon", "rare_pokemon" };
        public List<EchoEncounter> EchoEncounters = new List<EchoEncounter>();
        public string DiscordToken = "";
        public int Port = 16969;
        public bool useToken = false;
        public string DiscordUser = "";
        public string DiscordPassword = "";
        public bool usePokeSnipers = false;

        public static GlobalSettings Default => new GlobalSettings();

        public static GlobalSettings Load()
        {
            GlobalSettings settings;
            var configFile = Path.Combine(Directory.GetCurrentDirectory(), "config.json");

            if (File.Exists(configFile))
            {
                //if the file exists, load the settings
                var input = File.ReadAllText(configFile);

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                jsonSettings.DefaultValueHandling = DefaultValueHandling.Populate;

                settings = JsonConvert.DeserializeObject<GlobalSettings>(input, jsonSettings);
            }
            else
            {
                settings = new GlobalSettings();
            }

            var firstRun = !File.Exists(configFile);

            settings.Save(configFile);

            if (firstRun 
                || settings.Port == 0 
                || settings.ServerChannels == null
                || (settings.useToken && string.IsNullOrEmpty(settings.DiscordToken))
                || (!settings.useToken && string.IsNullOrEmpty(settings.DiscordUser))
                )
            {
                Console.WriteLine($"Invalid configuration detected. \nPlease edit {configFile} and try again");
                return null;
            }

            return settings;
        }

        public void Save(string fullPath)
        {
            var output = JsonConvert.SerializeObject(this, Formatting.Indented,
                new StringEnumConverter { CamelCaseText = true });

            var folder = Path.GetDirectoryName(fullPath);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(fullPath, output);
        }
    }

    public class EchoEncounter
    {
        public string Format = "{0:0.00000000},{1:0.00000000} {2:0.00}% IV {4} {5} {6}";
        public bool Always = false;
        public double MinimumIv = 0.0;
        public HashSet<PokemonId> Ids;
        public HashSet<PokemonMove> Moves;
        public int MovesToMatch = 0;
        public Dictionary<string, HashSet<string>> Channels;
    }
}
