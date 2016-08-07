using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PogoLocationFeeder.Repository;

namespace PogoLocationFeederTests.Tests
{
    [TestClass]
    public class PokezzRarePokemonRepositoryTests
    {
        [TestMethod]
        
        //Test is on ignore because it can fail random
        //This still can be used to test if the pokesnipers api works
        public void ReadAllTest()
        {
            var rarePokemonRepository = new PokezzRarePokemonRepository(RarePokemonsFactory.createRarePokemonList());
            rarePokemonRepository.FindAll();
        }
    }
}