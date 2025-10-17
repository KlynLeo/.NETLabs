using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Book.Commands
{
    public class UpdateBookCommandHandler
    {
        private readonly ApplicationDBContext _context;

        public UpdateBookCommandHandler(ApplicationDBContext context) => _context = context;

        public async Task Handle(UpdateBookCommand command)
        {
            var book = await _context.Books.FindAsync(command.Id);
            if (book == null)
                throw new KeyNotFoundException($"Book with id {command.Id} not found.");

            book.Title = command.Title;
            book.Author = command.Author;
            book.Year = command.Year;

            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }
    }
}
