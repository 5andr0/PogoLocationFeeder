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

namespace PogoLocationFeederTests
{
    [TestClass]
    public class GeoCoordinatesParserTest
    {
        [TestMethod]
        public void TestCoordinatesCoordBot()
        {
            testCoordinates(
                "[192 seconds remaining] 55% IV - Porygon at 48.880245472813,2.3258381797353 [Moveset: TackleFast/Psybeam]",
                48.880245, 2.325838);
        }

        [TestMethod]
        public void TestCoordinatesRandomPerson1()
        {
            testCoordinates("50.846499257055854,4.421932697296143 Lapras 11min 92IV Frosth Breath Dragon Pulse",
                50.846499, 4.421933);
        }

        [TestMethod]
        public void TestCoordinatesRandomPerson2()
        {
            testCoordinates("34.0392682838917,-118.494653181811, Eevee, 10min", 34.039268, -118.494653);
        }

        [TestMethod]
        public void TestCoordinatesPokeSniper()
        {
            testCoordinates("-33.8304880738,151.087396206", -33.830488, 151.087396);
        }

        [TestMethod]
        public void TestInvalidCoordinates()
        {
            Assert.IsNull(GeoCoordinatesParser.ParseGeoCoordinates("181.6969696969696,-1"));
            Assert.IsNull(GeoCoordinatesParser.ParseGeoCoordinates("-181.6969696969696,-1"));
            Assert.IsNull(GeoCoordinatesParser.ParseGeoCoordinates("69.6969696969696,-420"));
            Assert.IsNull(GeoCoordinatesParser.ParseGeoCoordinates("69.6969696969696,420"));
        }

        private void testCoordinates(string text, double expectedLatitude, double expectedLongitude)
        {
            var geoCoordinates = GeoCoordinatesParser.ParseGeoCoordinates(text);
            Assert.IsNotNull(geoCoordinates);
            Assert.AreEqual(expectedLatitude, geoCoordinates.Latitude);
            Assert.AreEqual(expectedLongitude, geoCoordinates.Longitude);
        }
    }
}
