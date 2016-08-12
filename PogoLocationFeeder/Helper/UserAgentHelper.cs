using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using Microsoft.Win32;
using System.Net.Security;

namespace PogoLocationFeeder.Helper
{
    class UserAgentHelper
    {
        private static Random _rand;
        private static Random Rand
        {
            get
            {
                if (_rand == null)
                    _rand = new Random();
                return _rand;
            }
        }

        public static string GetRandomUseragent()
        {
            switch (Rand.Next(5))
            {
                case 0:
                    return xNet.Http.OperaMiniUserAgent();
                case 1:
                    return xNet.Http.FirefoxUserAgent();
                case 2:
                    return xNet.Http.ChromeUserAgent();
                case 3:
                    return xNet.Http.OperaUserAgent();
                default:
                    return xNet.Http.IEUserAgent();
            }
        }
    }
}

/* The following license applies to the below xNet code (source: https://github.com/X-rus/xNet/)
 * 
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
namespace xNet
{
    public static class Http
    {
        [ThreadStatic]
        private static Random _rand;
        private static Random Rand
        {
            get
            {
                if (_rand == null)
                    _rand = new Random();
                return _rand;
            }
        }

        #region User Agent

        public static string IEUserAgent()
        {
            string windowsVersion = RandomWindowsVersion();

            string version = null;
            string mozillaVersion = null;
            string trident = null;
            string otherParams = null;

            if (windowsVersion.Contains("NT 5.1"))
            {
                version = "9.0";
                mozillaVersion = "5.0";
                trident = "5.0";
                otherParams = ".NET CLR 2.0.50727; .NET CLR 3.5.30729";
            }
            else if (windowsVersion.Contains("NT 6.0"))
            {
                version = "9.0";
                mozillaVersion = "5.0";
                trident = "5.0";
                otherParams = ".NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.30729";
            }
            else
            {
                switch (Rand.Next(3))
                {
                    case 0:
                        version = "10.0";
                        trident = "6.0";
                        mozillaVersion = "5.0";
                        break;

                    case 1:
                        version = "10.6";
                        trident = "6.0";
                        mozillaVersion = "5.0";
                        break;

                    case 2:
                        version = "11.0";
                        trident = "7.0";
                        mozillaVersion = "5.0";
                        break;
                }

                otherParams = ".NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E";
            }

            return
                $"Mozilla/{mozillaVersion} (compatible; MSIE {version}; {windowsVersion}; Trident/{trident}; {otherParams})";
        }


        public static string OperaUserAgent()
        {
            string version = null;
            string presto = null;

            switch (Rand.Next(4))
            {
                case 0:
                    version = "12.16";
                    presto = "2.12.388";
                    break;

                case 1:
                    version = "12.14";
                    presto = "2.12.388";
                    break;

                case 2:
                    version = "12.02";
                    presto = "2.10.289";
                    break;

                case 3:
                    version = "12.00";
                    presto = "2.10.181";
                    break;
            }

            return $"Opera/9.80 ({RandomWindowsVersion()}); U) Presto/{presto} Version/{version}";
        }


        public static string ChromeUserAgent()
        {
            string version = null;
            string safari = null;

            switch (Rand.Next(5))
            {
                case 0:
                    version = "41.0.2228.0";
                    safari = "537.36";
                    break;

                case 1:
                    version = "41.0.2227.1";
                    safari = "537.36";
                    break;

                case 2:
                    version = "41.0.2224.3";
                    safari = "537.36";
                    break;

                case 3:
                    version = "41.0.2225.0";
                    safari = "537.36";
                    break;

                case 4:
                    version = "41.0.2226.0";
                    safari = "537.36";
                    break;
            }

            return
                $"Mozilla/5.0 ({RandomWindowsVersion()}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{version} Safari/{safari}";
        }


        public static string FirefoxUserAgent()
        {
            string gecko = null;
            string version = null;

            switch (Rand.Next(5))
            {
                case 0:
                    version = "36.0";
                    gecko = "20100101";
                    break;

                case 1:
                    version = "33.0";
                    gecko = "20100101";
                    break;

                case 2:
                    version = "31.0";
                    gecko = "20100101";
                    break;

                case 3:
                    version = "29.0";
                    gecko = "20120101";
                    break;

                case 4:
                    version = "28.0";
                    gecko = "20100101";
                    break;
            }

            return $"Mozilla/5.0 ({RandomWindowsVersion()}; rv:{version}) Gecko/{gecko} Firefox/{version}";
        }


        public static string OperaMiniUserAgent()
        {
            string os = null;
            string miniVersion = null;
            string version = null;
            string presto = null;

            switch (Rand.Next(3))
            {
                case 0:
                    os = "iOS";
                    miniVersion = "7.0.73345";
                    version = "11.62";
                    presto = "2.10.229";
                    break;

                case 1:
                    os = "J2ME/MIDP";
                    miniVersion = "7.1.23511";
                    version = "12.00";
                    presto = "2.10.181";
                    break;

                case 2:
                    os = "Android";
                    miniVersion = "7.5.54678";
                    version = "12.02";
                    presto = "2.10.289";
                    break;
            }

            return $"Opera/9.80 ({os}; Opera Mini/{miniVersion}/28.2555; U; ru) Presto/{presto} Version/{version}";
        }

        #endregion

        private static string RandomWindowsVersion()
        {
            var windowsVersion = "Windows NT ";

            switch (Rand.Next(5))
            {
                case 0:
                    windowsVersion += "5.1"; // Windows XP
                    break;

                case 1:
                    windowsVersion += "6.0"; // Windows Vista
                    break;

                case 2:
                    windowsVersion += "6.1"; // Windows 7
                    break;
                case 3:
                    windowsVersion += "6.2"; // Windows 8
                    break;
                default:
                    windowsVersion += "10.0"; // Windows 8
                    break;
            }

            if (Rand.NextDouble() < 0.2)
            {
                windowsVersion += "; WOW64"; // 64-bit.
            }

            return windowsVersion;
        }
    }
}
