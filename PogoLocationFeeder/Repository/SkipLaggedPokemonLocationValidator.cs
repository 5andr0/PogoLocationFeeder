using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using PogoLocationFeeder.Config;

namespace PogoLocationFeeder.Repository
{
    class SkipLaggedPokemonLocationValidator
    {

        public static List<SniperInfo> FilterNonAvailableAndUpdateMissingPokemonId(List<SniperInfo> sniperInfos)
        {
            if (!GlobalSettings.VerifyOnSkiplagged)
            {
                return sniperInfos;;
            }
            var newSniperInfos = new List<SniperInfo>();
            foreach (var sniperInfo in sniperInfos)
            {
                var scanResult = ScanLocation(new GeoCoordinates(sniperInfo.Latitude, sniperInfo.Longitude));
                if (scanResult.Status == "fail")
                {
                    sniperInfo.Verified = false;
                } else if (scanResult.pokemons != null)
                    {
                    var st = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var t = DateTime.Now.ToUniversalTime() - st;
                    var currentTimestamp = t.TotalMilliseconds;
                    var pokemonsToFeed =  PokemonParser.ParsePokemons(GlobalSettings.PokekomsToFeedFilter);
                    var filteredPokemon = scanResult.pokemons.Where(q => pokemonsToFeed.Contains(q.pokemon_name));
                    var notExpiredPokemon = filteredPokemon.Where(q => q.expires < currentTimestamp);

                        if (notExpiredPokemon.Any())
                        {
                            foreach (var pokemonLocation in notExpiredPokemon)
                            {
                                SniperInfo newSniperInfo = new SniperInfo();
                                if (sniperInfo.Id.Equals(pokemonLocation.pokemon_name))
                                {
                                    newSniperInfo.IV = sniperInfo.IV;
                                }
                                newSniperInfo.Id = pokemonLocation.pokemon_name;
                                newSniperInfo.Latitude = pokemonLocation.latitude;
                                newSniperInfo.Longitude = pokemonLocation.longitude;
                                newSniperInfo.Verified = true;
                                newSniperInfo.ExpirationTimestamp = FromUnixTime(pokemonLocation.expires);
                                newSniperInfos.Add(newSniperInfo);
                            }
                        }
                        else
                        {
                            Log.Debug($"No snipable pokemon found at {sniperInfo.Latitude.ToString(CultureInfo.InvariantCulture)},{sniperInfo.Longitude.ToString(CultureInfo.InvariantCulture)}");
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

            var offset = 0.001;
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
                request.Accept = "application/json";
                request.UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36\r\n";
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
            public int pokemon_id { get; set; }
            public PokemonId pokemon_name { get; set; }

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
