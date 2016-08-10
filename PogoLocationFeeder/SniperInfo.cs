using System;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Helper
{
    public class SniperInfo
    {
        public ulong EncounterId { get; set; }
        public DateTime ExpirationTimestamp { get; set; } = default(DateTime);
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public PokemonId Id { get; set; } = PokemonId.Missingno;
        public string SpawnPointId { get; set; }
        public PokemonMove Move1 { get; set; }
        public PokemonMove Move2 { get; set; }
        public double IV { get; set; }
        public bool Verified { get; set; } = false;

        public override string ToString()
        {
            return "SniperInfo: id: " + Id
                   + ", Latitude: " + Latitude
                   + ", Longitude: " + Longitude
                   + (IV != default(double) ? ", IV: " + IV + "%" : "")
                   + (ExpirationTimestamp != default(DateTime) ? ", expiration: " + ExpirationTimestamp : "");
        }
    }
}