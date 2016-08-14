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

using System.Threading;
using System.Windows;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.ViewModels;
using System.Diagnostics;
using System;

namespace PogoLocationFeeder.GUI
{
    /// <summary>
    ///     Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private void AppStartup(object sender, StartupEventArgs args)
        {
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                MessageBox.Show(AppDomain.CurrentDomain.FriendlyName + " is already running. \n\nApplication will now close to prevent accidental usage of multiple socket.", "Application running!", MessageBoxButton.OK, MessageBoxImage.Stop);
                Application.Current.Shutdown();
            }
            else
            {
                //Waiting for the settings to be loaded
                Thread.Sleep(1000);
                var mainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel()
                };
                mainWindow.Show();
            }
        }
    }
}
