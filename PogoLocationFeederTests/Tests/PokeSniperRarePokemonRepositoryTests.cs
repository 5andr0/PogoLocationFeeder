using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PogoLocationFeeder.Repository;

namespace PogoLocationFeederTests.Tests
{
    [TestClass]
    public class PokeSniperReaderTests
    {
        [TestMethod]
        [Ignore]
        //Test is on ignore because it can fail random
        //This still can be used to test if the pokesnipers api works
        public void ReadAllTest()
        {
            var rarePokemonRepository = new PokeSniperRarePokemonRepository(RarePokemonsFactory.createRarePokemonList());
            var sniperInfos = rarePokemonRepository.FindAll();
            Assert.IsNotNull(sniperInfos);
            Assert.IsTrue(sniperInfos.Any());
            sniperInfos.ForEach(sniperInfo => Console.WriteLine(sniperInfo.ToString()));
        }
    }
}