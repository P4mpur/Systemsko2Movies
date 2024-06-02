using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WebServer
{
    class Program
    {
        const string api_key = "5fee2d373de0ac00a9db64cff3cad54d";
        static readonly HttpClient client = new HttpClient();
        static readonly Dictionary<string, byte[]> cache = new Dictionary<string, byte[]>();

        public static async Task Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8084/");
            listener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    _ = Task.Run(() => ProcessRequestAsync(context));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        static async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                if (request.HttpMethod == "GET")
                {
                    string query = Regex.Replace(request.Url.AbsolutePath, "%20", "+").TrimStart('/');
                    if (string.IsNullOrEmpty(query))
                    {
                        SendResponse(response, "<h1>Query cannot be empty</h1>");
                        return;
                    }

                    if (!cache.TryGetValue(query, out byte[] buffer))
                    {
                        Console.WriteLine("Using API:");
                        var data = await SearchMovieByNameAsync(query);
                        if (data == null || data["results"].Count() == 0)
                        {
                            SendResponse(response, "<h1>No movies found</h1>");
                            return;
                        }

                        string htmlContent = GenerateHtmlContent(data);
                        buffer = Encoding.UTF8.GetBytes(htmlContent);
                        lock (cache)
                        {
                            cache[query] = buffer;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Using Cache");
                    }

                    response.ContentLength64 = buffer.Length;
                    response.ContentType = "text/html; charset=UTF-8";
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    response.OutputStream.Close();

                    Console.WriteLine("Response sent");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Close();
            }
        }

        static async Task<JObject> SearchMovieByNameAsync(string query)
        {
            string url = $"https://api.themoviedb.org/3/search/movie?query={query}&api_key={api_key}";

            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string res_body = await response.Content.ReadAsStringAsync();
                    return JObject.Parse(res_body);
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        static void SendResponse(HttpListenerResponse response, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            response.ContentLength64 = buffer.Length;
            response.ContentType = "text/html; charset=UTF-8";
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        static string GenerateHtmlContent(JObject data)
        {
            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<!DOCTYPE html>")
                       .Append("<html lang=\"en\">")
                       .Append("<head><meta charset=\"UTF-8\"><title>Movie Overview</title></head>")
                       .Append("<body><h1>Welcome to MOVIE OVERVIEW</h1><p>Results:</p>")
                       .Append("<div>");

            foreach (var item in data["results"])
            {
                htmlBuilder.Append($"<h1>Original Title : {item["original_title"]}</h1>")
                           .Append($"<p>ID: {item["id"]}</p>")
                           .Append($"<p>Overview : {item["overview"]}</p>")
                           .Append($"<img src=\"https://image.tmdb.org/t/p/w500{item["poster_path"]}\" ></img>");
            }

            htmlBuilder.Append("</div></body></html>");
            return htmlBuilder.ToString();
        }
    }
}
