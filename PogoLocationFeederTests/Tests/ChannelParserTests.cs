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
