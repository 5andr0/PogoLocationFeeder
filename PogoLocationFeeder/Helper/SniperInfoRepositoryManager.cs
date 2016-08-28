/*
PogoLocationFeeder gathers pokemon data from various sources and serves it to connected clients
Copyright (C) 2016  PogoLocationFeeder Development Team <admin@pokefeeder.live>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Globalization;
using System.Linq;
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
            if (sniperInfo.ReceivedTimeStamp > DateTime.Now || sniperInfo.VerifiedOn > DateTime.Now
                || sniperInfo.ExpirationTimestamp > DateTime.Now.AddMinutes(20))
            {
                return false;
            }
            var oldSniperInfo = _sniperInfoRepository.Find(sniperInfo);
            if (oldSniperInfo != null)
            {
                if (sniperInfo.ChannelInfo != null && sniperInfo.ChannelInfo.server == Constants.Bot)
                {
                    if (!ValidateVerifiedSniperInfo(oldSniperInfo))
                    {
                        AddCaptureExisting(oldSniperInfo, sniperInfo);
                    }
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
            if (oldSniperInfo.IV <= 0 && sniperInfo.IV > 0)
            {
                oldSniperInfo.IV = sniperInfo.IV;
            }
            if (!oldSniperInfo.Verified)
            {
                if (sniperInfo.Verified)
                {
                    oldSniperInfo.Verified = sniperInfo.Verified;
                    oldSniperInfo.VerifiedOn = DateTime.Now;
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
                    if (oldSniperInfo.Move2 == PokemonMove.MoveUnset)
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
                }
                if (sniperInfo.ChannelInfo != null &&
                    !oldSniperInfo.GetAllChannelInfos()
                        .Any(ci => object.Equals(ci.server, sniperInfo.ChannelInfo.server)
                                    && (object.Equals(ci.channel, sniperInfo.ChannelInfo.channel))))
                {
                    oldSniperInfo.OtherChannelInfos.Add(sniperInfo.ChannelInfo);
                }
            }
            var captures = _sniperInfoRepository.Increase(oldSniperInfo);
            Log.Pokemon($"Captured existing: {FormatPokemonLog(oldSniperInfo, captures)}");
        }

        private bool ValidateVerifiedSniperInfo(SniperInfo sniperInfo)
        {
            return !PokemonId.Missingno.Equals(sniperInfo.Id)
                && sniperInfo.Verified
                && sniperInfo.EncounterId != default(ulong)
                && sniperInfo.Move1 != PokemonMove.MoveUnset
                && sniperInfo.Move2 != PokemonMove.MoveUnset
                && sniperInfo.SpawnPointId != null
                && sniperInfo.IV > 0;
        }

        private void AddDuplicateDiscovery(SniperInfo oldSniperInfo, SniperInfo sniperInfo)
        {
            bool updated = false;
            if (PokemonId.Missingno.Equals(oldSniperInfo.Id) && !PokemonId.Missingno.Equals(sniperInfo.Id))
            {
                oldSniperInfo.Id = sniperInfo.Id;
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
                Log.Debug($"Discovered (DUP): {FormatPokemonLog(oldSniperInfo, captures)}");
            }
        }

        private void AddNew(SniperInfo sniperInfo)
        {
            if(sniperInfo.ExpirationTimestamp > DateTime.Now.AddHours(2) || sniperInfo.ExpirationTimestamp < DateTime.Now)
            {
                sniperInfo.ExpirationTimestamp = default(DateTime);
            }
            var captures = _sniperInfoRepository.Update(sniperInfo);
            Log.Pokemon($"Discovered: {FormatPokemonLog(sniperInfo,  captures)}");
            if (GlobalSettings.Output != null)
                GlobalSettings.Output.PrintPokemon(sniperInfo);
        }

        private static string FormatPokemonLog(SniperInfo sniperInfo, int captures)
        {
            return $"{sniperInfo.ChannelInfo}: {sniperInfo.Id} at {sniperInfo.Latitude.ToString("N6", CultureInfo.InvariantCulture)},{sniperInfo.Longitude.ToString("N6", CultureInfo.InvariantCulture)}"
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
