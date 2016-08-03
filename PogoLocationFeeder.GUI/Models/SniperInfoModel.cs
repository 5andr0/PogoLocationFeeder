using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PogoLocationFeeder.GUI.ViewModels;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.Properties;

namespace PogoLocationFeeder.Common.Models
{
    public class SniperInfoModel
    {
        private SniperInfo _info;

        public SniperInfoModel()
        {
            copyCoordsCommand = new ActionCommand(CopyCoords);
            PokeSnipersCommand = new ActionCommand(PokeSnipers);
            SniperVisibility = GlobalSettings.SniperVisibility;
        }

        public BitmapImage Icon { get; set; }
        public string Server { get; set; }
        public string Channel { get; set; }
        public bool SniperVisibility { get; set; }

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
            }
        }

        public string Date { get; set; }

        public string IV { get; set; }

        public ICommand copyCoordsCommand { get; }
        public ICommand PokeSnipersCommand { get; }

        public void CopyCoords()
        {
            Clipboard.SetText(
                $"{Info.Latitude.ToString(CultureInfo.InvariantCulture)}, {Info.Longitude.ToString(CultureInfo.InvariantCulture)}");
        }

        private void StartProcessWithPath() {
            var sta = new Process();
            var SniperFilePath = Settings.Default.Sniper2Path;
            sta.StartInfo.FileName = SniperFilePath;
            sta.StartInfo.Arguments = $"pokesniper2://{Info.Id}/{Info.Latitude.ToString(CultureInfo.InvariantCulture)},{Info.Longitude.ToString(CultureInfo.InvariantCulture)}";
            sta.Start();
            sta.Dispose();
        }

        public void PokeSnipers()
        {
            try
            {
                if (Settings.Default.Sniper2Path.Contains(".exe")) {
                    StartProcessWithPath();
                } else {
                    Process.Start( $"pokesniper2://{Info.Id}/{Info.Latitude.ToString(CultureInfo.InvariantCulture)},{Info.Longitude.ToString(CultureInfo.InvariantCulture)}");
                }
            }
            catch (Exception e)
            {
                Log.Error("Error while launching pokesniper2", e);
            }
        }
    }
}