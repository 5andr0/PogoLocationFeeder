using System;
using System.IO;
using System.Net;
using System.Threading;
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