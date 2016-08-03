#region using directives

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Linq;
using PogoLocationFeeder.Helper;

#endregion

namespace PoGoLocationFeeder.Helper
{
    public class VersionCheckState
    {
        public const string VersionUri =
            "https://cdn.rawgit.com/5andr0/PogoLocationFeeder/master/PogoLocationFeeder/Properties/AssemblyInfo.cs";

        public const string LatestReleaseApi =
            "https://api.github.com/repos/5andr0/PogoLocationFeeder/releases/latest";

        private const string LatestRelease =
            "https://github.com/5andr0/PogoLocationFeeder/releases";

        public static Version RemoteVersion;

        public static async void Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var updateCheckSucceeded = IsLatest().Item1;
            var needupdate = IsLatest().Item2;

            if (!updateCheckSucceeded)
            {
                Log.Info("Unable to check for updates!");
                return;
            }

            if (!needupdate)
            {
                Log.Info("Great! You already have the newest version ({0})", RemoteVersion.ToString());
            }
            else
            {
                Log.Info("An update is available! Get the latest release at {0}", LatestRelease);
            }
            return;
        }

        private static string DownloadServerVersion()
        {
            using (var wC = new WebClient())
            {
                return wC.DownloadString(VersionUri);
            }
        }

        public static Tuple<bool, bool> IsLatest()
        {
            try
            {
                var regex = new Regex(@"\[assembly\: AssemblyVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]");
                var match = regex.Match(DownloadServerVersion());

                if (!match.Success)
                    return new Tuple<bool, bool>(false, false);

                var gitVersion = new Version($"{match.Groups[1]}.{match.Groups[2]}.{match.Groups[3]}.{match.Groups[4]}");
                RemoteVersion = gitVersion;

                Log.Debug($"Remote version: {RemoteVersion}. My version: {Assembly.GetExecutingAssembly().GetName().Version}");

                if (gitVersion >= Assembly.GetExecutingAssembly().GetName().Version)
                    return new Tuple<bool, bool>(true, false);
            }
            catch (Exception e)
            {
                Log.Fatal($"Version exception: {e.ToString()}");
                return new Tuple<bool, bool>(false, false); // Indicate that update check failed
            }

            return new Tuple<bool, bool>(true, true);
        }
    }
}