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
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.ViewModels;
using PogoLocationFeeder.Helper;
using PropertyChanged;
using Timer = System.Threading.Timer;

namespace PogoLocationFeeder.GUI.Models { 

    [ImplementPropertyChanged]
    public class SniperInfoModel
    {
        private SniperInfo _info;

        public SniperInfoModel()
        {
            copyCoordsCommand = new ActionCommand(CopyCoords);
            PokeSnipersCommand = new ActionCommand(PokeSnipers);
            RemoveMe = new ActionCommand(Remove);
            SniperVisibility = GlobalSettings.SniperVisibility;
            Created = DateTime.Now;
        }

        public BitmapImage Icon { get; set; }
        public string Server { get; set; }
        public string Channel { get; set; }
        public bool SniperVisibility { get; set; }
        public PackIconKind VerifiedIcon { get; set; } = PackIconKind.Close;
        public string VerifiedTooltip { get; set; }

        public SniperInfo Info
        {
            get { return _info; }
            set
            {
                _info = value;
                Date = Info.ExpirationTimestamp.Equals(default(DateTime))
                    ? "Unknown"
                    : Info.ExpirationTimestamp.ToString(CultureInfo.InvariantCulture);
                IV = Info.IV.Equals(0) ? "??" : Info.IV.ToString(CultureInfo.InvariantCulture);
                VerifiedIcon = Info.Verified ? PackIconKind.Check : PackIconKind.Close;
                VerifiedTooltip = Info.Verified ? "Verified " : "Not Verifed";
            }
        }

        public string Date { get; set; }

        public string IV { get; set; }

        public ICommand copyCoordsCommand { get; }
        public ICommand PokeSnipersCommand { get; }
        public ICommand RemoveMe { get; }

        public DateTime Created;

        public void Remove() {
            GlobalVariables.PokemonsInternal.Remove(this);
        }

        public void CopyCoords()
        {
            try {
                Clipboard.SetText(
                    $"{Info.Latitude.ToString(CultureInfo.InvariantCulture)}, {Info.Longitude.ToString(CultureInfo.InvariantCulture)}");

            } catch (Exception) {
                Clipboard.SetDataObject(
                    $"{Info.Latitude.ToString(CultureInfo.InvariantCulture)}, {Info.Longitude.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        private void StartProcessWithPath() {
            try
            {
                var process = new Process();
                var sniperFilePath = GlobalSettings.PokeSnipers2Exe;
                var sniperFileDir = System.IO.Path.GetDirectoryName(sniperFilePath);
                process.StartInfo.FileName = sniperFilePath;
                process.StartInfo.WorkingDirectory = sniperFileDir;
                process.StartInfo.Arguments =
                    $"pokesniper2://{Info.Id}/{Info.Latitude.ToString(CultureInfo.InvariantCulture)},{Info.Longitude.ToString(CultureInfo.InvariantCulture)}";
                process.Start();

                KillProcessLater(process);

            }
            catch (Exception e)
            {
                Log.Error("Error starting pokesniper2", e);
            }
        }


        public void PokeSnipers()
        {
            try
            {
                if (GlobalSettings.PokeSnipers2Exe.Contains(".exe")) {
                    Log.Debug($"using the path: {GlobalSettings.PokeSnipers2Exe} to start pokesniper2 ");
                    StartProcessWithPath();
                } else {
                    Log.Debug("using url to start pokesniper2 ");
                    var process = Process.Start($"pokesniper2://{Info.Id}/{Info.Latitude.ToString(CultureInfo.InvariantCulture)},{Info.Longitude.ToString(CultureInfo.InvariantCulture)}");
                    KillProcessLater(process);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error while launching pokesniper2", e);
            }
        }

        private static void KillProcessLater(Process process)
        {
            if (process != null)
            {
                Task.Run(async () => await AfterDelay(() =>
                {
                    process.Kill();
                    process.Dispose();
                }, 30000));
            }
        }


        static async Task AfterDelay(Action action, int delay)
        {
            try
            {
                await Task.Delay(delay);
                action.Invoke();
            }
            catch (Exception ex)
            {

            }
        }


    }
}
