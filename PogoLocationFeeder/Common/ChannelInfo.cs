namespace PogoLocationFeeder.Helper
{
    public class ChannelInfo
    {
        public string server { get; set; }
        public string channel { get; set; }
        public bool isValid { get; set; } = false;

        public override string ToString()
        {
            if (channel != null && server != null)
            {
                return $"{server}:{channel}";
            }
            if (server != null)
            {
                return server;
            }
            if (channel != null)
            {
                return channel;
            }
            return "UNKNOWN";
        }
    }
}