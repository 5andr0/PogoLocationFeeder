using Microsoft.VisualStudio.TestTools.UnitTesting;
using PogoLocationFeeder.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Repository.Tests
{
    [TestClass()]
    public class PokeSnipeRarePokemonRepositoryTests
    {
        [TestMethod()]
        public void FindAllTest()
        {
            var repo = new PokeSnipeRarePokemonRepository();
            var sniperInfos = repo.FindAll();

            Assert.IsNotNull(sniperInfos);
            Assert.IsTrue(sniperInfos.Any());
            sniperInfos.ForEach(sniperInfo => Console.WriteLine(sniperInfo.ToString()));
        }
    }
}