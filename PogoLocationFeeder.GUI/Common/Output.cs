/*
PogoLocationFeeder gathers pokemon data from various sources and serves it to connected clients
Copyright (C) 2016  PogoLocationFeeder Development Team <admin@pokefeeder.live>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using log4net.Appender;
using log4net.Core;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.Models;
using PogoLocationFeeder.GUI.ViewModels;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.GUI.Properties;

namespace PogoLocationFeeder.GUI.Common
{
    public class Output : IOutput
    {
        public void SetStatus(string message)
        {
            MainWindowViewModel.Instance.SetStatus(message);
        }

        public void PrintPokemon(SniperInfo sniperInfo)
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
                    Server = sniperInfo.ChannelInfo?.server,
                    Channel = sniperInfo.ChannelInfo?.channel
                };
                info.Icon.Freeze();
                InsertToList(info);
                RemoveListExtras();
            });
        }

        public void RemoveListExtras()
        {
            var pokes = GlobalVariables.PokemonsInternal;
            while (pokes.Count > GlobalSettings.ShowLimit)
            {
                pokes.Remove(pokes.Last());
            }
        }

        public void InsertToList(SniperInfoModel info)
        {
            var pokes = GlobalVariables.PokemonsInternal;
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
