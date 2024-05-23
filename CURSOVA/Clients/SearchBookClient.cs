using CURSOVA.Model;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using ReadingDiary.DataBase;
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
        private readonly Data _data;

        public SearchBookClient(Data data)
        {
            _address = Constants.Address;
            _apiKey = Constants.ApiKey;
            _apihost = Constants.ApiHost;
            _data = data;
        }

        public async Task<List<Book>> GetBookByTitle(string title)
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
                var result = JsonConvert.DeserializeObject<List<Book>>(body);
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

        //public async Task AddReadBook(Book book)
        //{
        //    await _data.InsertBooks(book,"read__books");
        //}

        //public async Task<List<Book>> GetReadBook(long userId)
        //{
        //    return await _data.SelectBooks(userId, "read__books");
        //}

        //public async Task AddFavoriteBook(Book book)
        //{
        //    await _data.InsertBooks(book, "favorite__books");
        //}

        //public async Task<List<Book>> GetFavoriteBook(long userId)
        //{
        //    return await _data.SelectBooks(userId, "favorite__books");
        //}
        //public async Task DeleteFavoriteBook(string title, long userId)
        //{
        //    await _data.DeleteBook(title, userId, "favorite__books");
        //}

        public async Task AddBook(Book book, string tableName)
        {
            await _data.InsertBooks(book, tableName);
        }

        public async Task<List<Book>> GetBooks(long userId, string tableName)
        {
            return await _data.SelectBooks(userId, tableName);
        }

        public async Task DeleteBook(string title, long userId, string tableName)
        {
            await _data.DeleteBook(title, userId, tableName);
        }
    }


}