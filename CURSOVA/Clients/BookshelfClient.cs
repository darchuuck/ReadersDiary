using CURSOVA.Model;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace CURSOVA.Clients
{
    public class GoogleBooksClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GoogleBooksClient(string apiKey)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.googleapis.com/books/v1/users/")
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _apiKey = apiKey;
        }

        public async Task<Bookshelf> CreateBookshelf(string userId, string title, string description = null)
        {
            var bookshelf = new Bookshelf
            {
                title = title,
                description = description,
                access = "public" 
            };

            var json = JsonConvert.SerializeObject(bookshelf);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{userId}/bookshelves?key={_apiKey}", content);

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var createdBookshelf = JsonConvert.DeserializeObject<Bookshelf>(responseBody);

            return createdBookshelf;
        }
    }
}

