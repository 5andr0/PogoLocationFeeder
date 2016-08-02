using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PogoLocationFeeder.GUI.Models;

namespace PogoLocationFeeder.GUI
{
    public static class GlobalVariables
    {

        public static ObservableCollection<SniperInfoModel> PokemonsInternal = new ObservableCollection<SniperInfoModel>();
    }
}