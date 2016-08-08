using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PogoLocationFeeder.Helper
{
    public class JsonSerializerSettingsFactory
    {

        public static JsonSerializerSettings create()
        {
            return new JsonSerializerSettings { Culture = CultureInfo.InvariantCulture };
        }
    }
}
