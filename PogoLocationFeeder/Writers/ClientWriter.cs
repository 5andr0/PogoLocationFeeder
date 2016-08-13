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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.Repository;

namespace PogoLocationFeeder.Writers
{
    public class ClientWriter
    {
        private readonly List<TcpClient> _arrSocket = new List<TcpClient>();
        public TcpListener Listener;

        public void StartNet(int port)
        {
            Log.Plain("PogoLocationFeeder is brought to you via https://github.com/5andr0/PogoLocationFeeder");
            Log.Plain("This software is 100% free and open-source.\n");
            Log.Plain("Please consider donating to if you like what we are doing!.");
            Log.Plain("You can find payment details on our GitHub page.\n");

            Log.Info("Application starting...");
            try
            {
                Listener = new TcpListener(IPAddress.Any, port);
                Listener.Start();
            }
            catch (SocketException e)
            {
                Log.Fatal($"Port {port} is already in use!", e);
                throw e;
            }

            StartAccept();
        }

        private void StartAccept()
        {
            Listener.BeginAcceptTcpClient(HandleAsyncConnection, Listener);
        }

        private void HandleAsyncConnection(IAsyncResult res)
        {
            StartAccept();
            var client = Listener.EndAcceptTcpClient(res);
            if (client != null && IsConnected(client.Client))
            {
                _arrSocket.Add(client);
                Log.Info($"New connection from {GetIp(client.Client)}");
            }
        }

        // A socket is still connected if a nonblocking, zero-byte Send call either:
        // 1) returns successfully or 
        // 2) throws a WAEWOULDBLOCK error code(10035)
        public static bool IsConnected(Socket client)
        {
            // This is how you can determine whether a socket is still connected.
            var blockingState = client.Blocking;

            try
            {
                var tmp = new byte[1];

                client.Blocking = false;
                client.Send(tmp, 0, 0);
                return true;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                return e.NativeErrorCode.Equals(10035);
            }
            finally
            {
                client.Blocking = blockingState;
            }
        }

        private static string GetIp(Socket s)
        {
            var remoteIpEndPoint = s.RemoteEndPoint as IPEndPoint;
            return remoteIpEndPoint?.ToString();
        }

        public async Task FeedToClients(List<SniperInfo> sniperInfos)
        {


            // Remove any clients that have disconnected
            if (GlobalSettings.ThreadPause) return;
            _arrSocket.RemoveAll(x => !IsConnected(x.Client));
            foreach (var target in sniperInfos)
            {


                foreach (var socket in _arrSocket)
                    // Repeat for each connected client (socket held in a dynamic array)
                {
                    try
                    {
                        var networkStream = socket.GetStream();
                        var s = new StreamWriter(networkStream);

                        s.WriteLine(JsonConvert.SerializeObject(target));
                        s.Flush();
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Caught exception", e);
                    }
                }
                // debug output
                if (GlobalSettings.Output != null)
                    GlobalSettings.Output.PrintPokemon(target);

                const string timeFormat = "HH:mm:ss";
                Log.Pokemon($"{target.ChannelInfo}: {target.Id} at {target.Latitude.ToString(CultureInfo.InvariantCulture)},{target.Longitude.ToString(CultureInfo.InvariantCulture)}"
                            + " with " + (!target.IV.Equals(default(double)) ? $"{target.IV}% IV" : "unknown IV")
                            +
                            (target.ExpirationTimestamp != default(DateTime)
                                ? $" until {target.ExpirationTimestamp.ToString(timeFormat)}"
                                : ""));
            }
        }
    }
}
