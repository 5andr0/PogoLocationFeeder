using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Api.Tests
{
    [TestClass()]
    public class PogoLocationFeederListenerTests
    {
        const int port = 16959;
        TcpListener listener;
        private List<TcpClient> arrSocket = new List<TcpClient>();

        [TestInitialize]
        public void Setup()
        {
            Task.Run(() =>
            {
                try
                {
                    listener = new TcpListener(IPAddress.Any, port);
                    listener.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Could open port {port}", e);
                    throw e;
                }
                listener.BeginAcceptTcpClient(HandleAsyncConnection, listener);
            });
        }

        List<SniperInfoModel> receivedSniperInfos = new List<SniperInfoModel>();

        [TestMethod()]
        public void AsyncStartTest()
        {
            PogoLocationFeederListener pogoLocationFeederListener =  new PogoLocationFeederListener();
            pogoLocationFeederListener.eventHandler += (sender, sniperInfo) =>
            {
                //Implement your code here
                System.Console.WriteLine("SniperInfo received");
                receivedSniperInfos.Add(sniperInfo);
            };
            pogoLocationFeederListener.AsyncStart("localhost", port);
            Thread.Sleep(500);
            SendToClients(createSniperInfo());
            Thread.Sleep(100);
            Assert.IsTrue(receivedSniperInfos.Any());
            Assert.AreEqual(POGOProtos.Enums.PokemonId.Abra, receivedSniperInfos[0].Id);
            Assert.AreEqual(12.345, receivedSniperInfos[0].Latitude);
            Assert.AreEqual(-98.765, receivedSniperInfos[0].Longitude);
            Assert.AreEqual(95.6, receivedSniperInfos[0].IV);
        }

        private SniperInfo createSniperInfo()
        {
            SniperInfo sniperInfo = new SniperInfo();
            sniperInfo.Id = POGOProtos.Enums.PokemonId.Abra;
            sniperInfo.Latitude = 12.345;
            sniperInfo.Longitude = -98.765;
            sniperInfo.IV = 95.6;
            return sniperInfo;
        }

        private void StartAccept()
        {
            listener.BeginAcceptTcpClient(HandleAsyncConnection, listener);
        }

        private void HandleAsyncConnection(IAsyncResult res)
        {
            StartAccept();
            TcpClient client = listener.EndAcceptTcpClient(res);
            if (client != null && IsConnected(client.Client))
            {
                arrSocket.Add(client);
                Console.WriteLine($"New connection");
            }
        }

        public static bool IsConnected(Socket client)
        {
            // This is how you can determine whether a socket is still connected.
            bool blockingState = client.Blocking;

            try
            {
                byte[] tmp = new byte[1];

                client.Blocking = false;
                client.Send(tmp, 0, 0);
                return true;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                return (e.NativeErrorCode.Equals(10035));
            }
            finally
            {
                client.Blocking = blockingState;
            }
        }

        private void SendToClients(SniperInfo sniperInfo)
        {
            foreach (var socket in arrSocket) // Repeat for each connected client (socket held in a dynamic array)
            {
                try
                {
                    NetworkStream networkStream = socket.GetStream();
                    StreamWriter s = new StreamWriter(networkStream);

                    s.WriteLine(JsonConvert.SerializeObject(sniperInfo));
                    s.Flush();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Caught exception: {e.ToString()}");
                }
            }
        }
    }
}