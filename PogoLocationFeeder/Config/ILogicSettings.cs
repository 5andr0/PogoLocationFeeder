namespace PoGo.LocationFeeder.LogicSettings
{
    public class ServerSettings
    {
        public ServerSettings(ulong serverId, string channel, string discordToken, int port)
        {
            ServerId = serverId;
            ServerChannel = channel;
            DiscordToken = discordToken;
            Port = port;
        }
        public ulong ServerId { get; set; }
        public string ServerChannel  { get; set; }
        public string DiscordToken { get; set; }
        public int Port { get; set; }
    }

    public interface ILogicSettings
    {
        ulong ServerId { get; }
        string ServerChannel { get; }
        string DiscordToken { get; }
        int Port { get; }
    }
}
