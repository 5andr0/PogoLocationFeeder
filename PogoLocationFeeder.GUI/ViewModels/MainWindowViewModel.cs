using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MangaChecker.ViewModels;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using PogoLocationFeeder.GUI.Models;
using PogoLocationFeeder.GUI.Properties;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.Repository;
using PoGo.LocationFeeder.Settings;
using POGOProtos.Enums;
using PropertyChanged;
using PogoLocationFeeder.GUI.Common;

//using POGOProtos.Enums;

namespace PogoLocationFeeder.GUI.ViewModels {
    [ImplementPropertyChanged]
    public class MainWindowViewModel
    {

        private static MainWindowViewModel _instance;
        public static MainWindowViewModel Instance => _instance;

        public MainWindowViewModel()
        {
            _instance = this;
            Pokemons = new ReadOnlyObservableCollection<SniperInfoModel>(GlobalVariables.PokemonsInternal);
            SettingsComand = new ActionCommand(ShowSettings);
            StartStopCommand = new ActionCommand(Startstop);
            DebugComand = new ActionCommand(ShowDebug);

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

            GlobalSettings.Output = new Output();
            Program p = new Program();
            Thread a = new Thread(p.Start) { IsBackground = true};
            //Start(); p
            a.Start();
        }

        public int TransitionerIndex { get; set; } = 0;

        //public PackIconKind PausePlayButtonIcon { get; set; } = PackIconKind.Pause;
        public ReadOnlyObservableCollection<SniperInfoModel> Pokemons { get; }

        public ICommand SettingsComand { get; }
        public ICommand DebugComand { get; }
        public ICommand StartStopCommand { get; }

        public string CustomIp { get; set; } = "localhost";

        public int CustomPort { get; set; } = 0;

        public string Status { get; set; } = "Connected to pogo-feed.mmoex.com";

        public string ThreadStatus { get; set; } = "[Running]";

        public int ShowLimit {
            get {
                if (Settings.Default.ShowLimit.Equals(0)) return 1;
                return Settings.Default.ShowLimit;
            }
            set {
                if (value <= 0) value = 1;
                Settings.Default.ShowLimit = value;
                Settings.Default.Save();
            }
        }

        public PackIconKind PausePlayButtonIcon { get;set;} = PackIconKind.Pause;

        public void SetStatus(string status) {
            Status = status;
        }

        public void ShowSettings() {
            if (TransitionerIndex != 0) {
                TransitionerIndex = 0;
                return;
            }
            TransitionerIndex = 1;
            if (GlobalSettings.Settings != null)
            {
                CustomPort = GlobalSettings.Settings.Port;
            }
            
        }

        public void ShowDebug() {
            if (TransitionerIndex != 0) {
                TransitionerIndex = 0;
                return;
            }
            TransitionerIndex = 2;
        }

        private void Startstop() {
            var status = GlobalSettings.ThreadPause;
            if (status) {
                GlobalSettings.ThreadPause = false;
                ThreadStatus = "[Running]";
                PausePlayButtonIcon = PackIconKind.Pause;
                return;
            }
            GlobalSettings.ThreadPause = true;
            ThreadStatus = "[Paused]";
            PausePlayButtonIcon= PackIconKind.Play;
        }
        
    }
}