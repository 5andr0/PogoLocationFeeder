using Microsoft.VisualStudio.TestTools.UnitTesting;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Helper.Tests
{
    [TestClass()]
    public class MessageParserTests
    {
        MessageParser messageParser = new MessageParser();

        [TestMethod()]
        public void parseMessageTest()
        {
            
            verifyParsing("[239 seconds remaining] 52% IV - Jolteon at 42.877637631245,74.620142194759 [ Moveset: ThunderShockFast/Thunderbolt ]",
                42.877637631245, 74.620142194759, PokemonId.Jolteon, 52, DateTime.Now.AddSeconds(239));
            verifyParsing("[239 seconds remaining] Jolteon at 42.877637631245,74.620142194759 [ Moveset: ThunderShockFast/Thunderbolt ]",
    42.877637631245, 74.620142194759, PokemonId.Jolteon, 0, DateTime.Now.AddSeconds(239));


        }

        private void verifyParsing(String text, double latitude, double longitude, PokemonId pokemonId, double iv, DateTime expiration)
        {
            List<SniperInfo> sniperInfo = messageParser.parseMessage(text);
            Assert.IsNotNull(sniperInfo);
            Assert.AreEqual(pokemonId, sniperInfo[0].id);
            Assert.AreEqual(latitude, sniperInfo[0].latitude);
            Assert.AreEqual(longitude, sniperInfo[0].longitude);
            Assert.AreEqual(iv, sniperInfo[0].iv);
            Assert.AreEqual(Truncate(expiration, TimeSpan.FromSeconds(1)), Truncate(sniperInfo[0].timeStamp, TimeSpan.FromSeconds(1)));

        }

        private static DateTime Truncate(DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }

    }

}