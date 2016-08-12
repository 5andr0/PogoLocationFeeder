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

#region using directives

using System;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.Config;

#endregion

namespace PoGoLocationFeeder.Helper
{
    public class VersionCheckState
    {
        public const string LatestReleaseApi =
            "https://api.github.com/repos/5andr0/pogolocationfeeder/releases/latest";

        private const string LatestRelease =
            "https://github.com/5andr0/PogoLocationFeeder/releases";

        public static Version RemoteVersion;

        public static void Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = IsLatest();

            var updateCheckSucceeded = result.Item1;
            var needupdate = result.Item2;

            if (!updateCheckSucceeded)
            {
                Log.Info("Unable to check for updates! Likely hitting github rate limit (60 checks per hour)");
                return;
            }

            if (!needupdate)
            {
                Log.Info("Great! You already have the newest version (v{0}, or later master)",
                    RemoteVersion.ToString().Remove(RemoteVersion.ToString().Length - 2));
            }
            else
            {
                Log.Info("An update is available! Get the latest release at {0}", LatestRelease);
                if (GlobalSettings.Output != null)
                    GlobalSettings.Output.SetStatus($"Version outdated! {RemoteVersion} is available");
            }
        }

        private static string DownloadServerVersion()
        {
            using (var wC = new WebClient())
            {
                wC.Headers.Add("User-Agent", "PogoLocationFeeder");
                return wC.DownloadString(LatestReleaseApi);
            }
        }

        public static Tuple<bool, bool> IsLatest()
        {
            try
            {
                var regex = new Regex("\"tag_name\":\\Sv(.*?)\",");
                Match match = null;
                try
                {
                    match = regex.Match(DownloadServerVersion());
                }
                catch (Exception)
                {
                    return new Tuple<bool, bool>(false, false);
                }

                if (!match.Success)
                    return new Tuple<bool, bool>(false, false);
                var gitVersion = new Version($"{match.Groups[1]}.0");
                RemoteVersion = gitVersion;

                Log.Debug(
                    $"My version: {Assembly.GetExecutingAssembly().GetName().Version} (or a later master). Remote version: {RemoteVersion}.");

                if (gitVersion > Assembly.GetExecutingAssembly().GetName().Version)
                    return new Tuple<bool, bool>(true, true);
            }
            catch (Exception e)
            {
                Log.Fatal($"Version exception: {e}");
                return new Tuple<bool, bool>(false, false);
            }

            return new Tuple<bool, bool>(true, false);
        }
    }
}
