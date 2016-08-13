using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Client
{
    public class Filter
    {
        public string pokemon { get; set; }
        public List<Channel> channels;
    }

    public class Channel
    {
        public string server { get; set; }
        public string channel { get; set; }
    }
}
