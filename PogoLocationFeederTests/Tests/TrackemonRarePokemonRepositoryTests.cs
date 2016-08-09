using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PogoLocationFeeder.Repository;

namespace PogoLocationFeeder.Helper.Helper.Repository.Tests
{
    [TestClass]
    public class TrackemonRarePokemonRepositoryTests
    {
        private readonly IRarePokemonRepository trackemonRarePokemonRepository =
            new TrackemonRarePokemonRepository();

        [TestMethod]
        [Ignore]
        public void FindAll()
        {
            var sniperInfos = trackemonRarePokemonRepository.FindAll();
            Assert.IsNotNull(sniperInfos);
            Assert.IsTrue(sniperInfos.Any());
            foreach (var sniperInfo in sniperInfos)
            {
                Console.WriteLine(sniperInfo);
            }
        }

        [TestMethod]
        public void GetChannel()
        {
            Assert.AreEqual("Trackemon", trackemonRarePokemonRepository.GetChannel());
        }
    }
}