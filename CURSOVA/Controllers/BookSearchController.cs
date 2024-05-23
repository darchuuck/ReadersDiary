using CURSOVA.Clients;
using CURSOVA.Model;
using Microsoft.AspNetCore.Mvc;
using ReadingDiary.Model;
using static System.Reflection.Metadata.BlobBuilder;



namespace CURSOVA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookController : ControllerBase
    {
     
        private readonly ILogger<BookController> _logger;


        public BookController(ILogger<BookController> logger)
        {
            _logger = logger;
        }

        //[HttpGet("search")]
        //public ActionResult<List<Book>> GetBook(string name)
        //{
            
        //    SearchBookClient searchBookClient = new SearchBookClient();
        //    List<Book> book = searchBookClient.GetBookByTitle(name).Result;
        //    if (book == null || book.Count == 0)
        //    {
        //        return NotFound("No books found for the specified name.");
        //    }
           
        //    return book;
        //}
        //[HttpGet("random")]
        //public ActionResult<SearchBookRandom> GetRandomBookByGenre(string genre)
        //{
            
        //    SearchBookClient searchBookClient = new SearchBookClient();
        //    List<SearchBookRandom> books = searchBookClient.GetBooksByGenre(genre).Result;

        //    if (books == null || books.Count == 0)
        //    {
        //        return NotFound("No books found for the specified genre.");
        //    }

        //    Random random = new Random();
        //    SearchBookRandom randomBook = books[random.Next(books.Count)];

        //    return randomBook;

          
        //}



    }
}
