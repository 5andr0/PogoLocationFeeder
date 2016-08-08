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
