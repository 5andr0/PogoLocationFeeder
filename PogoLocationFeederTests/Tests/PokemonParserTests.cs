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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Helper.Helper.Tests
{
    [TestClass]
    public class PokemonParserTests
    {
        [TestMethod]
        public void parsePokemonTest()
        {
            testPokemonParsing("kadabra", PokemonId.Kadabra);
            testPokemonParsing("Kadabra", PokemonId.Kadabra);
            testPokemonParsing("abra", PokemonId.Abra);
            testPokemonParsing("Mr Mime", PokemonId.MrMime);
            testPokemonParsing("Mr.Mime", PokemonId.MrMime);
            testPokemonParsing("MrMime", PokemonId.MrMime);
            testPokemonParsing("farfetchd", PokemonId.Farfetchd);
            testPokemonParsing("farfetch'd", PokemonId.Farfetchd);
            testPokemonParsing("Farfetch'd", PokemonId.Farfetchd);
            testPokemonParsing("farfetched", PokemonId.Farfetchd);
            testPokemonParsing("Blastoise", PokemonId.Blastoise);
            testPokemonParsing("qsddqfsfqds", PokemonId.Missingno);
            testPokemonParsing("Kabuto", PokemonId.Kabuto);
            testPokemonParsing("Kabutops", PokemonId.Kabutops);
        }


        [TestMethod]
        public void parsePokemonFullLine()
        {
            testPokemonParsing(
                "[302 seconds remaining] 70% IV - Snorlax at 37.729738754701,-97.372969967814 [ Moveset: ZenHeadbuttFast/HyperBeam ]",
                PokemonId.Snorlax);
            testPokemonParsing(
                "[482 seconds remaining] 73% IV - Wigglytuff at 31.934567351007,-4.4561212903872 [ Moveset: FeintAttackFast/HyperBeam ]",
                PokemonId.Wigglytuff);
            testPokemonParsing("52,6271480914, 13,2858625127 Magneton 90", PokemonId.Magneton);
        }

        private void testPokemonParsing(string text, PokemonId expectedPokemonId)
        {
            Assert.AreEqual(expectedPokemonId, PokemonParser.ParsePokemon(text));
        }
    }
}
