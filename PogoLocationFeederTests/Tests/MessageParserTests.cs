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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Helper.Helper.Tests
{
    [TestClass]
    public class MessageParserTests
    {

        [TestMethod]
        public void ParseMessageTest()
        {
            verifyParsing(
                "[239 seconds remaining] 52% IV - Jolteon at 42.877637631245,74.620142194759 [ Moveset: ThunderShockFast/Thunderbolt ]",
                42.877638, 74.620142, PokemonId.Jolteon, 52, DateTime.Now.AddSeconds(239));
            verifyParsing(
                "[239 seconds remaining] Jolteon at 42.877637631245,74.620142194759 [ Moveset: ThunderShockFast/Thunderbolt ]",
                42.877638, 74.620142, PokemonId.Jolteon, 0, DateTime.Now.AddSeconds(239));
            verifyParsing(
                "Dratini 42.326919, -83.042221 IV91 confirmed",
                42.326919, -83.042221, PokemonId.Dratini, 91, DateTime.MinValue);
            verifyParsing("pokesniper2://dragonair/37.598656,140.42916", 37.598656, 140.42916, PokemonId.Dragonair, 0, default(DateTime))
            ;
        }

        [TestMethod]
        public void TestSpaces()
        {
            verifyParsing("Dratini  42.12345 -40.123456", 42.12345, -40.123456, PokemonId.Dratini, 0, DateTime.MinValue);
        }

        [TestMethod]
        public void Test100Pr()
        {
           verifyParsing("Poliwag IV: 100.00 % 40.780349, -73.951366", 40.780349, -73.951366, PokemonId.Poliwag, 100, DateTime.MinValue);
        }

        [TestMethod]
        public void TestUnparsed()
        {
            verifyParsing("Dragonite; 42.324363,-71.103955 74 % lvl3 252 / 260cp", 42.324363, -71.103955,
                PokemonId.Dragonite, 74, DateTime.MinValue);
        }

        [TestMethod]
        public void TestNewLines()
        {
            verifyParsing("Vaporeon\r-33.804627,151.252824", -33.804627, 151.252824, PokemonId.Vaporeon, 0,
                DateTime.MinValue);
            verifyParsing("Vaporeon\n-33.804627,151.252824", -33.804627, 151.252824, PokemonId.Vaporeon, 0,
            DateTime.MinValue);
                    verifyParsing("Vaporeon\r\n-33.804627,151.252824", -33.804627, 151.252824, PokemonId.Vaporeon, 0,
            DateTime.MinValue);
                    verifyParsing("Vaporeon\n\r-33.804627,151.252824", -33.804627, 151.252824, PokemonId.Vaporeon, 0,
        DateTime.MinValue);
        }

        private void verifyParsing(string text, double latitude, double longitude, PokemonId pokemonId, double iv,
            DateTime expiration)
        {
            var sniperInfo = MessageParser.ParseMessage(text);
            Assert.IsNotNull(sniperInfo);
            Assert.AreEqual(pokemonId, sniperInfo[0].Id);
            Assert.AreEqual(latitude, sniperInfo[0].Latitude);
            Assert.AreEqual(longitude, sniperInfo[0].Longitude);
            Assert.AreEqual(iv, sniperInfo[0].IV);
            Assert.AreEqual(Truncate(expiration, TimeSpan.FromSeconds(1)),
                Truncate(sniperInfo[0].ExpirationTimestamp, TimeSpan.FromSeconds(1)));
        }

        private static DateTime Truncate(DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            return dateTime.AddTicks(-(dateTime.Ticks%timeSpan.Ticks));
        }
    }
}
