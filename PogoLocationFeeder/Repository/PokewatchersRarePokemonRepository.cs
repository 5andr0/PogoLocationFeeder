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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;

namespace PogoLocationFeeder.Repository
{
    public class PokeWatchersRarePokemonRepository : IRarePokemonRepository
    {
        //private const int timeout = 20000;

        private const string URL = "https://pokewatchers.com/grab/";
        public const string Channel = "PokeWatchers";

        public PokeWatchersRarePokemonRepository()
        {
        }

        public List<SniperInfo> FindAll()
        {
            try
            {
                var userAgent = UserAgentHelper.GetRandomUseragent();
                var content = getContent(userAgent);
                var cookie = CreateCookie(content);
                if (cookie != null)
                {
                    content = getContent(userAgent, cookie);
                    return GetJsonList(content);
                }
                else
                {
                    Log.Debug("Could find a cookie for PokeWatchers");
                }
            }
            catch (Exception e)
            {
                Log.Debug("Pokewatchers API error: {0}", e.Message);
            }
            return null;
        }

        private string CreateCookie(string content)
        {
            var match = Regex.Match(content, @"<script>(1?.*)<\/script>");
            if (match.Success)
            {
                var script = match.Groups[1].Value;
                var replace = Regex.Replace(script, @"e\(r\)",
                    @"e(r.replace('document.cookie', 'F' ).replace('location.reload();', '' ))");
                replace += "WScript.Echo (F);";
                var tempFileName = Path.GetTempPath() + $"{DateTime.Now.Millisecond}_pokefeeder.js";

                using (StreamWriter sw = new StreamWriter(tempFileName))
                {
                    sw.WriteLine(replace);
                }
                var cookieText = ExecuteAndRead(tempFileName);
                return cookieText;
            }
            return null;
        }

        private string getContent(string userAgent, string cookieText = null)
        {
            var request = WebRequest.CreateHttp(URL);

            request.UserAgent = userAgent;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.Method = "GET";
            request.Timeout = 15000;
            request.Host = "pokewatchers.com";
            request.ReadWriteTimeout = 32000;

            if (cookieText != null)
            {
                cookieText = cookieText.Replace("\r", "").Replace("\n", "");
                var cookieMonster = new CookieContainer();
                var cookies = cookieText.Split(';');
                foreach (var cookie in cookies)
                {
                    var matcher = Regex.Match(cookie, @"(1?sucuri.*)\s?=\s?(2?.*)");
                    if (matcher.Success)
                    {
                        cookieMonster.Add(new Cookie(matcher.Groups[1].Value,
                            matcher.Groups[2].Value,
                            "/",
                            "pokewatchers.com"));
                    }
                }
                request.CookieContainer = cookieMonster;
            }
            using (var resp = request.GetResponse())
            {
                using (var reader = new StreamReader(resp.GetResponseStream()))
                {
                    var content = reader.ReadToEnd();
                    return content;
                }
            }
        }

        private string ExecuteAndRead(string filePath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            Process p = new Process();

            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;

            startInfo.UseShellExecute = false;
            startInfo.Arguments = $"//NoLogo {filePath}";
            startInfo.FileName = "Cscript";

            p.StartInfo = startInfo;
            p.Start();

            p.WaitForExit();
            var output = p.StandardOutput.ReadToEnd();

            return output;
        }

        private List<SniperInfo> GetJsonList(string reader)
        {
            var results = JsonConvert.DeserializeObject<List<PokewatchersResult>>(reader, new JsonSerializerSettingsCultureInvariant());
            var list = new List<SniperInfo>();
            foreach (var result in results)
            {
                var sniperInfo = Map(result);
                if (sniperInfo != null)
                {
                    list.Add(sniperInfo);
                }
            }
            return list;
        }

        private SniperInfo Map(PokewatchersResult result)
        {
            var sniperInfo = new SniperInfo();
            var pokemonId = PokemonParser.ParsePokemon(result.name);
            sniperInfo.Id = pokemonId;
            var geoCoordinates = GeoCoordinatesParser.ParseGeoCoordinates(result.coords);
            if (geoCoordinates == null)
            {
                return null;
            }
            sniperInfo.Latitude = Math.Round(geoCoordinates.Latitude, 7);
            sniperInfo.Longitude = Math.Round(geoCoordinates.Longitude, 7);

            var untilTime = DateTime.Now.AddTicks(result.until);
            sniperInfo.ExpirationTimestamp = untilTime;
            sniperInfo.ChannelInfo = new ChannelInfo { server = Channel };

            return sniperInfo;
        }
    }


    internal class PokewatchersResult
    {

        [JsonProperty("pokemon")]
        public string name { get; set; }

        [JsonProperty("cords")]
        public string coords { get; set; }

        [JsonProperty("timeend")]
        public long until { get; set; }

        [JsonProperty("icon")]
        public string icon { get; set; }
    }

}
