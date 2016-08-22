/*
PogoLocationFeeder gathers pokemon data from various sources and serves it to connected clients
Copyright (C) 2016  PogoLocationFeeder Development Team <admin@pokefeeder.live>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
        //Test is on ignore because it can fail random
        //This still can be used to test if the pokesnipers api works
        public void ReadAllTest()
        {
            var rarePokemonRepository = new PokeSnipersRarePokemonRepository();
            var sniperInfos = rarePokemonRepository.FindAll();
            Assert.IsNotNull(sniperInfos);
            Assert.IsTrue(sniperInfos.Any());
            sniperInfos.ForEach(sniperInfo => Console.WriteLine(sniperInfo.ToString()));
        }
    }
}
