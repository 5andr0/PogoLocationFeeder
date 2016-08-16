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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using log4net.Config;
using MahApps.Metro;
using MaterialDesignThemes.Wpf;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.Common;
using PogoLocationFeeder.GUI.Models;
using PogoLocationFeeder.GUI.Properties;
using PogoLocationFeeder.Input;
using PropertyChanged;
using PogoLocationFeeder.Helper;

//using POGOProtos.Enums;

namespace PogoLocationFeeder.GUI.ViewModels
{
    [ImplementPropertyChanged]
    public class MainWindowViewModel
    {
        private Visibility _colVisibility;
        private ComboBoxItem _appTheme;

        public MainWindowViewModel()
        {
            Instance = this;
            Pokemons = new ReadOnlyObservableCollection<SniperInfoModel>(GlobalVariables.PokemonsInternal);
            PokemonFilter = new ReadOnlyObservableCollection<PokemonFilterModel>(GlobalVariables.AllPokemonsInternal);
            PokemonToFilter = new ReadOnlyObservableCollection<PokemonFilterModel>(GlobalVariables.PokemonToFeedFilterInternal);
            SettingsComand = new ActionCommand(ShowSettings);
            StartStopCommand = new ActionCommand(Startstop);
            DebugComand = new ActionCommand(ShowDebug);
            RemovePathCommand = new ActionCommand(RemovePath);
            SaveCommand = new ActionCommand(SaveClick);
            PayPalCommand = new ActionCommand(OpenPaypal);
            BitcoinCommand = new ActionCommand(OpenBitcoin);
            FilterCommand = new ActionCommand(ShowFilter);
            SortAlphabeticalCommand = new ActionCommand(SortByAlphabetical);
            SortIdCommand = new ActionCommand(SortById);
            SendCommand = new ActionCommand(Send);
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
            var p = new Program();
            var a = new Thread(p.Start) { IsBackground = true };
            a.Start();
            var b = new Thread(CleanupThread.Start) { IsBackground = true };
            b.Start();
            Common.PokemonFilter.Load();
        }

        public static MainWindowViewModel Instance { get; private set; }

        public int TransitionerIndex { get; set; }

        public PackIconKind PausePlayButtonIcon { get; set; } = PackIconKind.Pause;
        public ReadOnlyObservableCollection<SniperInfoModel> Pokemons { get; }
        public ReadOnlyObservableCollection<PokemonFilterModel> PokemonFilter { get; set; }
        public ReadOnlyObservableCollection<PokemonFilterModel> PokemonToFilter { get; set; }

        public ICommand SettingsComand { get; }
        public ICommand DebugComand { get; }
        public ICommand StartStopCommand { get; }
        public ICommand RemovePathCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand PayPalCommand { get; }
        public ICommand BitcoinCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand SortAlphabeticalCommand { get; }
        public ICommand SortIdCommand { get; }
        public ICommand SendCommand { get; }

        public string CustomIp { get; set; } = "localhost";

        public int CustomPort { get; set; }

        public string Status { get; set; } = "Connected to pogo-feed.mmoex.com";

        public string ThreadStatus { get; set; } = "[Running]";

        public int ShowLimit { get; set; }

        public string Sniper2Exe { get; set; }

        public string RemoveMinutes { get; set; }
        public bool UseSkiplagged { get; set; }
        public bool UseFilter { get; set; }
        public bool UseGeoLocationBoundsFilter { get; set; }
        public LatLngBounds GeoLocationBounds { get; set; }

        public PokemonFilterModel SelectedPokemonFilter { get; set; }
        public PokemonFilterModel SelectedPokemonFiltered { get; set; }
        public int IndexPokemonToFilter { get; set; }
        public SolidColorBrush SortAlphaActive { get; set; }
        public SolidColorBrush SortIdActive { get; set; } = new SolidColorBrush(Colors.DimGray);

        public Visibility ColVisibility
        {
            get
            {
                _colVisibility = GlobalSettings.IsOneClickSnipeSupported() ? Visibility.Visible : Visibility.Collapsed;
                return _colVisibility;
            }
            set { _colVisibility = value; }
        }

        public ComboBoxItem AppTheme {
            get { return _appTheme; }
            set {
                ChangeTheme(value.Content.ToString());
                _appTheme = value;
            }
        }

        public string AppThemeText { get; set; }

        public string SendText { get; set; }

        public void ChangeTheme(string theme) {
            try {
                switch (theme.ToLower()) {
                    case "light":
                        new PaletteHelper().SetLightDark(false);
                        break;
                    case "dark":
                        new PaletteHelper().SetLightDark(true);
                        break;

                }

            } catch (Exception) {
                
            }
        }

