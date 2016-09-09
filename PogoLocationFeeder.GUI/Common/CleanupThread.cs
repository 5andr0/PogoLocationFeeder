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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Helper;

namespace PogoLocationFeeder.GUI.Common {
    public static class CleanupThread {
        public static void Start() {
            while(true) {
                try {
                    foreach (var poke in GlobalVariables.PokemonsInternal) {
                        var ukn = "";
                        var expiration = poke.Info.ExpirationTimestamp;
                        if(expiration.Equals(default(DateTime))) {
                            expiration = poke.Created.AddMinutes(GlobalSettings.RemoveAfter);
                            ukn = "unk. ";
                        }
                        var remaining = expiration - DateTime.Now;

                        if(remaining < TimeSpan.Zero) {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                                GlobalVariables.PokemonsInternal.Remove(poke);
                            }));
                            //Log.Debug($"Closin Thread with {poke.Info.Id} {poke.Info.Latitude} {poke.Info.Longitude} in it.");
                            //return;
                        }
                            poke.Date = $"{ukn}{remaining.Minutes}m {remaining.Seconds}s";
                    }
                    Thread.Sleep(1000);
                } catch(Exception) {
                    Thread.Sleep(1000);
                    //hmm ignore?
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}
