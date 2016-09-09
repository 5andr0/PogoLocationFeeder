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
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PogoLocationFeeder.API
{
    public class PogoLocationFeederListener
    {
        public delegate void PogoLocationFeederEventHandler(object sender, SniperInfoModel sniperInfo);

        public event PogoLocationFeederEventHandler EventHandler;

        public Task AsyncStart(string server, int port, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => Start(server, port, cancellationToken), cancellationToken);
        }

        private async Task Start(string server, int port, CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    using (var lClient = new TcpClient())
                    {
                        lClient.Connect(server, port);
                        using (var sr = new StreamReader(lClient.GetStream()))
                        {
                            while (lClient.Connected)
                            {
                                var line = sr.ReadLine();
                                if (line == null)
                                    throw new Exception("Unable to ReadLine from sniper socket");

                                var info = JsonConvert.DeserializeObject<SniperInfoModel>(line);
                                OnReceive(info);
                            }
                        }
                    }
                }
                catch (SocketException)
                {
                    // this is spammed to often. Maybe add it to debug log later
                }
                catch (Exception)
                {
                    // most likely System.IO.IOException
                }
                await Task.Delay(5000, cancellationToken);
            }
        }

        protected virtual void OnReceive(SniperInfoModel sniperInfo)
        {
            Task.Run(() =>
            {
                EventHandler?.Invoke(this, sniperInfo);
            });
        }
    }
}
