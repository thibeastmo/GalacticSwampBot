using System.Net.Http;
using System.Threading.Tasks;
namespace GalacticSwampBot.Library {
    public class Requester {
        public static async Task<string> GetRequest(string url)
        {
            url = AdaptForApi(url);
            using (HttpClient http = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Accept", "application/json");
                HttpResponseMessage response = http.SendAsync(request).Result;
                if (response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    // status-code: 2xx
                    return await response.Content.ReadAsStringAsync();

                }
            }
            return null;
        }
        private static string AdaptForApi(string value)
        {
            return value.Replace(" ", "%20");
        }
    }
}
