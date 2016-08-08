using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PogoLocationFeeder.Helper
{
    public class JsonSerializerSettingsCultureInvariant : JsonSerializerSettings
    {
        public JsonSerializerSettingsCultureInvariant()
        {
            Culture = CultureInfo.InvariantCulture;
        }
    }
}
