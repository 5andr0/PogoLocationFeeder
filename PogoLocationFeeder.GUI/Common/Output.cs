using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using log4net.Appender;
using log4net.Core;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Common.Models;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.ViewModels;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.Common.Properties;

namespace PogoLocationFeeder.GUI.Common
{
    public class Output : IOutput
    {
        private static int _showLimit = Settings.Default.ShowLimit;

        public void SetStatus(string message)
        {
            MainWindowViewModel.Instance.SetStatus(message);
        }

        public void PrintPokemon(SniperInfo sniperInfo, ChannelInfo channelInfo)
        {
            Application.Current.Dispatcher.BeginInvoke((Action) delegate
            {
                var info = new SniperInfoModel
                {
                    Info = sniperInfo,
                    Icon =
                        new BitmapImage(
                            new Uri(
                                $"pack://application:,,,/PogoLocationFeeder.GUI;component/Assets/icons/{(int) sniperInfo.Id}.png",
                                UriKind.Absolute)),
                    Server = channelInfo.server,
                    Channel = channelInfo.channel
                };
                InsertToList(info);
            });
        }

        public void InsertToList(SniperInfoModel info)
        {
            var pokes = GlobalVariables.PokemonsInternal;
            _showLimit = Settings.Default.ShowLimit;
            if (pokes.Count > _showLimit)
            {
                var diff = pokes.Count - _showLimit;
                for (var i = 0; i < diff; i++)
                {
                    pokes.Remove(pokes.Last());
                }
            }
            if (pokes.Count >= _showLimit)
                pokes.Remove(pokes.Last());
            pokes.Insert(0, info);
        }

        public static void Write(string message)
        {
            Settings.Default.DebugOutput += $"\n{message}";
        }
    }

    public class DebugOutputViewAppender : AppenderSkeleton
    {
        protected override bool RequiresLayout => true;

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (GlobalSettings.Output != null)
            {
                using (var stringWriter = new StringWriter())
                {
                    RenderLoggingEvent(stringWriter, loggingEvent);
                    if (loggingEvent.Level == Level.Fatal)
                    {
                        MessageBox.Show(stringWriter.ToString(), "Fatal Error", MessageBoxButton.OK,
                            MessageBoxImage.Exclamation);
                    }
                    Output.Write(stringWriter.ToString());
                }
            }
        }
    }
}