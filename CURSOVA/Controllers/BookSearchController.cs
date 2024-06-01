using CURSOVA.Clients;
using CURSOVA.Model;
using Microsoft.AspNetCore.Mvc;
using CURSOVA.DataBase;

using static System.Reflection.Metadata.BlobBuilder;



namespace CURSOVA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookController : ControllerBase
    {
     
        private readonly ILogger<BookController> _logger;
        private readonly Data _data;
        private readonly SearchBookClient _searchBookClient;


        public BookController(ILogger<BookController> logger, Data data)
        {
            _logger = logger;
            _data = data;
            _searchBookClient = new SearchBookClient(_data);

        }

        [HttpGet("search")]
        public ActionResult<List<Book>> GetBook(string name)
        {
            List<Book> book = _searchBookClient.GetBookByTitle(name).Result;
            if (book == null || book.Count == 0)
            {
                return NotFound("No books found for the specified name.");
            }

            return book;
        }
        [HttpGet("random")]
        public ActionResult<SearchBookRandom> GetRandomBookByGenre(string genre)
        {
            List<SearchBookRandom> books = _searchBookClient.GetBooksByGenre(genre).Result;

            if (books == null || books.Count == 0)
            {
                return NotFound("No books found for the specified genre.");
            }

            Random random = new Random();
            SearchBookRandom randomBook = books[random.Next(books.Count)];

            return randomBook;


        }



    }
}
