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
                48.880245472813, 2.3258381797353);
        }

        [TestMethod]
        public void TestCoordinatesRandomPerson1()
        {
            testCoordinates("50.846499257055854,4.421932697296143 Lapras 11min 92IV Frosth Breath Dragon Pulse",
                50.846499257055854, 4.421932697296143);
        }

        [TestMethod]
        public void TestCoordinatesRandomPerson2()
        {
            testCoordinates("34.0392682838917,-118.494653181811, Eevee, 10min", 34.0392682838917, -118.494653181811);
        }

        [TestMethod]
        public void TestCoordinatesPokeSniper()
        {
            testCoordinates("-33.8304880738,151.087396206", -33.8304880738, 151.087396206);
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