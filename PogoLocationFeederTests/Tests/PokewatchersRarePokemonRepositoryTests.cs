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
    public class PokewatchersRarePokemonRepositoryTests
    {
        [TestMethod()]
        public void FindAllTest()
        {
            var pokeWatchersRareRepository = new PokewatchersRarePokemonRepository();
            var sniperInfos = pokeWatchersRareRepository.FindAll();

            Assert.IsNotNull(sniperInfos);
            Assert.IsTrue(sniperInfos.Any());
            sniperInfos.ForEach(sniperInfo => Console.WriteLine(sniperInfo.ToString()));

        }
    }
}
