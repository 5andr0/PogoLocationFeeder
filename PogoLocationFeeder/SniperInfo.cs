using System;
using POGOProtos.Enums;


namespace PogoLocationFeeder
{
    public class SniperInfo
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double iv { get; set; }
        public DateTime expirationTime { get; set; }
        public DateTime creationTime { get; set; }
        public PokemonId id { get; set; }

        public SniperInfo()
        {
            creationTime = DateTime.Now;
        }
    }
}