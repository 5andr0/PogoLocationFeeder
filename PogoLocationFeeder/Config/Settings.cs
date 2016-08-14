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

#region using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;

#endregion

namespace PogoLocationFeeder.Config
{
    public class GlobalSettings
    {
        public static bool ThreadPause = false;
        public static GlobalSettings Settings;
        public static bool Gui = false;
        public static IOutput Output;
        public static int Port = 16969;
        public static bool UsePokeSnipers = true;
        public static bool UseTrackemon = false;
        public static bool UsePokewatchers = true;
        public static bool UsePokezz = true;
        public static bool UsePokemonGoIVClub = true;
        public static bool UseFilter = true;
        public static string AppTheme = "Dark";
        public static bool IsServer = false;
        public static bool IsManaged = true;
        public static string ServerHost = "pogofeeder.live";
        public static int ServerPort = 49000;
        public static string PokeSnipers2Exe = "";
        public static int RemoveAfter = 15;
        public static int ShowLimit = 30;
        public static bool VerifyOnSkiplagged = true;
        public static bool ShareBotCaptures = true;
        public static List<string> PokekomsToFeedFilter;
        public static List<int> BotWebSocketPorts = new List<int>() { 14251 };

        public static bool SniperVisibility => IsOneClickSnipeSupported();
        public static GlobalSettings Default => new GlobalSettings();
        public static string ConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "Config", "config.json");

        public static string FilterPath = Path.Combine(Directory.GetCurrentDirectory(), "Config", "filter.json");


        public static GlobalSettings Load()
        {
            GlobalSettings settings;

            if (File.Exists(ConfigFile)) {
                SettingsToSave set;
                //if the file exists, load the Settings
                var input = File.ReadAllText(ConfigFile);

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Converters.Add(new StringEnumConverter {CamelCaseText = true});
                jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                jsonSettings.DefaultValueHandling = DefaultValueHandling.Populate;
                set = JsonConvert.DeserializeObject<SettingsToSave>(input, jsonSettings);
                settings = new GlobalSettings();
                Port = set.Port;
                //UseTrackemon = set.UseTrackemon;
                UsePokeSnipers = set.UsePokeSnipers;
                UsePokewatchers = set.UsePokewatchers;
                UsePokezz = set.UsePokezz;
                UsePokemonGoIVClub = set.UsePokemonGoIVClub;
                VerifyOnSkiplagged = set.VerifyOnSkiplagged;
                RemoveAfter = set.RemoveAfter;
                ShowLimit = Math.Max(set.ShowLimit, 1);
                PokeSnipers2Exe = set.PokeSnipers2Exe;
                UseFilter = set.UseFilter;
                AppTheme = set.AppTheme;
                IsServer = set.IsServer;
                IsManaged = set.IsManaged;
                ServerHost = set.ServerHost;
                ServerPort = set.ServerPort;
                ShareBotCaptures = set.ShareBotCaptures;
                BotWebSocketPorts = set.BotWebSocketPorts;
            }
            else
            {
                settings = new GlobalSettings();
            }
            PokekomsToFeedFilter = LoadFilter();
            var firstRun = !File.Exists(ConfigFile);
            Save();

            if (firstRun
                || Port == 0
                )
            {
                Log.Error($"Invalid configuration detected. \nPlease edit {ConfigFile} and try again");
                return null;
            }
            return settings;
        }

        public static bool IsOneClickSnipeSupported()
        {
            if (PokeSnipers2Exe != null && PokeSnipers2Exe.Contains(".exe"))
            {
                return true;
            }
            const string keyName = @"pokesniper2\Shell\Open\Command";
            //return Registry.GetValue(keyName, valueName, null) == null;
            using (var Key = Registry.ClassesRoot.OpenSubKey(keyName))
            {
                return Key != null;
            }
        }

        public static void Save()
        {
            var output = JsonConvert.SerializeObject(new SettingsToSave(), Formatting.Indented,
                new StringEnumConverter {CamelCaseText = true});

            var folder = Path.GetDirectoryName(ConfigFile);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            try {
                File.WriteAllText(ConfigFile, output);
            } catch (Exception) {
                //ignore
            }
        }
        public static List<string> DefaultPokemonsToFeed = new List<string>() {"Venusaur", "Charizard", "Blastoise","Beedrill","Raichu","Sandslash","Nidoking","Nidoqueen","Clefable","Ninetales",
            "Golbat","Vileplume","Golduck","Primeape","Arcanine","Poliwrath","Alakazam","Machamp","Golem","Rapidash","Slowbro","Farfetchd","Muk","Cloyster","Gengar","Exeggutor",
          "Marowak","Hitmonchan","Lickitung","Rhydon","Chansey","Kangaskhan","Starmie","MrMime","Scyther","Magmar","Electabuzz","Magmar","Jynx","Gyarados","Lapras","Ditto",
          "Vaporeon","Jolteon","Flareon","Porygon","Kabutops","Aerodactyl","Snorlax","Articuno","Zapdos","Moltres","Dragonite", "Mewtwo", "Mew"};
        public static List<string> LoadFilter() {
            if (File.Exists(FilterPath)) {
                var input = File.ReadAllText(FilterPath);
                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Converters.Add(new StringEnumConverter {CamelCaseText = true});
                jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                jsonSettings.DefaultValueHandling = DefaultValueHandling.Populate;
                return JsonConvert.DeserializeObject<List<string>>(input, jsonSettings).
                    GroupBy(x => PokemonParser.ParsePokemon(x)).
                    Select(y => y.FirstOrDefault()).ToList();
            } else {
                var output = JsonConvert.SerializeObject(DefaultPokemonsToFeed, Formatting.Indented,
                new StringEnumConverter { CamelCaseText = true });

                var folder = Path.GetDirectoryName(FilterPath);
                if(folder != null && !Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }
                File.WriteAllText(FilterPath, output);
                return new List<string>();
            }
        }

    }

    public class SettingsToSave
    {
        [DefaultValue(16969)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Port = GlobalSettings.Port;

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsePokeSnipers = GlobalSettings.UsePokeSnipers;

        //public bool UseTrackemon = GlobalSettings.UseTrackemon;
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsePokezz = GlobalSettings.UsePokezz;

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsePokewatchers = GlobalSettings.UsePokewatchers;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string PokeSnipers2Exe = GlobalSettings.PokeSnipers2Exe;

        [DefaultValue(5)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int RemoveAfter = GlobalSettings.RemoveAfter;

        [DefaultValue(30)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int ShowLimit = Math.Max(GlobalSettings.ShowLimit, 1);
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool VerifyOnSkiplagged = GlobalSettings.VerifyOnSkiplagged;

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsePokemonGoIVClub = GlobalSettings.UsePokemonGoIVClub;

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UseFilter = GlobalSettings.UseFilter;

        [DefaultValue("Dark")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string AppTheme = GlobalSettings.AppTheme;

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsServer = GlobalSettings.IsServer;
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsManaged = GlobalSettings.IsManaged;
        [DefaultValue("pogofeeder.live")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string ServerHost = GlobalSettings.ServerHost;
        [DefaultValue(49000)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int ServerPort = GlobalSettings.ServerPort;
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool ShareBotCaptures = GlobalSettings.ShareBotCaptures;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<int> BotWebSocketPorts = GlobalSettings.BotWebSocketPorts;
    }
}
