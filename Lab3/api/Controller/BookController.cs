using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using api.Model;
using api.Interfaces;
using api.Dtos;

namespace api.Controller
{
    [ApiController]
    [Route("api/books")]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;

        public BookController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks([FromQuery] BookQuery query)
        {
            return Ok(await _bookRepository.GetAllBooksAsync(query));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _bookRepository.GetBookByIdAsync(id);
            if (book == null)
                throw new KeyNotFoundException($"Book with id {id} not found."); 

            return Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] Book book)
        {
            if (string.IsNullOrWhiteSpace(book.Title))
                throw new ArgumentException("Title cannot be empty."); 

            if (string.IsNullOrWhiteSpace(book.Author))
                throw new ArgumentNullException(nameof(book.Author), "Author cannot be null.");

            var exists = (await _bookRepository.GetAllBooksAsync(new BookQuery()))
                .Any(b => b.Title == book.Title && b.Author == book.Author);
            if (exists)
                throw new InvalidOperationException("A book with the same title and author already exists."); 

            await _bookRepository.AddBookAsync(book);
            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            if (id != book.Id)
                throw new ArgumentException("Route ID and book ID do not match."); 
            await _bookRepository.UpdateBookAsync(book);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _bookRepository.GetBookByIdAsync(id);
            if (book == null)
                throw new KeyNotFoundException($"Book with id {id} not found."); 

            await _bookRepository.DeleteBookAsync(id);
            return NoContent();
        }
    }
}
