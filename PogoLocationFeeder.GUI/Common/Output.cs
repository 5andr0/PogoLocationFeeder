using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PogoLocationFeeder.Common;
using System.Windows;
using PogoLocationFeeder.GUI.Models;
using System.Windows.Media.Imaging;
using System.IO;

namespace PogoLocationFeeder.GUI.Common
{
    public class Output : PogoLocationFeeder.Common.IOutput
    {
        public void Write(string message, LogLevel level = LogLevel.Info, ConsoleColor color = ConsoleColor.Black, bool force = false)
        {
        }

        public void PrintPokemon(SniperInfo sniperInfo, string source)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate () {
                var info = new SniperInfoModel
                {
                    Info = sniperInfo,
                    Icon = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + $"\\assets\\icons\\{(int)sniperInfo.Id}.png")),
                    Source = source
                };
                GlobalVariables.PokemonsInternal.Insert(0, info);
            });
        }
    }
}
