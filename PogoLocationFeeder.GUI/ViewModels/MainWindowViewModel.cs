using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MangaChecker.ViewModels;
using Newtonsoft.Json;
using PogoLocationFeeder.GUI.Common;
using PogoLocationFeeder.GUI.Models;
using PogoLocationFeeder.GUI.Properties;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.Repository;
using PoGo.LocationFeeder.Settings;
using POGOProtos.Enums;
using PropertyChanged;
//using POGOProtos.Enums;

namespace PogoLocationFeeder.GUI.ViewModels {
    [ImplementPropertyChanged]
    public class MainWindowViewModel {
        public int TransitionerIndex { get; set; } = 0;
        
        private string _customIp = "localhost";
        private int _customPort = 16969;

        public MainWindowViewModel() {
            Pokemons = new ReadOnlyObservableCollection<SniperInfoModel>(GlobalVariables.PokemonsInternal);
            SettingsComand = new ActionCommand(ShowSettings);
            StartStopCommand = new ActionCommand(StartStop);

            var x = Directory.GetCurrentDirectory();
            var poke = new SniperInfo {
                Id = PokemonId.Missingno,
                Latitude = 45.99999,
                Longitude = 66.6677,
                ExpirationTimestamp = DateTime.Now,
            };
            var y = new SniperInfoModel() {
                Info = poke,
                Icon = new BitmapImage(new Uri(x + $"\\assets\\icons\\{(int)poke.Id}.png"))
            };
            GlobalVariables.PokemonsInternal.Add(y);
            //new PogoLocationFeeder.Common.IOutput()
            GlobalSettings.Output = new Output();
            Program p = new Program();
            Thread a = new Thread(new ThreadStart(p.Start)) {IsBackground = true, ApartmentState = ApartmentState.STA};
            //Start(); p
            a.Start();
        }

        public ReadOnlyObservableCollection<SniperInfoModel> Pokemons { get; }

        public ICommand SettingsComand { get; }
        public ICommand StartStopCommand { get; }

        public string CustomIp {
            get { return _customIp; }
            set { _customIp = value; }
        }

        public int CustomPort {
            get { return _customPort; }
            set { _customPort = value; }
        }

        public string Status { get; set; }

        public void ShowSettings() {
            if (TransitionerIndex != 0) {
                TransitionerIndex = 0;
                return;
            }
            TransitionerIndex = 1;
        }

        private void StartStop() {
            //todo
        }
        

       
        public void setStatus(string status) {
            Status = status;
        }
    }
}
