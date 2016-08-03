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
using PogoLocationFeeder.GUI.Properties;

namespace PogoLocationFeeder.GUI.Common
{
    public class Output : PogoLocationFeeder.Common.IOutput
    {
        private static readonly string assetPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        private static readonly string iconPath = Path.Combine(assetPath, "icons");
        private static object _MessageLock = new object();

        public void Write(string message, LogLevel level = LogLevel.Info, ConsoleColor color = ConsoleColor.Black)
        {
            lock (_MessageLock)
            {
                Settings.Default.DebugOutput += $"\n{message}";
            }
        }

        public void WriteFormat(string message, params object[] args)
        {
            lock (_MessageLock)
            {
                Settings.Default.DebugOutput += $"\n{string.Format(message, args)}";
            }
        }

        public void PrintPokemon(SniperInfo sniperInfo, string server, string channel)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate () {
                var info = new SniperInfoModel
                {
                    Info = sniperInfo,
                    Icon = new BitmapImage(new Uri(Path.Combine(iconPath, $"{(int)sniperInfo.Id}.png"))),
                    Server = server,
                    Channel = channel
                };
                GlobalVariables.PokemonsInternal.Insert(0, info);
            });
        }
    }
}
