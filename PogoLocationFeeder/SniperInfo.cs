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
        public PokemonMove move1 { get; set; }
        public PokemonMove move2 { get; set; }

        public override string ToString()
        {
            return "SniperInfo: id: " + id
                + ", latitude: " + latitude 
                + ", longitude: " + longitude 
                + (iv != default(double) ? ", IV: " + iv + "%" : "")
                + (timeStamp != default (DateTime) ? ", expiration: " + timeStamp : "")
                + (move1 != PokemonMove.MoveUnset ? ", move1: " + move1 : "")
                + (move2 != PokemonMove.MoveUnset ? ", move2: " + move2 : "");

        }
    }
}