using System;
using POGOProtos.Enums;


namespace PogoLocationFeeder
{
    public class SniperInfo
    {
        public ulong EncounterId { get; set; }
        public DateTime ExpirationTimestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public PokemonId Id { get; set; }
        public string SpawnPointId { get; set; }
        public PokemonMove Move1 { get; set; }
        public PokemonMove Move2 { get; set; }
        public double IV { get; set; }

        public override string ToString()
        {
            return "SniperInfo: id: " + Id
                + ", latitude: " + Latitude 
                + ", longitude: " + Longitude 
                + (IV != default(double) ? ", IV: " + IV + "%" : "")
                + (ExpirationTimestamp != default (DateTime) ? ", expiration: " + ExpirationTimestamp : "");

        }
    }
}
