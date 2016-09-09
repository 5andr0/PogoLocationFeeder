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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using PogoLocationFeeder.Server;

namespace PogoLocationFeeder.Helper
{
    public class MessageCache
    {
        public static readonly MessageCache Instance = new MessageCache();
        public readonly SniperInfoRepository _clientRepository;
        private readonly SniperInfoRepositoryManager _sniperInfoRepositoryManager;

        private MessageCache()
        {
            _clientRepository = new SniperInfoRepository();
            _sniperInfoRepositoryManager = new SniperInfoRepositoryManager(_clientRepository);

        }

        public List<SniperInfo> FindUnSentMessages(List<SniperInfo> sniperInfos)
        {
            return sniperInfos.Where(sniperInfo => !IsSentAlready(sniperInfo)).ToList();
        }

        private bool IsSentAlready(SniperInfo sniperInfo)
        {
            return !_sniperInfoRepositoryManager.AddToRepository(sniperInfo);
        }

        internal void Add(SniperInfo sniperInfo)
        {
            _sniperInfoRepositoryManager.AddToRepository(sniperInfo);
        }
    }
}
