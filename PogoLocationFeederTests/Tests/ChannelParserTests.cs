using Microsoft.VisualStudio.TestTools.UnitTesting;
using PogoLocationFeeder.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PogoLocationFeederTests.Tests
{
    [TestClass()]
    public class ChannelParserTests
    {
        [TestMethod()]
        public void ToChannelInfoTest()
        {
            var channelParser = new ChannelParser();
            channelParser.LoadChannelSettings();
            var channelInfo = channelParser.ToChannelInfo("207998375251935232");
            Assert.IsNotNull(channelInfo);
            Assert.IsTrue(channelInfo.isValid);
            Assert.AreEqual("PokeMobBot - a Pokémon Go Bot", channelInfo.server);
            Assert.AreEqual("sniper",channelInfo.channel);
        }

        [TestMethod()]
        public void InvalidToChannelInfoTest()
        {
            var channelParser = new ChannelParser();
            channelParser.LoadChannelSettings();
            var channelInfo = channelParser.ToChannelInfo("207998375251935232ddqsd");
            Assert.IsNotNull(channelInfo);
            Assert.IsFalse(channelInfo.isValid);
            Assert.AreEqual("Unknown", channelInfo.server);
            Assert.AreEqual("Unknown", channelInfo.channel);
        }
    }
}