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
            Task.Run(() => pokeSpawnsRarePokemonRepository.StartClient());
            Thread.Sleep(120* 1000);
        }
    }
}