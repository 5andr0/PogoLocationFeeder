using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Repository
{
    public interface RarePokemonRepository
    {
        List<SniperInfo> FindAll();
        String GetChannel();
    }
}
