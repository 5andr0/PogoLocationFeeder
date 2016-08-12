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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.Models;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;

namespace PogoLocationFeeder.GUI.Common {
    public static class PokemonFilter {
        public static List<PokemonFilterModel> GetPokemons() {
            var pokes = new List<PokemonFilterModel>();
            foreach (var poke in Enum.GetValues(typeof(PokemonId))) {
                var id = (PokemonId) poke;
                var img = new BitmapImage(
                    new Uri(
                        $"pack://application:,,,/PogoLocationFeeder.GUI;component/Assets/icons/{(int) id}.png",
                        UriKind.Absolute));
                img.Freeze();
                pokes.Add(new PokemonFilterModel(id, img));
            }
            return pokes;
        }

        public static string ConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "Config", "filter.json");

        public static void Save() {
            var list = new List<string>();
            foreach (var pokemonFilterModel in GlobalVariables.PokemonToFeedFilterInternal) {
                list.Add(pokemonFilterModel.Id.ToString());
            }
            var output = JsonConvert.SerializeObject(list, Formatting.Indented,
                new StringEnumConverter {CamelCaseText = true});

            var folder = Path.GetDirectoryName(ConfigFile);
            if (folder != null && !Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
            File.WriteAllText(ConfigFile, output);
        }

        public static void Load() {
            if (!File.Exists(ConfigFile)) { 
                var output = JsonConvert.SerializeObject(GlobalSettings.DefaultPokemonsToFeed, Formatting.Indented,
                    new StringEnumConverter {CamelCaseText = true});

                var folder = Path.GetDirectoryName(ConfigFile);
                if (folder != null && !Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }
                File.WriteAllText(ConfigFile, output);
            }
            GlobalSettings.PokekomsToFeedFilter = GlobalSettings.LoadFilter();
            var set = GlobalSettings.PokekomsToFeedFilter.OrderBy(x => PokemonParser.ParsePokemon(x));

            foreach (var s in set) {
                try
                {
                    var id = PokemonParser.ParsePokemon(s, false);
                    var img = new BitmapImage(
                    new Uri(
                        $"pack://application:,,,/PogoLocationFeeder.GUI;component/Assets/icons/{(int)id}.png",
                        UriKind.Absolute));
                    img.Freeze();
                    GlobalVariables.PokemonToFeedFilterInternal.Add(new PokemonFilterModel(id, img));
                    GlobalVariables.AllPokemonsInternal.Remove(GlobalVariables.AllPokemonsInternal.Single(x => x.Id == id));
                }
                catch (Exception e)
                {
                    Log.Warn("Could not add pokemon to the filter" , e);
                }
            }
        }
    }

}
