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
            sniperInfo.latitude = 1;
            sniperInfo.longitude = 2;
            sniperInfo.timeStamp = DateTime.Now.AddMilliseconds(100);

            SniperInfo sniperInfo2= new SniperInfo();
            sniperInfo2.latitude = 1;
            sniperInfo2.longitude = 2;

            SniperInfo differntSniperInfo = new SniperInfo();
            differntSniperInfo.latitude = 4;
            differntSniperInfo.longitude = 5;
            differntSniperInfo.timeStamp = DateTime.Now.AddMilliseconds(100);

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