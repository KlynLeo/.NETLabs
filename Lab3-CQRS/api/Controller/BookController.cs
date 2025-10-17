using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos;
using api.Interfaces;
using api.Model;
using Microsoft.AspNetCore.Mvc;

namespace api.Controller
{
    [ApiController]
    [Route("api/books")]
    public class BookController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public BookController(ApplicationDBContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetBooks([FromQuery] GetBooksQuery query)
        {
            var handler = new GetBooksQueryHandler(_context);
            var books = await handler.Handle(query);
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                throw new KeyNotFoundException($"Book with id {id} not found.");
            return Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookCommand command)
        {
            var handler = new CreateBookCommandHandler(_context);
            var id = await handler.Handle(command);
            return CreatedAtAction(nameof(GetBook), new { id }, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookCommand command)
        {
            if (id != command.Id)
                throw new ArgumentException("Route ID and book ID do not match.");
            var handler = new UpdateBookCommandHandler(_context);
            await handler.Handle(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var handler = new DeleteBookCommandHandler(_context);
            await handler.Handle(new DeleteBookCommand { Id = id });
            return NoContent();
        }
    }
}
