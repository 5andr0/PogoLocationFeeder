using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Repository.Tests
{
    [TestClass()]
    public class RareSpawnsRarePokemonRepositoryTests
    {

        [TestMethod()]
        [Ignore]
        public void TestRareSpawns()
        {
            var pokeSpawnsRarePokemonRepository  = new RareSpawnsRarePokemonRepository();
            var pokesnipers = pokeSpawnsRarePokemonRepository.FindAll();
            Assert.IsTrue(pokesnipers.Any());
        }
    }
}