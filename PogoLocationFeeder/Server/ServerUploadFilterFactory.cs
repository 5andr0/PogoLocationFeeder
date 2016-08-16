using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PogoLocationFeeder.Client;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.Repository;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Server
{
    class ServerUploadFilterFactory
    {
        public static ServerUploadFilter Create(List<PokemonId> pokemon)
        {
            ServerUploadFilter serverUploadFilter = new ServerUploadFilter();

            var pokemonsBinary = PokemonFilterToBinary.ToBinary(pokemon);

            serverUploadFilter.pokemon = pokemonsBinary;
            return serverUploadFilter;
        }
    }
}
