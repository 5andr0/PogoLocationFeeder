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
using System.Net;
using System.Threading;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Helper;

namespace PogoLocationFeeder.Readers
{
    public class DiscordWebReader
    {
        public Stream stream;

        public DiscordWebReader()
        {
            InitializeWebClient();
        }

        //private WebClient Wc { get; set; }

        public void InitializeWebClient()
        {
            var request = WebRequest.Create(new Uri("http://pogo-feed.mmoex.com/messages"));
            ((HttpWebRequest) request).AllowReadStreamBuffering = false;

            try
            {
                var response = request.GetResponse();
                Log.Info($"Connection established. Waiting for data...");
                GlobalSettings.Output?.SetStatus($"Connected to discord feed");

                stream = response.GetResponseStream();
            }
            catch (WebException)
            {
                Log.Warn($"Experiencing connection issues. Throttling...");
                Thread.Sleep(30*1000);
            }
            catch (Exception e)
            {
                Log.Warn($"Exception: {e}\n\n\n");
            }
        }


        public class AuthorStruct
        {
            public string avatar;
            public string discriminator;
            public string id;
            public string username;
        }

        public class DiscordMessage
        {
            public string channel_id = "";
            //public List<AuthorStruct> author;
            public string content = "";
            public string id = "";
            public string timestamp = "";
            //public string edited_timestamp = null;
            public bool tts = false;
            //public string mentions = "";
            //public string nonce = "";
            //public bool deleted = false;
            //public bool pinned = false;
            //public bool mention_everyone = false;
            //public string mention_roles = "";
            //public xxx attachments = "";
            //public xxx embeds = "";
        }
    }
}
