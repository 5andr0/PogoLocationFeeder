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
using PogoLocationFeeder.GUI.ViewModels;
using PogoLocationFeeder.Helper;

namespace PogoLocationFeeder.GUI.Common
{
    public class Output : PogoLocationFeeder.Common.IOutput
    {
        private static int ShowLimit = Settings.Default.ShowLimit;

        public void SetStatus(string message)
        {
            MainWindowViewModel.Instance.SetStatus(message);
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

        public void PrintPokemon(SniperInfo sniperInfo, ChannelInfo channelInfo)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate () {
                var info = new SniperInfoModel
                {
                    Info = sniperInfo,
                    Icon = new BitmapImage(new Uri($"pack://application:,,,/PogoLocationFeeder.GUI;component/Assets/icons/{(int)sniperInfo.Id}.png", UriKind.Absolute)),
                    Server = channelInfo.server,
                    Channel = channelInfo.channel
                };
                InsertToList(info);
            });
        }

        public static void Write(string message)
        {
            Settings.Default.DebugOutput += $"\n{message}";
        }
    }

    public class DebugOutputViewAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
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
                    Output.Write(stringWriter.ToString());
                }
            }
        }


        override protected bool RequiresLayout
        {
            get { return true; }
        }
    }
}
