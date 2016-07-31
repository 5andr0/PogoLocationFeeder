using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Helper
{
    public class IVParser
    {
        public static double parseIV(string input)
        {
            double iv;
            iv = parseRegexDouble(input, @"(?i)\b(1?\d{1,3}[,.]?\d{0,3})\W*\%?\W*IV"); // 52 IV 52% IV 52IV 52.5 IV
            if (iv == default(double))
                iv = parseRegexDouble(input, @"(?i)\bIV\W?(1?\d{1,2}[,.]?\d{0,3})");
            if (iv == default(double))
                iv = parseRegexDouble(input, @"\b(1?\d{1,3}[,.]?\d{0,3})\W*\%?"); // 52% 52 %

            return iv;
        }

        private static double parseRegexDouble(string input, string regex)
        {
            Match match = Regex.Match(input, regex);
            if (match.Success)
            {
                return Convert.ToDouble(match.Groups[1].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            else
                return default(double);
        }
    }

}
