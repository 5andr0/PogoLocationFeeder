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
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PogoLocationFeeder.Helper;

namespace PogoLocationFeederTests.Tests
{
    [TestClass]
    public class MessageCacheTests
    {
        [TestMethod]
        public void FindUnSentMessagesTest()
         {
            var messageCache = MessageCache.Instance;
            var sniperInfo = new SniperInfo
            {
                Latitude = 1,
                Longitude = 2,
                ExpirationTimestamp = DateTime.Now.AddMilliseconds(100),
                ReceivedTimeStamp = DateTime.Now
            };
            var sniperInfo2 = new SniperInfo
            {
                Latitude = 1,
                Longitude = 2,
                ReceivedTimeStamp = DateTime.Now
            };

            var differntSniperInfo = new SniperInfo
            {
                Latitude = 4,
                Longitude = 5,
                ExpirationTimestamp = DateTime.Now.AddMilliseconds(100),
                ReceivedTimeStamp = DateTime.Now
            };

            var unsentMessages = messageCache.FindUnSentMessages(new List<SniperInfo> {sniperInfo});
            Assert.IsNotNull(unsentMessages);
            Assert.AreEqual(1, unsentMessages.Count);
            Assert.AreEqual(1, MessageCache.Instance._clientRepository.Count());

            unsentMessages = messageCache.FindUnSentMessages(new List<SniperInfo> {sniperInfo2});
            Assert.IsNotNull(unsentMessages);
            Assert.AreEqual(0, unsentMessages.Count);
            Assert.AreEqual(1, MessageCache.Instance._clientRepository.Count());

            unsentMessages = messageCache.FindUnSentMessages(new List<SniperInfo> {differntSniperInfo});
            Assert.IsNotNull(unsentMessages);
            Assert.AreEqual(1, unsentMessages.Count);
            Assert.AreEqual(2, MessageCache.Instance._clientRepository.Count());

            Thread.Sleep(200);
            Assert.AreEqual(0, MessageCache.Instance._clientRepository.Count());

            unsentMessages = messageCache.FindUnSentMessages(new List<SniperInfo> {sniperInfo2});
            Assert.IsNotNull(unsentMessages);
            Assert.AreEqual(1, unsentMessages.Count);
        }
    }
}
