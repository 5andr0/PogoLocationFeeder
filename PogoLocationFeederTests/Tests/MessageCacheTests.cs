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
            var messageCache = new MessageCache();
            var sniperInfo = new SniperInfo
            {
                Latitude = 1,
                Longitude = 2,
                ExpirationTimestamp = DateTime.Now.AddMilliseconds(100)
            };

            var sniperInfo2 = new SniperInfo
            {
                Latitude = 1,
                Longitude = 2
            };

            var differntSniperInfo = new SniperInfo
            {
                Latitude = 4,
                Longitude = 5,
                ExpirationTimestamp = DateTime.Now.AddMilliseconds(100)
            };

            var unsentMessages = messageCache.FindUnSentMessages(new List<SniperInfo> {sniperInfo});
            Assert.IsNotNull(unsentMessages);
            Assert.AreEqual(1, unsentMessages.Count);

            unsentMessages = messageCache.FindUnSentMessages(new List<SniperInfo> {sniperInfo2});
            Assert.IsNotNull(unsentMessages);
            Assert.AreEqual(0, unsentMessages.Count);

            unsentMessages = messageCache.FindUnSentMessages(new List<SniperInfo> {differntSniperInfo});
            Assert.IsNotNull(unsentMessages);
            Assert.AreEqual(1, unsentMessages.Count);

            Thread.Sleep(110);

            unsentMessages = messageCache.FindUnSentMessages(new List<SniperInfo> {sniperInfo2});
            Assert.IsNotNull(unsentMessages);
            Assert.AreEqual(1, unsentMessages.Count);
        }
    }
}