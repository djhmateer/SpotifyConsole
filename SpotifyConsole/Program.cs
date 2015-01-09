using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json.Linq;

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
            ArtistsResponse result = CallAPIAndDeserialise("metallica");

            var href = result.Href;
            var total = result.Total;
            var items = result.Items;
            Console.WriteLine(href);
            Console.WriteLine(total);

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

        public class ArtistsResponse {
            public string Href { get; set; }
            public int Total { get; set; }

            public List<Artist> Items { get; set; }

            public class Artist{
                public string Id { get; set; }
                public List<SpotifyImage> Images { get; set; }
                public string Name { get; set; }

                public class SpotifyImage{
                    public int Height { get; set; }
                    public string Url { get; set; }
                    public int Width { get; set; }
                }
            }
        }

        static ArtistsResponse CallAPIAndDeserialise(string parameter = null) {
            string json = CallAPI(parameter);

            var jsonNoArtistsRootElement = JObject.Parse(json)["artists"].ToString();
            var result = JsonConvert.DeserializeObject<ArtistsResponse>(jsonNoArtistsRootElement);
            
            return result;
        }

        private static string CallAPI(string parameter) {
            if (!String.IsNullOrWhiteSpace(parameter)) parameter = HttpUtility.UrlEncode(parameter);

            var url = String.Format("https://api.spotify.com/v1/search?q={0}&type=artist", parameter);

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
            return text;
        }

        static void Log(string text) {
            File.WriteAllText("log.txt", text);
        }
    }
}
