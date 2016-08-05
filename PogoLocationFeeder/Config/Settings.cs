#region using directives

using System;
using System.IO;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Helper;

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
        public static bool UsePokeSnipers = false;
        public static bool UseTrackemon = false;
        public static string PokeSnipers2Exe = "";
        public static int RemoveAfter = 15;
        public static int ShowLimit = 30;

        public static bool SniperVisibility => IsOneClickSnipeSupported();
        public static GlobalSettings Default => new GlobalSettings();
        public static string ConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "Config", "config.json");


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
                UseTrackemon = set.UseTrackemon;
                UsePokeSnipers = set.UsePokeSnipers;
                RemoveAfter = set.RemoveAfter;
                ShowLimit = Math.Max(set.ShowLimit, 1);
                PokeSnipers2Exe = set.PokeSnipers2Exe;
            }
            else
            {
                settings = new GlobalSettings();
            }

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
            File.WriteAllText(ConfigFile, output);
        }

    }

    public class SettingsToSave {
        public int Port = GlobalSettings.Port;
        public bool UsePokeSnipers = GlobalSettings.UsePokeSnipers;
        public bool UseTrackemon = GlobalSettings.UseTrackemon;
        public string PokeSnipers2Exe = GlobalSettings.PokeSnipers2Exe;
        public int RemoveAfter = GlobalSettings.RemoveAfter;
        public int ShowLimit = Math.Max(GlobalSettings.ShowLimit, 1);

    }
}