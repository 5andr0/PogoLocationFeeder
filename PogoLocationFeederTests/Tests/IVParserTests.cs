using Microsoft.VisualStudio.TestTools.UnitTesting;
using PogoLocationFeeder.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Helper.Tests
{
    [TestClass()]
    public class IVParserTests
    {
        [TestMethod()]
        public void parseIVTest()
        {
            Assert.AreEqual(100, IVParser.parseIV("100 IV"));
            Assert.AreEqual(88.01, IVParser.parseIV("88,01 IV"));
            Assert.AreEqual(5.2, IVParser.parseIV("5.2 %   IV"));

            Assert.AreEqual(15.0, IVParser.parseIV("15 IV"));
            Assert.AreEqual(85.11, IVParser.parseIV("IV 85.11 %"));
            Assert.AreEqual(85.11, IVParser.parseIV("IV 85,11"));

            Assert.AreEqual(100, IVParser.parseIV("100.00 %"));
            Assert.AreEqual(98, IVParser.parseIV("Dratini 97,8.200341 IV 98"));
            Assert.AreEqual(98, IVParser.parseIV("Dratini IV 98 97,8.200341 "));
            Assert.AreEqual(98, IVParser.parseIV("Dratini 98 IV 97,8.200341 "));
        }


        [TestMethod()]
        public void parseNoIV()
        {
            Assert.AreEqual(0, IVParser.parseIV("[239 seconds remaining] Jolteon at 42.877637631245, 74.620142194759[Moveset: ThunderShockFast / Thunderbolt]"));
        }

    }
}