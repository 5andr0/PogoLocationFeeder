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
