using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Helper.Helper.Tests
{
    [TestClass]
    public class MessageParserTests
    {
        private readonly MessageParser messageParser = new MessageParser();

        [TestMethod]
        public void parseMessageTest()
        {
            verifyParsing(
                "[239 seconds remaining] 52% IV - Jolteon at 42.877637631245,74.620142194759 [ Moveset: ThunderShockFast/Thunderbolt ]",
                42.877637631245, 74.620142194759, PokemonId.Jolteon, 52, DateTime.Now.AddSeconds(239));
            verifyParsing(
                "[239 seconds remaining] Jolteon at 42.877637631245,74.620142194759 [ Moveset: ThunderShockFast/Thunderbolt ]",
                42.877637631245, 74.620142194759, PokemonId.Jolteon, 0, DateTime.Now.AddSeconds(239));
        }

        private void verifyParsing(string text, double latitude, double longitude, PokemonId pokemonId, double iv,
            DateTime expiration)
        {
            var sniperInfo = messageParser.parseMessage(text);
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