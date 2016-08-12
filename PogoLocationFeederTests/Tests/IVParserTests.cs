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

        }


    [TestMethod]
        public void parseNoIV()
        {
            Assert.AreEqual(91, IVParser.ParseIV("209171120702619648: Vaporeon 40.736749, -74.010540 IV91 off"));

            Assert.AreEqual(0,
                IVParser.ParseIV(
                    "[239 seconds remaining] Jolteon at 42.877637631245, 74.620142194759[Moveset: ThunderShockFast / Thunderbolt]"));
        }
    }
}
