using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Telegramm.models;

namespace Telegramm.Clients
{
    public class TelegramBookClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseAddress;

        public TelegramBookClient(string baseAddress)
        {
            _httpClient = new HttpClient();
            _baseAddress = baseAddress;
        }

        public async Task<List<BookTel>> SearchBooksAsync(string name)
        {
            var response = await _httpClient.GetFromJsonAsync<List<BookTel>>($"{_baseAddress}/Book/search?name={name}");
            return response;
        }

        public async Task<List<SearchBookRandomTel>> GetRandomBookByGenreAsync(string genre)
        {
            var response = await _httpClient.GetFromJsonAsync<List<SearchBookRandomTel>>($"{_baseAddress}/Book/random?genre={genre}");
            return response;
        }

        public async Task AddBookAsync(BookTel book, string tableName)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseAddress}/MyBooks/add?tableName={tableName}", book);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<BookTel>> GetUserBooksAsync(long userId, string tableName)
        {
            var response = await _httpClient.GetFromJsonAsync<List<BookTel>>($"{_baseAddress}/MyBooks/user_books?userId={userId}&tableName={tableName}");
            return response;
        }

        public async Task DeleteBookAsync(string title, long userId, string tableName)
        {
            var response = await _httpClient.DeleteAsync($"{_baseAddress}/MyBooks/delete?title={title}&userId={userId}&tableName={tableName}");
            response.EnsureSuccessStatusCode();
        }
    }
}
