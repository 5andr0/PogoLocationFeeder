using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Input;
using log4net.Config;
using MaterialDesignThemes.Wpf;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Common.Models;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.Common;
using PogoLocationFeeder.GUI.Properties;
using PropertyChanged;

//using POGOProtos.Enums;

namespace PogoLocationFeeder.GUI.ViewModels
{
    [ImplementPropertyChanged]
    public class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            Instance = this;
            Pokemons = new ReadOnlyObservableCollection<SniperInfoModel>(GlobalVariables.PokemonsInternal);
            SettingsComand = new ActionCommand(ShowSettings);
            StartStopCommand = new ActionCommand(Startstop);
            DebugComand = new ActionCommand(ShowDebug);
            RemovePathCommand = new ActionCommand(RemovePath);
            Settings.Default.DebugOutput = "";
            //var poke = new SniperInfo {
            //    Id = PokemonId.Missingno,
            //    Latitude = 45.99999,
            //    Longitude = 66.6677,
            //    ExpirationTimestamp = DateTime.Now
            //};
            //var y = new SniperInfoModel {
            //    Info = poke,
            //    Icon = new BitmapImage(new Uri(Path.Combine(iconPath, $"{(int) poke.Id}.png")))
            //};
            //GlobalVariables.PokemonsInternal.Add(y);
            GlobalSettings.Gui = true;
            XmlConfigurator.Configure(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("PogoLocationFeeder.GUI.App.config"));
            GlobalSettings.Output = new Output();
            GlobalSettings.PokeSnipers2exe = Settings.Default.Sniper2Path;
            var p = new Program();
            var a = new Thread(p.Start) {IsBackground = true};
            a.Start();
        }

        public static MainWindowViewModel Instance { get; private set; }

        public int TransitionerIndex { get; set; }

        public PackIconKind PausePlayButtonIcon { get; set; } = PackIconKind.Pause;
        public ReadOnlyObservableCollection<SniperInfoModel> Pokemons { get; }

        public ICommand SettingsComand { get; }
        public ICommand DebugComand { get; }
        public ICommand StartStopCommand { get; }
        public ICommand RemovePathCommand { get; }

        public string CustomIp { get; set; } = "localhost";

        public int CustomPort { get; set; }

        public string Status { get; set; } = "Connected to pogo-feed.mmoex.com";

        public string ThreadStatus { get; set; } = "[Running]";

        public int ShowLimit
        {
            get
            {
                if (Settings.Default.ShowLimit.Equals(0)) return 1;
                return Settings.Default.ShowLimit;
            }
            set
            {
                if (value <= 0) value = 1;
                Settings.Default.ShowLimit = value;
                Settings.Default.Save();
            }
        }

        public string Sniper2exe {
            get {
                return Settings.Default.Sniper2Path;
            }
            set {
                Settings.Default.Sniper2Path = value;
                Settings.Default.Save();
            }
        }

        public void RemovePath() {
            Sniper2exe = string.Empty;
        }

        public void SetStatus(string status)
        {
            Status = status;
        }

        public void ShowSettings()
        {
            if (TransitionerIndex != 0)
            {
                TransitionerIndex = 0;
                return;
            }
            TransitionerIndex = 1;
            if (GlobalSettings.Settings != null)
            {
                CustomPort = GlobalSettings.Settings.Port;
            }
        }

        public void ShowDebug()
        {
            if (TransitionerIndex != 0)
            {
                TransitionerIndex = 0;
                return;
            }
            TransitionerIndex = 2;
        }

        private void Startstop()
        {
            var status = GlobalSettings.ThreadPause;
            if (status)
            {
                GlobalSettings.ThreadPause = false;
                ThreadStatus = "[Running]";
                PausePlayButtonIcon = PackIconKind.Pause;
                return;
            }
            GlobalSettings.ThreadPause = true;
            ThreadStatus = "[Paused]";
            PausePlayButtonIcon = PackIconKind.Play;
        }
    }
}