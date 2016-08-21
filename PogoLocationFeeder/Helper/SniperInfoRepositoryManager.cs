using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Server;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Helper
{
    public class SniperInfoRepositoryManager
    {
        private readonly SniperInfoRepository _sniperInfoRepository;
        const string timeFormat = "HH:mm:ss";

        public SniperInfoRepositoryManager(SniperInfoRepository sniperInfoRepository)
        {
            _sniperInfoRepository = sniperInfoRepository;
        }

        public bool AddToRepository(SniperInfo sniperInfo)
        {
            var oldSniperInfo = _sniperInfoRepository.Find(sniperInfo);
            if (oldSniperInfo != null)
            {
                if (sniperInfo.ChannelInfo != null && sniperInfo.ChannelInfo.server == Constants.Bot)
                {
                    AddCaptureExisting(oldSniperInfo, sniperInfo);
                }
                else
                {
                    AddDuplicateDiscovery(oldSniperInfo, sniperInfo);
                }
                return false;
            }
            else
            {
                AddNew(sniperInfo);
                return true;
            }
        }

        private void AddCaptureExisting(SniperInfo oldSniperInfo, SniperInfo sniperInfo)
        {
            if (PokemonId.Missingno.Equals(oldSniperInfo.Id))
            {
                oldSniperInfo.Id = sniperInfo.Id;
            }
            if (oldSniperInfo.IV == 0 && sniperInfo.IV != 0)
            {
                oldSniperInfo.IV = sniperInfo.IV;
            }
            if (!oldSniperInfo.Verified)
            {
                if (sniperInfo.Verified)
                {
                    oldSniperInfo.Verified = sniperInfo.Verified;
                    oldSniperInfo.VerifiedOn = DateTime.Now;
                }
            }
            if (oldSniperInfo.ExpirationTimestamp == default(DateTime))
            {
                if (sniperInfo.ExpirationTimestamp != default(DateTime))
                {
                    oldSniperInfo.VerifiedOn = sniperInfo.ExpirationTimestamp;
                }
            }
            if (oldSniperInfo.EncounterId == default(ulong))
            {
                if (sniperInfo.EncounterId != default(ulong))
                {
                    oldSniperInfo.EncounterId = sniperInfo.EncounterId;
                }
            }
            if (oldSniperInfo.Move1 == PokemonMove.MoveUnset)
            {
                if (sniperInfo.Move1 != PokemonMove.MoveUnset)
                {
                    oldSniperInfo.Move1 = sniperInfo.Move1;
                }
            }
            if (oldSniperInfo.Move2== PokemonMove.MoveUnset)
            {
                if (sniperInfo.Move2 != PokemonMove.MoveUnset)
                {
                    oldSniperInfo.Move2 = sniperInfo.Move2;
                }
            }
            if (oldSniperInfo.SpawnPointId == default(string))
            {
                if (sniperInfo.SpawnPointId != default(string))
                {
                    oldSniperInfo.SpawnPointId = sniperInfo.SpawnPointId;
                }
            }
            oldSniperInfo.OtherChannelInfos.Add(sniperInfo.ChannelInfo);
            var captures = _sniperInfoRepository.Increase(oldSniperInfo);
            Log.Pokemon($"Captured existing: {FormatPokemonLog(oldSniperInfo, sniperInfo.ChannelInfo, captures)}");
        }

        private void AddDuplicateDiscovery(SniperInfo oldSniperInfo, SniperInfo sniperInfo)
        {
            bool updated = false;
            if (PokemonId.Missingno.Equals(oldSniperInfo.Id) && !PokemonId.Missingno.Equals(sniperInfo.Id))
            {
                oldSniperInfo.Id = sniperInfo.Id;
                updated = true;
            }
            if (oldSniperInfo.IV == 0 && sniperInfo.IV != 0)
            {
                oldSniperInfo.IV = sniperInfo.IV;
                updated = true;
            }
            if (oldSniperInfo.ExpirationTimestamp == default(DateTime) &&
                sniperInfo.ExpirationTimestamp != default(DateTime))
            {
                oldSniperInfo.ExpirationTimestamp = sniperInfo.ExpirationTimestamp;
                updated = true;
            }
            if (sniperInfo.ChannelInfo !=null && !oldSniperInfo.GetAllChannelInfos().Any(ci => object.Equals(ci.server, sniperInfo.ChannelInfo.server)
            && (object.Equals(ci.channel, sniperInfo.ChannelInfo.channel))))
            {
                oldSniperInfo.OtherChannelInfos.Add(sniperInfo.ChannelInfo);
                updated = true;
            }
            if (updated)
            {
                var captures = _sniperInfoRepository.Update(oldSniperInfo);
                Log.Debug($"Discovered (DUP): {FormatPokemonLog(oldSniperInfo, sniperInfo.ChannelInfo, captures)}");
            }
        }

        private void AddNew(SniperInfo sniperInfo)
        {
            var captures = _sniperInfoRepository.Update(sniperInfo);
            Log.Pokemon($"Discovered: {FormatPokemonLog(sniperInfo,  sniperInfo.ChannelInfo, captures)}");
            if (GlobalSettings.Output != null)
                GlobalSettings.Output.PrintPokemon(sniperInfo);
        }

        private static string FormatPokemonLog(SniperInfo sniperInfo, ChannelInfo channelInfo, int captures)
        {
            return $"{channelInfo}: {sniperInfo.Id} at {sniperInfo.Latitude.ToString("N6", CultureInfo.InvariantCulture)},{sniperInfo.Longitude.ToString("N6", CultureInfo.InvariantCulture)}"
                   + " with " +
                   (!sniperInfo.IV.Equals(default(double))
                       ? $"{sniperInfo.IV}% IV"
                       : "unknown IV")
                   + (sniperInfo.Move1 != PokemonMove.MoveUnset ? $" Move1: {sniperInfo.Move1.ToString()}" : "")
                   + (sniperInfo.Move2 != PokemonMove.MoveUnset ? $" Move2: {sniperInfo.Move2.ToString()}" : "")
                   + (sniperInfo.EncounterId != default(ulong) ? $" EncounterId: {sniperInfo.EncounterId.ToString()}" : "")
                   + (sniperInfo.ExpirationTimestamp != default(DateTime)
                       ? $" until {sniperInfo.ExpirationTimestamp.ToString(timeFormat)}"
                       : "") + $", Captures {captures}";
        }
    }
}
