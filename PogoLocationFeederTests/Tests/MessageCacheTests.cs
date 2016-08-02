using Microsoft.VisualStudio.TestTools.UnitTesting;
using PogoLocationFeeder.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Helper.Tests
{
    [TestClass()]
    public class MessageCacheTests
    {

        [TestMethod()]
        public void findUnSentMessagesTest()
        {
            MessageCache messageCache = new MessageCache();
            SniperInfo sniperInfo =  new SniperInfo();
            sniperInfo.Latitude = 1;
            sniperInfo.Longitude = 2;
            sniperInfo.ExpirationTimestamp = DateTime.Now.AddMilliseconds(100);

            SniperInfo sniperInfo2= new SniperInfo();
            sniperInfo2.Latitude = 1;
            sniperInfo2.Longitude = 2;

            SniperInfo differntSniperInfo = new SniperInfo();
            differntSniperInfo.Latitude = 4;
            differntSniperInfo.Longitude = 5;
            differntSniperInfo.ExpirationTimestamp = DateTime.Now.AddMilliseconds(100);

            List<SniperInfo> unsentMessages = messageCache.findUnSentMessages(new List<SniperInfo>() { sniperInfo });
            Assert.IsNotNull(unsentMessages);
            Assert.AreEqual(1, unsentMessages.Count);

            unsentMessages = messageCache.findUnSentMessages(new List<SniperInfo>() { sniperInfo2 });
            Assert.IsNotNull(unsentMessages);
            Assert.AreEqual(0, unsentMessages.Count);

            unsentMessages = messageCache.findUnSentMessages(new List<SniperInfo>() { differntSniperInfo });
            Assert.IsNotNull(unsentMessages);
            Assert.AreEqual(1, unsentMessages.Count);

            Thread.Sleep(110);

            unsentMessages = messageCache.findUnSentMessages(new List<SniperInfo>() { sniperInfo2 });
            Assert.IsNotNull(unsentMessages);
            Assert.AreEqual(1, unsentMessages.Count);
        }
    }
}