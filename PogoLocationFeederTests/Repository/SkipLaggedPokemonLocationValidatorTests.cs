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
    public class SkipLaggedPokemonLocationValidatorTests
    {
        [TestMethod()]
        public void VerifySkipLaggedIsWorkingTest()
        {
            Assert.IsTrue(SkipLaggedPokemonLocationValidator.VerifySkipLaggedIsWorking());
        }
    }
}