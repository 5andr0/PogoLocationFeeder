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
using log4net.Core;
using log4net.Appender;
using PoGo.LocationFeeder.Settings;

namespace PogoLocationFeeder.GUI.Common
{
    public class Output : PogoLocationFeeder.Common.IOutput
    {
        private static readonly string assetPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        private static readonly string iconPath = Path.Combine(assetPath, "icons");
        private static object _MessageLock = new object();
        private static int ShowLimit = Settings.Default.ShowLimit;

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
        public void InsertToList(SniperInfoModel info) {
            var pokes = GlobalVariables.PokemonsInternal;
            ShowLimit = Settings.Default.ShowLimit;
            if(pokes.Count > ShowLimit) {
                var diff = pokes.Count - ShowLimit;
                for(int i = 0; i < diff; i++) {
                    pokes.Remove(pokes.Last());
                }
            }
            if(pokes.Count >= ShowLimit)
                pokes.Remove(pokes.Last());
            pokes.Insert(0, info);
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
                InsertToList(info);
            });
        }
    }

    public class DebugOutputViewAppender : AppenderSkeleton
    {
        override protected void Append(LoggingEvent loggingEvent)
        {
            if (GlobalSettings.Output != null)
            {
                using (StringWriter stringWriter = new StringWriter())
                {
                    RenderLoggingEvent(stringWriter, loggingEvent);
                    if (loggingEvent.Level == Level.Fatal)
                    {
                        System.Windows.MessageBox.Show(stringWriter.ToString(), "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    GlobalSettings.Output.Write(stringWriter.ToString());
                }
            }
        }


        override protected bool RequiresLayout
        {
            get { return true; }
        }
    }
}
