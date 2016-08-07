using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.Properties;
using PogoLocationFeeder.GUI.ViewModels;
using PogoLocationFeeder.Helper;
using PropertyChanged;

namespace PogoLocationFeeder.GUI.Models { 

    [ImplementPropertyChanged]
    public class SniperInfoModel
    {
        private SniperInfo _info;

        public SniperInfoModel()
        {
            copyCoordsCommand = new ActionCommand(CopyCoords);
            PokeSnipersCommand = new ActionCommand(PokeSnipers);
            SniperVisibility = GlobalSettings.SniperVisibility;
            Created = DateTime.Now;


            Thread clean = new Thread(CleanupThread) { IsBackground = true };
            clean.Start();
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

        public DateTime Created;

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
            var sta = new Process();
            var sniperFilePath = GlobalSettings.PokeSnipers2Exe;
            sta.StartInfo.FileName = sniperFilePath;
            sta.StartInfo.Arguments = $"pokesniper2://{Info.Id}/{Info.Latitude.ToString(CultureInfo.InvariantCulture)},{Info.Longitude.ToString(CultureInfo.InvariantCulture)}";
            sta.Start();
            sta.Dispose();
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
                    Process.Start($"pokesniper2://{Info.Id}/{Info.Latitude.ToString(CultureInfo.InvariantCulture)},{Info.Longitude.ToString(CultureInfo.InvariantCulture)}");
                }
            }
            catch (Exception e)
            {
                Log.Error("Error while launching pokesniper2", e);
            }
        }
        public void CleanupThread() {
            while(true) {
                try {
                    var ukn = "";
                    var expiration = Info.ExpirationTimestamp;
                    if(expiration.Equals(default(DateTime))) {
                        expiration = Created.AddMinutes(GlobalSettings.RemoveAfter);
                        ukn = "unk. ";
                    }
                    var remaining = expiration - DateTime.Now;

                    if(remaining < TimeSpan.Zero) {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                            GlobalVariables.PokemonsInternal.Remove(this);
                        }));
                    }
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                        Date = $"{ukn}{remaining.Minutes}m {remaining.Seconds}s";
                    }));
                    Thread.Sleep(1000);
                } catch (Exception) {
                    Thread.Sleep(1000);
                    //hmm ignore?
                }
                
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}