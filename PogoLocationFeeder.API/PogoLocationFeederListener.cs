using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Threading;

namespace PogoLocationFeeder.Api
{
    public class PogoLocationFeederListener
    {
        public delegate void PogoLocationFeederEventHandler(object sender, SniperInfoModel sniperInfo);

        public event PogoLocationFeederEventHandler eventHandler;

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
                if (eventHandler != null)
                {
                    eventHandler(this, sniperInfo);
                }
            });
        }

    }
}