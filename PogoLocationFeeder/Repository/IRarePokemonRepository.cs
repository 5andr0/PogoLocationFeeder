using System.Collections.Generic;
using PogoLocationFeeder.Helper;

namespace PogoLocationFeeder.Repository
{
    public interface IRarePokemonRepository
    {
        List<SniperInfo> FindAll();
        string GetChannel();
    }
}