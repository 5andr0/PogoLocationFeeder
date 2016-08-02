using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Common
{
    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4
    }
    public interface IOutput
    {
        void Write(string message, LogLevel level = LogLevel.Info, ConsoleColor color = ConsoleColor.Black, bool force = false);
        void PrintPokemon(SniperInfo info, string source);
    }
}
