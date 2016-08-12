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

namespace PogoLocationFeeder.Helper
{
    public class ChannelInfo
    {
        public string server { get; set; }
        public string channel { get; set; }
        public bool isValid { get; set; } = false;

        public override string ToString()
        {
            if (channel != null && server != null)
            {
                return $"{server}:{channel}";
            }
            if (server != null)
            {
                return server;
            }
            if (channel != null)
            {
                return channel;
            }
            return "UNKNOWN";
        }
    }
}
