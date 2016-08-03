using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PogoLocationFeeder.Helper
{
    public class IVParser
    {
        public static double ParseIV(string input)
        {
            var iv = ParseRegexDouble(input,
                GeoCoordinatesParser.GeoCoordinatesRegex + @"\W+(?i)IV\W+(1?\d{1,3}[,.]?\d{0,3})\b");
                // 97,8.200341 IV 98
            //if (iv == default(double))
            //    iv = ParseRegexDouble(input, @"(?i)IV\b(1?\d{1,3}[,.]?\d{0,3})\W*" + GeoCoordinatesParser.GeoCoordinatesRegex); // 
            if (iv == default(double))
                iv = ParseRegexDouble(input, @"(?i)\b(1?\d{1,3}[,.]?\d{0,3})\W*\%?\W*IV"); // 52 IV 52% IV 52IV 52.5 IV
            if (iv == default(double))
                iv = ParseRegexDouble(input, @"(?i)\bIV\W?(1?\d{1,2}[,.]?\d{0,3})");
            if (iv == default(double))
                iv = ParseRegexDouble(input, @"\b(1?\d{1,3}[,.]?\d{0,3})\W*\%"); // 52% 52 %

            return iv;
        }

        private static double ParseRegexDouble(string input, string regex)
        {
            var match = Regex.Match(input, regex);
            if (match.Success)
            {
                return Convert.ToDouble(match.Groups[1].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            return default(double);
        }
    }
}