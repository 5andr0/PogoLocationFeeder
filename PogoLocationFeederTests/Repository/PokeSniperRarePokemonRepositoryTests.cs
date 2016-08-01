using Microsoft.VisualStudio.TestTools.UnitTesting;
using PogoLocationFeeder;
using PogoLocationFeeder.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Tests
{
    [TestClass()]
    public class PokeSniperRarePokemonRepositoryTests
    {
        PokeSniperRarePokemonRepository pokeSniperRarePokemonRepository = new PokeSniperRarePokemonRepository(RarePokemonsFactory.createRarePokemonList());

        [TestMethod()]
        [Ignore]
        //Test is on ignore because it can fail random
        //This still can be used to test if the pokesnipers api works
        public void FindAllTest()
        {
            List<SniperInfo> sniperInfos = pokeSniperRarePokemonRepository.FindAll();
            Assert.IsNotNull(sniperInfos);
            Assert.IsTrue(sniperInfos.Any());
            sniperInfos.ForEach(SniperInfo => Console.WriteLine(SniperInfo));
        }

        [TestMethod()]
        public void GetChannel()
        {
            Assert.AreEqual("Pokesnipers", pokeSniperRarePokemonRepository.GetChannel());
        }

    }
}