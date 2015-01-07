using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace SpotifyConsole {
    public class Song {
        public string Artist { get; set; }
        public string Title { get; set; }
        public double Duration { get; set; }

        public override string ToString() {
            return Artist + " - " + Title;
        }
    }

    class Program {
        public static List<string> invalid = new List<string>();
        public static HashSet<Song> songs = new HashSet<Song>();

        static void Main() {
            var testSong = new Song {
                Artist = "Metallica",
                Title = "One",
                Duration = 325
            };
            Console.WriteLine(testSong);
            songs.Add(testSong);

            //var textFileOutput = new List<string>();
            var textFileOutput = new List<string>();

            foreach (var song in songs) {
                // Call the API
                Response response = Get<Response>("search", "track", song.Artist + " " + song.Title);

                var orderedTracks = response.Tracks;
                Response.TrackInfo bestMatch = orderedTracks.FirstOrDefault();

                if (bestMatch == null) {
                    invalid.Add(song.ToString());
                    continue;
                }

                //textFileOutput.Add(bestMatch.Href);
                //textFileOutput.Add(response);
                System.Threading.Thread.Sleep(100); // limit API rate
            }

            //File.WriteAllLines("output.txt", textFileOutput);
            Console.WriteLine("Done!");
            Console.ReadLine();
        }
        
        public class Response {
            public class ResponseInfo {
                [JsonProperty("num_results")]
                public int NumResults { get; set; }

                public int Limit { get; set; }
                public int Offset { get; set; }
                public int Page { get; set; }
            }

            public class ArtistInfo {
                public string Name { get; set; }
            }

            public class TrackInfo {
                public string Name { get; set; }
                public string Href { get; set; }
                public double Length { get; set; }
                public IEnumerable<ArtistInfo> Artists { get; set; }
            }

            public ResponseInfo Info { get; set; }
            public IEnumerable<TrackInfo> Tracks { get; set; }
        }

        static T Get<T>(string service, string method, string parameters = null) where T : class {
            //var url = String.Format("http://ws.spotify.com/{0}/1/{1}", service, method);
            var url = String.Format("https://api.spotify.com/v1/search?q=metallica&type=artist", service, method);
            //if (!String.IsNullOrWhiteSpace(parameters)) {
            //    url += "?q=" + HttpUtility.UrlEncode(parameters);
            //}

            string text = null;
            bool done = false;
            while (!done) {
                try {
                    Console.WriteLine("Requesting: " + url);

                    var request = (HttpWebRequest)HttpWebRequest.Create(url);
                    request.Accept = "application/json";

                    var response = (HttpWebResponse)request.GetResponse();

                    using (var sr = new StreamReader(response.GetResponseStream())) {
                        text = sr.ReadToEnd();
                    }

                    done = true;
                }
                catch (WebException ex) {
                    Console.WriteLine("Exception: " + ex.Message);
                    Console.WriteLine("Retrying in 1 second...");
                    System.Threading.Thread.Sleep(1000);
                }
            }

            if (String.IsNullOrEmpty(text)) throw new InvalidOperationException();

            Log(text);
            return JsonConvert.DeserializeObject<T>(text);
        }

        static void Log(string text){
            File.WriteAllText("log.txt", text);
        }
    }
}
