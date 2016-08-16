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

using System.Collections.Generic;
using PogoLocationFeeder.Config;

namespace PogoLocationFeeder.Repository
{
    public class RarePokemonRepositoryFactory
    {
        public static List<IRarePokemonRepository> CreateRepositories(GlobalSettings globalSettings)
        {
            var rarePokemonRepositories = new List<IRarePokemonRepository>();
            if (GlobalSettings.UsePokeSnipers)
            {
                rarePokemonRepositories.Add(new PokeSniperRarePokemonRepository());
            }
            if (GlobalSettings.UseTrackemon)
            {
                rarePokemonRepositories.Add(new TrackemonRarePokemonRepository());
            }
            if (GlobalSettings.UsePokezz)
            {
                rarePokemonRepositories.Add(new PokezzRarePokemonRepository());
            }
            if (GlobalSettings.UsePokewatchers)
            {
                rarePokemonRepositories.Add(new PokewatchersRarePokemonRepository());
            }
            if (GlobalSettings.UsePokemonGoIVClub)
            {
                rarePokemonRepositories.Add(new PokemonGoIVClubRarePokemonRepository());
            }
            return rarePokemonRepositories;
        }
    }
}
