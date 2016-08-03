using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Common
{
    public interface IOutput
    {
        void Write(string message);
        void PrintPokemon(SniperInfo info, string server, string channel);
        void WriteFormat(string message, params object[] args);
        void SetStatus(string message);
    }
}