        public void RemovePath()
        {
            Sniper2Exe = "";
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
            GlobalSettings.Load();
            ShowLimit = GlobalSettings.ShowLimit;
            CustomPort = GlobalSettings.Port;
            Sniper2Exe = GlobalSettings.PokeSnipers2Exe;
            ShowLimit = GlobalSettings.ShowLimit;
            UseSkiplagged = GlobalSettings.VerifyOnSkiplagged;
            RemoveMinutes = GlobalSettings.RemoveAfter.ToString();
            UseFilter = GlobalSettings.UseFilter;
            UseGeoLocationBoundsFilter = GlobalSettings.UseGeoLocationBoundsFilter;
            GeoLocationBounds = GlobalSettings.GeoLocationBounds;
            AppThemeText = GlobalSettings.AppTheme;
            TransitionerIndex = 1;

        }

        public void SaveClick()
        {
            if (Sniper2Exe != null && Sniper2Exe.Contains(".exe"))
            {
                ColVisibility = Visibility.Visible;
            }
            if (Sniper2Exe == null || Sniper2Exe.Equals(""))
            {
                Sniper2Exe = "";
                ColVisibility = Visibility.Collapsed;
            }
            GlobalSettings.ShowLimit = Math.Max(ShowLimit, 1);
            GlobalSettings.Port = CustomPort;
            GlobalSettings.PokeSnipers2Exe = Sniper2Exe;
            GlobalSettings.ShowLimit = ShowLimit;
            GlobalSettings.RemoveAfter = int.Parse(RemoveMinutes);
            GlobalSettings.VerifyOnSkiplagged = UseSkiplagged;
            GlobalSettings.UseFilter = UseFilter;
            GlobalSettings.UseGeoLocationBoundsFilter = UseGeoLocationBoundsFilter;
            GlobalSettings.GeoLocationBounds = GeoLocationBounds;
            GlobalSettings.AppTheme = AppThemeText;
            GlobalSettings.Save();

            GlobalSettings.Output.RemoveListExtras();
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

        public void ShowFilter()
        {
            if (TransitionerIndex != 0)
            {
                TransitionerIndex = 0;
                Common.PokemonFilter.Save();
                return;
            }
            TransitionerIndex = 3;
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

        public void OpenPaypal()
        {
            try
            {
                Process.Start("https://www.paypal.com/en_US/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=QZCKGUUQ9RYPY");

            }
            catch (Exception)
            {
                //ignore
            }
        }
        public void OpenBitcoin()
        {
            try
            {
                Process.Start("bitcoin:1FeederpUZXQN6F45M5cpYuYP6MzE2huPp?label=PogoLocationFeeder");

            }
            catch (Exception)
            {
                //ignore
            }
        }

        public void SortByAlphabetical()
        {
            SortIdActive = new SolidColorBrush(Colors.Transparent);
            SortAlphaActive = new SolidColorBrush(Colors.DimGray);

            GlobalVariables.SortMode = SortMode.AlphabeticalAsc;
            var list = GlobalVariables.AllPokemonsInternal;
            GlobalVariables.AllPokemonsInternal = new ObservableCollection<PokemonFilterModel>(list.OrderBy(x => x.Id.ToString()));

            list = GlobalVariables.PokemonToFeedFilterInternal;
            GlobalVariables.PokemonToFeedFilterInternal = new ObservableCollection<PokemonFilterModel>(list.OrderBy(x => x.Id.ToString()));

            PokemonFilter = new ReadOnlyObservableCollection<PokemonFilterModel>(GlobalVariables.AllPokemonsInternal);
            PokemonToFilter = new ReadOnlyObservableCollection<PokemonFilterModel>(GlobalVariables.PokemonToFeedFilterInternal);
        }

        public void SortById()
        {
            SortIdActive = new SolidColorBrush(Colors.DimGray);
            SortAlphaActive = new SolidColorBrush(Colors.Transparent);

            GlobalVariables.SortMode = SortMode.IdAsc;
            var list = GlobalVariables.AllPokemonsInternal;
            GlobalVariables.AllPokemonsInternal = new ObservableCollection<PokemonFilterModel>(list.OrderBy(x => x.Id));

            list = GlobalVariables.PokemonToFeedFilterInternal;
            GlobalVariables.PokemonToFeedFilterInternal = new ObservableCollection<PokemonFilterModel>(list.OrderBy(x => x.Id));

            PokemonFilter = new ReadOnlyObservableCollection<PokemonFilterModel>(GlobalVariables.AllPokemonsInternal);
            PokemonToFilter = new ReadOnlyObservableCollection<PokemonFilterModel>(GlobalVariables.PokemonToFeedFilterInternal);
        }

        public void Send() {
            try {
                InputService.Instance.ParseAndSend(SendText);
                SendText = string.Empty;

            } catch (Exception) {
                //ignore
            }
        }
    }

    public class BindingProxy : Freezable
    {
        #region Overrides of Freezable

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        #endregion

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object),
                typeof(BindingProxy));
    }
}
