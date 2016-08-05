using PogoLocationFeeder.Helper;

namespace PogoLocationFeeder.Common
{
    public interface IOutput
    {
        void PrintPokemon(SniperInfo info, ChannelInfo channelInfo);
        void SetStatus(string message);
        void RemoveListExtras();
    }
}