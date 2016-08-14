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
            if (GlobalSettings.IsManaged)
            {
                foreach (var unsent in sniperInfos)
                {
                    PogoClient.sniperInfosToSend.Enqueue(unsent);
                }
            }
            return sniperInfos.Any();
        }
    }
}
