#region using directives

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Microsoft.Win32;
using PogoLocationFeeder.Helper;

#endregion

namespace PoGo.LocationFeeder.Settings {
    public class GlobalSettings {
        public int Port = 16969;
        public bool UsePokeSnipers = false;
        public bool UseTrackemon = false;


        public static bool SniperVisibility => isOneClickSnipeSupported();
        public static bool ThreadPause = false;
        public static GlobalSettings Default => new GlobalSettings();
        public static GlobalSettings Settings;
        public static PogoLocationFeeder.Common.IOutput Output;

        public static GlobalSettings Load() {
            GlobalSettings settings;
            var configFile = Path.Combine(Directory.GetCurrentDirectory(), "Config", "config.json");

            if(File.Exists(configFile)) {
                //if the file exists, load the settings
                var input = File.ReadAllText(configFile);

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                jsonSettings.DefaultValueHandling = DefaultValueHandling.Populate;

                settings = JsonConvert.DeserializeObject<GlobalSettings>(input, jsonSettings);
            } else {
                settings = new GlobalSettings();
            }

            var firstRun = !File.Exists(configFile);
            settings.Save(configFile);

            if(firstRun
                || settings.Port == 0
                ) {
                Log.Error($"Invalid configuration detected. \nPlease edit {configFile} and try again");
                return null;
            }
            return settings;
        }
        public static bool isOneClickSnipeSupported() {
            const string keyName = @"pokesniper2\Shell\Open\Command";
            //return Registry.GetValue(keyName, valueName, null) == null;
            using(RegistryKey Key = Registry.ClassesRoot.OpenSubKey(keyName)) {
                return Key != null;
            }
        }

        public void Save(string fullPath) {
            var output = JsonConvert.SerializeObject(this, Formatting.Indented,
                new StringEnumConverter { CamelCaseText = true });

            var folder = Path.GetDirectoryName(fullPath);
            if(folder != null && !Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(fullPath, output);
        }
    }

}
