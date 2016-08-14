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
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using PropertyChanged;

namespace autoupdate.ViewModels {
    [ImplementPropertyChanged]
    public class MainWindowViewModel {
        public string Status { get; set; } = "Getting latest...";
        public double Progress { get; set; } = 0;
        public string Size { get; set; }

        public string Name;
        public string Link;


        public MainWindowViewModel() {
            GetLatest();
        }

        public const string LatestReleaseApi =
            "https://api.github.com/repos/5andr0/pogolocationfeeder/releases/latest";
        public void Start(string name, string link) {
            if(!Directory.Exists("temp"))
                Directory.CreateDirectory("temp");
            var client = new WebClient();
            var ur = new Uri(link);
            client.DownloadFileCompleted += WebClientDownloadCompleted;
            client.DownloadProgressChanged += WebClientDownloadProgressChanged;
            Status = "Downloading";
            client.DownloadFileAsync(ur, Path.Combine(Directory.GetCurrentDirectory(), "temp", name));
        }
        void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            Progress = (double)e.ProgressPercentage;
            //Console.WriteLine("Download status: {0}%.", e.ProgressPercentage);
            Size = $"{e.BytesReceived / 1024}KB / {e.TotalBytesToReceive/ 1024}KB";
        }

        void WebClientDownloadCompleted(object sender, AsyncCompletedEventArgs e) {
            Status = "Download finished!";
            StartUnzip();
        }

        public void GetLatest() {
            string content;
            using(var wC = new WebClient()) {
                wC.Headers.Add("User-Agent", "PogoLocationFeeder");
                content = wC.DownloadString(LatestReleaseApi);
            }
            Name = Regex.Match(content, "\"name\":\"(PogoLocationFeeder.v.+zip)\",", RegexOptions.IgnoreCase).Groups[1].Value;
            Link = Regex.Match(content, "\"browser_download_url\":\"(https://github.com/5andr0/PogoLocationFeeder/releases/download/.+.zip)\"", RegexOptions.IgnoreCase).Groups[1].Value;

            Start(Name, Link);
        }

        public void StartUnzip() {
            Status = "Starting to unzip";
            try {
                using(var archive = ZipFile.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "temp", Name))) {
                    foreach(var entry in archive.Entries) {
                        if (entry.FullName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || entry.Name.EndsWith(".json")) {
                            var file = entry.Name;
                            if (entry.Name.EndsWith(".json")) {
                                file = $"Config\\default.{entry.Name}";
                            }
                            if(File.Exists(entry.Name))
                                File.Delete(entry.Name);
                            Size = $"{entry.Name}";
                            entry.ExtractToFile(Path.Combine(Directory.GetCurrentDirectory(), file));
                        }
                    }
                }
                Status = "Finished";
                Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), "temp"), true);
                Application.Current.Shutdown();
            } catch(Exception) {
                //
            }
        }
    }
}
