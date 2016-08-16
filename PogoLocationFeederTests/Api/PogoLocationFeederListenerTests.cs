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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PogoLocationFeeder.API;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;

namespace PogoLocationFeederTests.Api
{
    [TestClass]
    public class PogoLocationFeederListenerTests
    {
        private const int port = 16959;
        private readonly List<TcpClient> _arrSocket = new List<TcpClient>();
        private TcpListener _listener;

        private readonly List<SniperInfoModel> _receivedSniperInfos = new List<SniperInfoModel>();

        [TestInitialize]
        public void Setup()
        {
            Task.Run(() =>
            {
                try
                {
                    _listener = new TcpListener(IPAddress.Any, port);
                    _listener.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Could open port {port}", e);
                    throw e;
                }
                _listener.BeginAcceptTcpClient(HandleAsyncConnection, _listener);
            });
        }

        [TestMethod]
        public void AsyncStartTest()
        {
            var pogoLocationFeederListener = new PogoLocationFeederListener();
            pogoLocationFeederListener.EventHandler += (sender, sniperInfo) =>
            {
                //Implement your code here
                Console.WriteLine("SniperInfo received");
                _receivedSniperInfos.Add(sniperInfo);
            };
            pogoLocationFeederListener.AsyncStart("localhost", port);
            Thread.Sleep(500);
            SendToClients(createSniperInfo());
            Thread.Sleep(100);
            Assert.IsTrue(_receivedSniperInfos.Any());
            Assert.AreEqual(PokemonId.Abra, _receivedSniperInfos[0].Id);
            Assert.AreEqual(12.345, _receivedSniperInfos[0].Latitude);
            Assert.AreEqual(-98.765, _receivedSniperInfos[0].Longitude);
            Assert.AreEqual(95.6, _receivedSniperInfos[0].IV);
        }

        private SniperInfo createSniperInfo()
        {
            var sniperInfo = new SniperInfo();
            sniperInfo.Id = PokemonId.Abra;
            sniperInfo.Latitude = 12.345;
            sniperInfo.Longitude = -98.765;
            sniperInfo.IV = 95.6;
            return sniperInfo;
        }

        private void StartAccept()
        {
            _listener.BeginAcceptTcpClient(HandleAsyncConnection, _listener);
        }

        private void HandleAsyncConnection(IAsyncResult res)
        {
            StartAccept();
            var client = _listener.EndAcceptTcpClient(res);
            if (client != null && IsConnected(client.Client))
            {
                _arrSocket.Add(client);
                Console.WriteLine($"New connection");
            }
        }

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

        private void SendToClients(SniperInfo sniperInfo)
        {
            foreach (var socket in _arrSocket) // Repeat for each connected client (socket held in a dynamic array)
            {
                try
                {
                    var networkStream = socket.GetStream();
                    var s = new StreamWriter(networkStream);

                    s.WriteLine(JsonConvert.SerializeObject(sniperInfo));
                    s.Flush();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Caught exception: {e}");
                }
            }
        }
    }
}
