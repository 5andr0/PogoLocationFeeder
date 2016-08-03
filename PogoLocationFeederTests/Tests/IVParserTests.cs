using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PogoLocationFeeder.Helper.Helper.Tests
{
    [TestClass]
    public class IVParserTests
    {
        [TestMethod]
        public void parseIVTest()
        {
            Assert.AreEqual(100, IVParser.ParseIV("100 IV"));
            Assert.AreEqual(88.01, IVParser.ParseIV("88,01 IV"));
            Assert.AreEqual(5.2, IVParser.ParseIV("5.2 %   IV"));

            Assert.AreEqual(15.0, IVParser.ParseIV("15 IV"));
            Assert.AreEqual(85.11, IVParser.ParseIV("IV 85.11 %"));
            Assert.AreEqual(85.11, IVParser.ParseIV("IV 85,11"));

            Assert.AreEqual(100, IVParser.ParseIV("100.00 %"));
            Assert.AreEqual(98, IVParser.ParseIV("Dratini 97,8.200341 IV 98"));
            Assert.AreEqual(98, IVParser.ParseIV("Dratini IV 98 97,8.200341 "));
            Assert.AreEqual(98, IVParser.ParseIV("Dratini 98 IV 97,8.200341 "));
        }


        [TestMethod]
        public void parseNoIV()
        {
            Assert.AreEqual(0,
                IVParser.ParseIV(
                    "[239 seconds remaining] Jolteon at 42.877637631245, 74.620142194759[Moveset: ThunderShockFast / Thunderbolt]"));
        }
    }
}