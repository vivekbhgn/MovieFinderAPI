using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIInterfaceApplication
{
    class Program
    {
        static List<RootObject> results = new List<RootObject>();

        static void Main(string[] args)
        {
            List<char> delimiters = new List<char> { '.', ' ', '_', '-' };
            List<char> brackets = new List<char> { '{', '}', '[', ']', '(', ')' };


            string fileName = "Gol Maal.DVDScr.XVID.AC3.HQ.Hive-CM8";

            Dictionary<int, string> queryWithPriority = new Dictionary<int, string>();
            queryWithPriority.Add(1, fileName);
            StringBuilder title = new StringBuilder();
            StringBuilder titleSep = new StringBuilder();
            List<string> titleAppended = new List<string>();
            List<string> wordsInTitle = new List<string>();
            int priority = 1;
            foreach (char c in fileName)
            {
                if (delimiters.Contains(c))
                {

                    // queryWithPriority.Add(++priority, title.ToString());
                    title.Append(" ");
                    titleAppended.Add(title.ToString());
                    wordsInTitle.Add(titleSep.ToString());
                    titleSep = new StringBuilder();
                    //title = new StringBuilder();
                }
                else if (!brackets.Contains(c))
                {
                    titleSep.Append(c);
                    title.Append(c);
                }
            }
            titleAppended.OrderByDescending(t => t.Length).ToList().ForEach(w =>
            {
                queryWithPriority.Add(++priority, w);
            });
            wordsInTitle.ForEach(w =>
                {
                    queryWithPriority.Add(++priority, w);
                });


            ParallelOptions p = new ParallelOptions();
            p.MaxDegreeOfParallelism = 6;

            Parallel.ForEach(queryWithPriority, p, k =>
                {
                    GetMovieData(k.Value, k.Key, priority);
                });

            var allTitles = results.Where(r => r.results != null).SelectMany(r => r.results);
            Dictionary<string, double> titlesWithPriority = new Dictionary<string, double>();
            foreach (var t in allTitles)
            {
                if (titlesWithPriority.ContainsKey(t.title + t.id))
                {
                    var valuePriority = titlesWithPriority.Where(ti => ti.Key == t.title + t.id).First().Value;

                    titlesWithPriority.Remove(t.title + t.id);

                    titlesWithPriority.Add(t.title + t.id, t.priority + valuePriority);
                }
                else
                {
                    titlesWithPriority.Add(t.title + t.id, t.priority);
                }
            }

            var finalTitlePriority = titlesWithPriority.Max(t => t.Value);

            var finalTitle = titlesWithPriority.Where(t => t.Value == finalTitlePriority);
        }

        enum OperationType
        {
            Search = 1
        }

        enum EntityType
        {
            Movie = 1
        }


        static void GetMovieData(string query, int priority, int maxPriority)
        {
            var baseURI = "https://api.themoviedb.org/3/search/movie";
            // https://api.themoviedb.org/3/search/movie?api_key=8d04f6928132343ce7e2328800310daa&query=Children


            var client = new RestClient();
            client.BaseUrl = new Uri(baseURI);

            var request = new RestRequest(Method.GET);
            //request.Resource = "/movie";
            //request.RootElement = "search";

            request.AddParameter("api_key", "8d04f6928132343ce7e2328800310daa");
            request.AddParameter("query", query);
            IRestResponse response = client.Execute(request);

            RootObject deserializedProduct = JsonConvert.DeserializeObject<RootObject>(response.Content);
            var raisedPriority = Math.Pow(2, maxPriority - priority);
            if (deserializedProduct.results != null && deserializedProduct.results.Any())
            {
                deserializedProduct.results.ForEach(r => r.priority = raisedPriority);
            }
            results.Add(deserializedProduct);
        }

    }


    public class Result
    {
        public double priority { get; set; }

        public string poster_path { get; set; }
        public bool adult { get; set; }
        public string overview { get; set; }
        public string release_date { get; set; }
        public List<object> genre_ids { get; set; }
        public int id { get; set; }
        public string original_title { get; set; }
        public string original_language { get; set; }
        public string title { get; set; }
        public string backdrop_path { get; set; }
        public double popularity { get; set; }
        public int vote_count { get; set; }
        public bool video { get; set; }
        public double vote_average { get; set; }
    }

    public class RootObject
    {
        public int page { get; set; }
        public List<Result> results { get; set; }
        public int total_results { get; set; }
        public int total_pages { get; set; }
    }
}
