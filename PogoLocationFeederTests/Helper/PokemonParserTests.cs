using Microsoft.VisualStudio.TestTools.UnitTesting;
using PogoLocationFeeder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Tests
{
    [TestClass()]
    public class PokemonParserTests
    {
        [TestMethod()]
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
            testPokemonParsing("farfetched", PokemonId.Farfetchd);
            testPokemonParsing("Blastoise", PokemonId.Blastoise);
            testPokemonParsing("qsddqfsfqds", PokemonId.Missingno);
        }


        [TestMethod()]
        public void parsePokemonFullLine()
        {
            testPokemonParsing("[302 seconds remaining] 70% IV - Snorlax at 37.729738754701,-97.372969967814 [ Moveset: ZenHeadbuttFast/HyperBeam ]", PokemonId.Snorlax);
            testPokemonParsing("[482 seconds remaining] 73% IV - Wigglytuff at 31.934567351007,-4.4561212903872 [ Moveset: FeintAttackFast/HyperBeam ]", PokemonId.Wigglytuff);
            testPokemonParsing("52,6271480914, 13,2858625127 Magneton 90", PokemonId.Magneton);
        }

        private void testPokemonParsing(String text, PokemonId expectedPokemonId)
        {
            Assert.AreEqual(expectedPokemonId, PokemonParser.parsePokemon(text));
        }
    }
}