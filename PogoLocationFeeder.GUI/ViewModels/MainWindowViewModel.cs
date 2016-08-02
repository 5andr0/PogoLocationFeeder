using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MangaChecker.ViewModels;
using PogoLocationFeeder.GUI.Properties;
using PropertyChanged;
//using POGOProtos.Enums;

namespace PogoLocationFeeder.GUI.ViewModels {
    [ImplementPropertyChanged]
    public class MainWindowViewModel {
        public int TransitionerIndex { get; set; } = 0;

        public ObservableCollection<SniperInfo> PokemonsInternal = new ObservableCollection<SniperInfo>();
        private string _customIp = "localhost";
        private int _customPort = 16969;

        public MainWindowViewModel() {
            Pokemons = new ReadOnlyObservableCollection<SniperInfo>(PokemonsInternal);
            SettingsComand = new ActionCommand(ShowSettings);
            StartStopCommand = new ActionCommand(StartStop);

            var x = System.IO.Directory.GetCurrentDirectory();
            var poke = new SniperInfo {
                //id = PokemonId.Aerodactyl,
                latitude = 45.99999,
                longitude = 66.6677,
                iv = 69.00,
                timeStamp = DateTime.Now,
            };
            poke.image = new BitmapImage(new Uri(x + $"\\icons\\{(int)poke.id}.png"));
            PokemonsInternal.Add(poke);

        }

        public ReadOnlyObservableCollection<SniperInfo> Pokemons { get; }

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

        public string Status { get; set; } = "Listeing...";

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
    }
}
