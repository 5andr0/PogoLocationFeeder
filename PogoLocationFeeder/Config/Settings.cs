#region using directives

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PoGo.LocationFeeder.LogicSettings;

#endregion

namespace PoGo.LocationFeeder.Settings
{
    public class GlobalSettings
    {
        public ulong ServerId = 0;
        public string ServerChannel = "";
        public string DiscordToken = "";
        public int Port = 0;
        public bool useToken = false;
        public string DiscordUser = "";
        public string DiscordPassword = "";

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
                || settings.ServerId == 0 
                || settings.Port == 0 
                || string.IsNullOrEmpty(settings.ServerChannel) 
                || string.IsNullOrEmpty(settings.DiscordToken)
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

    public class LogicSettings : ILogicSettings
    {
        private readonly GlobalSettings _settings;

        public ulong ServerId => _settings.ServerId;
        public string ServerChannel => _settings.ServerChannel;
        public string DiscordToken => _settings.DiscordToken;
        public int Port => _settings.Port;
    }
}
