using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using WebSocket4Net;

namespace PogoLocationFeederTests.Tests
{
    [TestClass]
    public class PokemobotWebsocketTest
    {
        [TestMethod]
        public void Test()
        {
            while (true)
            {
                    using (var client = new WebSocket("wss://localhost:14251", "basic", WebSocketVersion.Rfc6455))
                    {
                        System.Net.ServicePointManager.ServerCertificateValidationCallback =
                            (sender, certificate, chain, errors) => true;
  
                        client.AllowUnstrustedCertificate = false;
                        client.Opened += (s, e) =>
                        {
                        };

                        client.Closed += (s, e) =>
                        {
                        };
                        client.MessageReceived += (s, e) =>
                        {
    
                        };
                        client.Error += (s, e) =>
                        {

                        };
                        client.Open();
                    Thread.Sleep(100000);
                    }
            }
        }
    }
}
