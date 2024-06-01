using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using TelegramBook.models;
using TelegramBook.DataBase;

namespace TelegramBook.Clients
{
    public class TelegramBookClient
    {
        private readonly string _address;
        private readonly string _apiKey;
        private readonly string _apihost;
        private readonly Data _data;

        public TelegramBookClient(Data data)
        {
            _address = Constants.Address;
            _apiKey = Constants.ApiKey;
            _apihost = Constants.ApiHost;
            _data = data;
        }

        public async Task<List<BookTel>> GetBookByTitle(string title)
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
                var result = JsonConvert.DeserializeObject<List<BookTel>>(body);
                return result;
            }

        }
        public async Task<List<SearchBookRandomTel>> GetBooksByGenre(string genre)
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
                var results = JsonConvert.DeserializeObject<List<SearchBookRandomTel>>(body);
                return results;
            }


        }

        public async Task AddBook(BookTel book, string tableName)
        {
            await _data.InsertBooks(book, tableName);
        }

        public async Task<List<BookTel>> GetBooks(long userId, string tableName)
        {
            return await _data.SelectBooks(userId, tableName);
        }

        public async Task DeleteBook(string title, long userId, string tableName)
        {
            await _data.DeleteBook(title, userId, tableName);
        }
    }
}
