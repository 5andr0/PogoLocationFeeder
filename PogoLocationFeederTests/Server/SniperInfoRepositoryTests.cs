using Microsoft.VisualStudio.TestTools.UnitTesting;
using PogoLocationFeeder.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Server.Tests
{
    [TestClass()]
    public class SniperInfoRepositoryTests
    {


        [TestMethod()]
        public void FindTest()
        {
            var sniperInfoRepository = new SniperInfoRepository();
            sniperInfoRepository.Increase(new SniperInfo() {Latitude = 48.830168, Longitude = 2.316475, Id = PokemonId.Poliwrath});
            var oldSniperInfo = sniperInfoRepository.Find(new SniperInfo()
            {
                Latitude = 48.830169,
                Longitude = 2.316475,
                Id = PokemonId.Poliwrath
            });

            Assert.IsNotNull(oldSniperInfo);


        }

        [TestMethod()]
        public void FindTest2()
        {
            var sniperInfoRepository = new SniperInfoRepository();
            sniperInfoRepository.Increase(new SniperInfo() { Latitude = 40.762857, Longitude = -73.950447, Id = PokemonId.Vaporeon });
            var oldSniperInfo = sniperInfoRepository.Find(new SniperInfo()
            {
                Latitude = 40.762858,
                Longitude = -73.950448,
                Id = PokemonId.Vaporeon
            });

            Assert.IsNotNull(oldSniperInfo);
        }

        [TestMethod()]
        public void FindTest3()
        {
            var sniperInfoRepository = new SniperInfoRepository();
            sniperInfoRepository.Increase(new SniperInfo() { Latitude = 40.762401, Longitude = -73.950447, Id = PokemonId.Vaporeon });
            var oldSniperInfo = sniperInfoRepository.Find(new SniperInfo()
            {
                Latitude = 40.762858,
                Longitude = -73.950448,
                Id = PokemonId.Vaporeon
            });

            Assert.IsNull(oldSniperInfo);
        }
    }
}