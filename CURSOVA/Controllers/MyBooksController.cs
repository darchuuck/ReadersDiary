using Microsoft.AspNetCore.Mvc;
using CURSOVA.Clients;
using CURSOVA.Model;
using Microsoft.AspNetCore.Mvc;
using CURSOVA.DataBase;

namespace CURSOVA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MyBooksController : ControllerBase
    {

        private readonly ILogger<MyBooksController> _logger;
        private readonly Data _data;
        private readonly SearchBookClient _searchBookClient;


        public MyBooksController(ILogger<MyBooksController> logger, Data data)
        {
            _logger = logger;
            _data = data;
            _searchBookClient = new SearchBookClient(_data);

        }
        // Метод для додавання книги в прочитанні або улюблені
        [HttpPost("add")]
        public async Task<IActionResult> AddBook([FromQuery] string tableName, [FromBody] Book book)
        {
            try
            {
                _logger.LogInformation("Attempting to add book: {Book} to table: {TableName}", book.Name, tableName);
                await _searchBookClient.AddBook(book, tableName);
                _logger.LogInformation("Book added successfully to table: {TableName}", tableName);
                return Ok("Book added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding book to table: {TableName}", tableName);
                return StatusCode(500, "Internal server error");
            }
        }

        // Метод для отримання книг улюблених або прочитаних користувача
        [HttpGet("user's_books")]
        public async Task<ActionResult<List<Book>>> GetUserBooks([FromQuery] long userId, [FromQuery] string tableName)
        {
            var books = await _searchBookClient.GetBooks(userId, tableName);
            if (books == null || books.Count == 0)
            {
                return NotFound("No books found for the specified user.");
            }

            return books;
        }

        // Метод для видалення книги з улюблених
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteBook([FromQuery] string title, [FromQuery] long userId, [FromQuery] string tableName)
        {
            await _searchBookClient.DeleteBook(title, userId, tableName);
            return Ok("Book deleted successfully.");
        }
    }

}