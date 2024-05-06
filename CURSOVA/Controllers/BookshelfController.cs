using CURSOVA.Clients;
using CURSOVA.Model;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookshelfController : ControllerBase
    {
        private readonly GoogleBooksClient _googleBooksClient;

        public BookshelfController(GoogleBooksClient googleBooksClient)
        {
            _googleBooksClient = googleBooksClient;
        }

        [HttpPost("{userId}")]
        public async Task<ActionResult<Bookshelf>> CreateBookshelf(string userId, [FromBody] Bookshelf bookshelf)
        {
            var createdBookshelf = await _googleBooksClient.CreateBookshelf(userId, bookshelf.title, bookshelf.description);
            return Ok(createdBookshelf);
        }
    }
}
