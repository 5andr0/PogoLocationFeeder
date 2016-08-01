using System;
using POGOProtos.Enums;


namespace PogoLocationFeeder
{
    public class SniperInfo
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double iv { get; set; }
        public DateTime timeStamp { get; set; }
        public PokemonId id { get; set; }

        public override string ToString()
        {
            return "SniperInfo: id: " + id
                + ", latitude: " + latitude 
                + ", longitude: " + longitude 
                + (iv != default(double) ? ", IV: " + iv + "%" : "")
                + (timeStamp != default (DateTime) ? ", expiration: " + timeStamp : "");

        }
    }
}