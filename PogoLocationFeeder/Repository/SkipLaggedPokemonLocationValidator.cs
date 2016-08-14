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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.Config;

namespace PogoLocationFeeder.Repository
{
    class SkipLaggedPokemonLocationValidator
    {

        public static List<SniperInfo> FilterNonAvailableAndUpdateMissingPokemonId(List<SniperInfo> sniperInfos)
        {
            if (!GlobalSettings.VerifyOnSkiplagged)
            {
                return sniperInfos;
            }
            var newSniperInfos = new List<SniperInfo>();
            var filteredSniperInfos = SkipLaggedCache.FindUnSentMessages(sniperInfos);
            foreach (var sniperInfo in filteredSniperInfos)
            {
                if (sniperInfo.Verified)
                {
                    newSniperInfos.Add(sniperInfo);
                    continue;
                }
                var scanResult = ScanLocation(new GeoCoordinates(sniperInfo.Latitude, sniperInfo.Longitude));
                if (scanResult.Status == "fail" || scanResult.Status == "error")
                {
                    sniperInfo.Verified = false;
                    newSniperInfos.Add(sniperInfo);
                } else if (scanResult.pokemons != null)
                    {
                    var st = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var t = DateTime.Now.ToUniversalTime() - st;
                    var currentTimestamp = t.TotalMilliseconds;
                    var pokemonsToFeed =  PokemonParser.ParsePokemons(GlobalSettings.PokekomsToFeedFilter);
                    var filteredPokemon = scanResult.pokemons.Where(q => pokemonsToFeed.Contains(PokemonParser.ParseById(q.pokemon_id)));
                    var notExpiredPokemon = filteredPokemon.Where(q => q.expires < currentTimestamp);

                        if (notExpiredPokemon.Any())
                        {
                            foreach (var pokemonLocation in notExpiredPokemon)
                            {
                                SniperInfo newSniperInfo = new SniperInfo();
                                
                                if (sniperInfo.Id.Equals(pokemonLocation.Id))
                                {
                                    newSniperInfo.IV = sniperInfo.IV;
                                }
                                newSniperInfo.Id = PokemonParser.ParseById(pokemonLocation.pokemon_id);
                                newSniperInfo.Latitude = Math.Round(pokemonLocation.latitude, 7);
                                newSniperInfo.Longitude = Math.Round(pokemonLocation.longitude, 7);
                                newSniperInfo.Verified = true;
                                newSniperInfo.ChannelInfo = sniperInfo.ChannelInfo;
                                newSniperInfo.ExpirationTimestamp = FromUnixTime(pokemonLocation.expires);
                                newSniperInfos.Add(newSniperInfo);
                            }
                        }
                        else
                        {
                            Log.Trace($"No snipable pokemon found at {sniperInfo.Latitude.ToString(CultureInfo.InvariantCulture)},{sniperInfo.Longitude.ToString(CultureInfo.InvariantCulture)}");
                        }
                }

            }
            return newSniperInfos;
        }
        public static DateTime FromUnixTime(double unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime).ToLocalTime();
        }

        private static ScanResult ScanLocation(GeoCoordinates location)
        {
           var formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };

            var offset = 0.003;
            // 0.003 = half a mile; maximum 0.06 is 10 miles
            if (offset < 0.001) offset = 0.003;
            if (offset > 0.06) offset = 0.06;

            var boundLowerLeftLat = location.Latitude - offset;
            var boundLowerLeftLng = location.Longitude - offset;
            var boundUpperRightLat = location.Latitude + offset;
            var boundUpperRightLng = location.Longitude + offset;

            var uri =
                $"http://skiplagged.com/api/pokemon.php?bounds={boundLowerLeftLat.ToString(formatter)},{boundLowerLeftLng.ToString(formatter)},{boundUpperRightLat.ToString(formatter)},{boundUpperRightLng.ToString(formatter)}";

            ScanResult scanResult;
            try
            {
                var request = WebRequest.CreateHttp(uri);
                request.UserAgent = UserAgentHelper.GetRandomUseragent();
                request.Accept = "application/json";
                request.Method = "GET";
                request.Timeout = 15000;
                request.ReadWriteTimeout = 32000;

                using (var resp = request.GetResponse())
                {
                    using (var reader = new StreamReader(resp.GetResponseStream()))
                    {
                        var fullresp =
                            reader.ReadToEnd()
                                .Replace(" M", "Male")
                                .Replace(" F", "Female")
                                .Replace("Farfetch'd", "Farfetchd")
                                .Replace("Mr.Maleime", "MrMime");
                        scanResult = JsonConvert.DeserializeObject<ScanResult>(fullresp);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Error querying skiplagged", ex);
                scanResult = new ScanResult
                {
                    Status = "fail",
                    pokemons = new List<PokemonLocation>()
                };
            }
            return scanResult;
        }

        static class SkipLaggedCache
        {
            private const string MessagePrefix = "SkipLaggedCache_";
            private const int MinutesToAddInCache = 15;

            public static List<SniperInfo> FindUnSentMessages(List<SniperInfo> sniperInfos)
            {
                return sniperInfos.Where(sniperInfo => !IsSentAlready(sniperInfo)).ToList();
            }

            private static bool IsSentAlready(SniperInfo sniperInfo)
            {
                var coordinates = GetCoordinatesString(sniperInfo);
                if (MemoryCache.Default.Contains(coordinates))
                {
                    Log.Trace($"Skipping duplicate {sniperInfo}");
                    return true;
                }
                var expirationDate = DateTime.Now.AddMinutes(MinutesToAddInCache);
                MemoryCache.Default.Add(coordinates, sniperInfo, new DateTimeOffset(expirationDate));
                return false;
            }

            private static string GetCoordinatesString(SniperInfo sniperInfo)
            {
                return MessagePrefix + sniperInfo.Latitude.ToString("N5",CultureInfo.InvariantCulture) + ", " + sniperInfo.Longitude.ToString("N5", CultureInfo.InvariantCulture);
            }
        }

        public class ScanResult
        {
            public string Status { get; set; }
            public List<PokemonLocation> pokemons { get; set; }
        }

        public class PokemonLocation
        {
            public PokemonLocation(double lat, double lon)
            {
                latitude = lat;
                longitude = lon;
            }

            public long Id { get; set; }
            public double expires { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public long pokemon_id { get; set; }
            public string pokemon_name { get; set; }

            public bool Equals(PokemonLocation obj)
            {
                return Math.Abs(latitude - obj.latitude) < 0.0001 && Math.Abs(longitude - obj.longitude) < 0.0001;
            }

            public override bool Equals(object obj) // contains calls this here
            {
                var p = obj as PokemonLocation;
                if (p == null) // no cast available
                {
                    return false;
                }

                return Math.Abs(latitude - p.latitude) < 0.0001 && Math.Abs(longitude - p.longitude) < 0.0001;
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public override string ToString()
            {
                return latitude.ToString("0.0000") + ", " + longitude.ToString("0.0000");
            }
        }
    }
}
