using Botukbooks.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static Botukbooks.TelegramBookBot;



namespace Botukbooks.Client

{
    public class BookApiClient
    {
        private readonly HttpClient _httpClient;
    
        private readonly ILogger<BookApiClient> _logger;

        public BookApiClient(HttpClient httpClient, ILogger<BookApiClient> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(Constants.Address);
            _logger = logger;
        }

        public async Task<List<BookTel>> SearchBooksAsync(string name)
        {
            _logger.LogInformation("Searching books with name: {Name}", name);
            var response = await _httpClient.GetAsync($"/Book/search?name={name}");
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Received response for books search with name: {Name}", name);
            return await response.Content.ReadFromJsonAsync<List<BookTel>>();
        }

        public async Task<SearchBookRandomTel> GetRandomBookByGenreAsync(string genre)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/Book/random?genre={genre}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var book = JsonConvert.DeserializeObject<SearchBookRandomTel>(content);

                return book;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new GenreNotFoundException($"No books found for genre: {genre}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving random book by genre: {ex.Message}");
                throw;
            }
        }

        public async Task<string> AddBookAsync(BookTel book, string tableName)
        {
            try
            {
               
                var requestUrl = $"/MyBooks/add?tableName={tableName}";
                var jsonContent = JsonConvert.SerializeObject(book);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(requestUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return "Book added successfully.";
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return $"Failed to add book. Status code: {response.StatusCode}, Error: {errorMessage}";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<List<BookTel>> GetUserBooksAsync(long userId, string tableName)
        {
            _logger.LogInformation("Getting user books for userId: {UserId} from table: {TableName}", userId, tableName);
            var response = await _httpClient.GetAsync($"/MyBooks/user's_books?userId={userId}&tableName={tableName}");
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Received response for user books for userId: {UserId} from table: {TableName}", userId, tableName);
            return await response.Content.ReadFromJsonAsync<List<BookTel>>();
        }

        public async Task DeleteBookAsync(string title, long userId, string tableName)
        {
            _logger.LogInformation("Deleting book: {Title} for userId: {UserId} from table: {TableName}", title, userId, tableName);
            var response = await _httpClient.DeleteAsync($"/MyBooks/delete?title={title}&userId={userId}&tableName={tableName}");
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Book deleted successfully");
        }
    }
}

