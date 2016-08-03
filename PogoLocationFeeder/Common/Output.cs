using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PogoLocationFeeder.Helper;

namespace PogoLocationFeeder.Common
{
    public interface IOutput
    {
        void PrintPokemon(SniperInfo info, ChannelInfo channelInfo);
        void SetStatus(string message);
    }
}
