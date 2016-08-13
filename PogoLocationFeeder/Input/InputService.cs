using PogoLocationFeeder.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PogoLocationFeeder.Client;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Writers;

namespace PogoLocationFeeder.Input
{
    public class InputService
    {

        public static readonly InputService Instance = new InputService();

        private InputService() { }

        public bool ParseAndSend(string text)
        {
            List<SniperInfo> sniperInfos = MessageParser.ParseMessage(text);
            sniperInfos.ForEach(s => s.ChannelInfo = new ChannelInfo() { server= FilterFactory.PogoFeeder});

            if (GlobalSettings.IsManaged)
            {
                foreach (var sniperInfo in sniperInfos)
                {
                    PogoClient.sniperInfosToSend.Enqueue(sniperInfo);
                }
            }
            if (!GlobalSettings.IsServer)
            {
                Task.Run(() => ClientWriter.Instance.FeedToClients(sniperInfos));
            }
            return sniperInfos.Any();

        }
    }
}
