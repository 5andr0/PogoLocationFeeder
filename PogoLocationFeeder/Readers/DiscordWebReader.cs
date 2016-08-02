using System;
using System.IO;
using System.Net;
using System.Threading;

namespace PogoLocationFeeder
{
    public class DiscordWebReader
    {
        WebClient wc { get; set; }

        public Stream stream = null;

        public DiscordWebReader()
        {
            InitializeWebClient();
        }

        public void InitializeWebClient()
        {
            var request = WebRequest.Create(new Uri("http://pogo-feed.mmoex.com/messages"));
            ((HttpWebRequest)request).AllowReadStreamBuffering = false;

            try
            {
                var response = request.GetResponse();
                Console.WriteLine($"Connection established. Waiting for data...");
                stream = response.GetResponseStream();
            }
            catch (WebException e)
            {
                Console.WriteLine($"Experiencing connection issues. Throttling...");
                Thread.Sleep(30 * 1000);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.ToString()}\n\n\n");
            }
        }

       
        public class AuthorStruct
        {
            public string username;
            public string id;
            public string discriminator;
            public string avatar;
        }
        public class DiscordMessage
        {
            public string id = "";
            public string channel_id = "";
            //public List<AuthorStruct> author;
            public string content = "";
            public string timestamp = "";
            //public string edited_timestamp = null;
            public bool tts = false;
            //public bool mention_everyone = false;
            //public bool pinned = false;
            //public bool deleted = false;
            //public string nonce = "";
            //public string mentions = "";
            //public string mention_roles = "";
            //public xxx attachments = "";
            //public xxx embeds = "";
        }
    }
}