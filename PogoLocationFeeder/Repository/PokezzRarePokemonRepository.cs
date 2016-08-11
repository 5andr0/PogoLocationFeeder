using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using WebSocket4Net;

namespace PogoLocationFeeder.Repository
{
    public class PokezzRarePokemonRepository : IRarePokemonRepository
    {
        private const string URL = "ws://pokezz.com/socket.io/?EIO=3&transport=websocket";
        private const string Channel = "PokeZZ";
        private const int Timeout = 5000;

        public PokezzRarePokemonRepository()
        {
        }

        public List<SniperInfo> FindAll()
        {
            List<SniperInfo> newSniperInfos = new List<SniperInfo>();
            try
            {
                using (var client = new WebSocket(URL, "basic", WebSocketVersion.Rfc6455))
                {
                    client.MessageReceived += (s, e) =>
                    {
                        var message = e.Message;
                        var match = Regex.Match(message, @"^(1?\d+)\[""[a|b]"",""(2?.*)""\]$");
                        if (match.Success)
                        {
                            if (match.Groups[1].Value == "42")
                            {
                                var sniperInfos = Parse(match.Groups[2].Value);
                                if (sniperInfos != null && sniperInfos.Any())
                                {
                                    newSniperInfos.AddRange(sniperInfos);
                                }
                            }
                        }
                    };
                    client.Open();
                    Thread.Sleep(Timeout);
                    client.Close();
                }
            }
            catch (Exception e)
            {
                Log.Warn("Received error from Pokezz. More info the logs");
                Log.Debug("Received error from Pokezz: ", e);

            }
            return newSniperInfos;
        }

        public string GetChannel()
        {
            return Channel;
        }


        private static List<SniperInfo> Parse(string reader)
        {
            var lines = reader.Split('~');
            var list = new List<SniperInfo>();

            foreach (var line in lines)
            {
                var sniperInfo = ParseLine(line);
                if (sniperInfo != null)
                {
                    list.Add(sniperInfo);
                }
            }
            return list;
        }

        private static SniperInfo ParseLine(string line)
        {
            var match = Regex.Match(line,
                @"(?<id>\d+)\|(?<lat>\-?\d+[\,|\.]\d+)\|(?<lon>\-?\d+[\,|\.]\d+)\|(?<expires>\d+)\|(?<verified>[1|0])\|\|");
            if (match.Success)
            {
                var sniperInfo = new SniperInfo();
                var pokemonId = PokemonParser.ParseById(Convert.ToInt64(match.Groups["id"].Value));
                sniperInfo.Id = pokemonId;
                var lat = Convert.ToDouble(match.Groups["lat"].Value, CultureInfo.InvariantCulture);
                var lon = Convert.ToDouble(match.Groups["lon"].Value, CultureInfo.InvariantCulture);

                sniperInfo.Latitude = lat;
                sniperInfo.Longitude = lon;

                var expires = Convert.ToInt64(match.Groups["expires"].Value);
                if (expires != default(long))
                {
                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var untilTime = epoch.AddSeconds(expires).ToLocalTime();
                    if (untilTime < DateTime.Now)
                    {
                        return null;
                    }
                    sniperInfo.ExpirationTimestamp = untilTime;
                }
                return sniperInfo;
            }
            return null;
        }
    }
}