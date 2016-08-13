using PogoLocationFeeder.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PogoLocationFeeder.Client;
using PogoLocationFeeder.Common;
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
            sniperInfos.ForEach(s => s.ChannelInfo = new ChannelInfo() { server= Constants.PogoFeeder});
            var unsentInfos = MessageCache.Instance.FindUnSentMessages(sniperInfos);
            if (GlobalSettings.IsManaged)
            {
                foreach (var sniperInfo in unsentInfos)
                {
                    PogoClient.sniperInfosToSend.Enqueue(sniperInfo);
                }
            }
            if (!GlobalSettings.IsServer)
            {

                Task.Run(() => ClientWriter.Instance.FeedToClients(unsentInfos));
            }
            return unsentInfos.Any();
        }


        public bool BotCapture(SniperInfo sniperInfo)
        {
            var sniperInfos = new List<SniperInfo>() {sniperInfo};
            var unsentInfos = MessageCache.Instance.FindUnSentMessages(sniperInfos);
            if (GlobalSettings.IsManaged)
            {
                foreach (var unsent in unsentInfos)
                {
                    PogoClient.sniperInfosToSend.Enqueue(unsent);
                }
            }
            return unsentInfos.Any();

        }
    }
}
