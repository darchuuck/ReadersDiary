using CURSOVA.Model;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using ReadingDiary.Model;
using System.Net;
using System.Net.Http.Headers;


namespace CURSOVA.Clients
{
    public class SearchBookClient
    {
        private readonly string _address;
        private readonly string _apiKey;
        private readonly string _apihost;

        public SearchBookClient()
        {
            _address = Constants.Address;
            _apiKey = Constants.ApiKey;
            _apihost = Constants.ApiHost;
        }

        public async Task<List<BookSearch>> GetBookByTitle(string title)
        {
            
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(($"{_address}/search/{title}")),
                Headers =
            {
                { "X-RapidAPI-Key", _apiKey},
                { "X-RapidAPI-Host", _apihost },
            },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<BookSearch>>(body);
                return result;
            }

        }
        public async Task<List<SearchBookRandom>> GetBooksByGenre(string genre)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{_address}/week/{genre}/100"),
                Headers =
            {
                { "X-RapidAPI-Key", _apiKey},
                { "X-RapidAPI-Host", _apihost },
            },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<List<SearchBookRandom>>(body);
                return results;
            }

        }
    }
}