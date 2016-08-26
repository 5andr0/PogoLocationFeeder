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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PogoLocationFeeder.Helper
{
    public class JsonSerializerSettingsCultureInvariant : JsonSerializerSettings
    {
        public JsonSerializerSettingsCultureInvariant()
        {
            Culture = CultureInfo.InvariantCulture;
            Converters = new List<JsonConverter> { new DoubleConverter()};
        }
    }

    public class DoubleConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(double) || objectType == typeof(double?));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                return token.ToObject<double>();
            }
            if (token.Type == JTokenType.String)
            {
                var match = Regex.Match(token.ToString(), @"(1?\-?\d+\.?\d*)");
                if (match.Success)
                {
                    return Double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                }
                return Double.Parse(token.ToString(),
                       System.Globalization.CultureInfo.InvariantCulture);
            }
            if (token.Type == JTokenType.Null && objectType == typeof(double?))
            {
                return null;
            }
            throw new JsonSerializationException("Unexpected token type: " +
                                                  token.Type.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

}
