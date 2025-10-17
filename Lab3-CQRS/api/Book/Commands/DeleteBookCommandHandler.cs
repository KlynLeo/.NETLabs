using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Book.Commands
{
    public class DeleteBookCommandHandler
    {
        private readonly ApplicationDBContext _context;

        public DeleteBookCommandHandler(ApplicationDBContext context) => _context = context;

        public async Task Handle(DeleteBookCommand command)
        {
            var book = await _context.Books.FindAsync(command.Id);
            if (book == null)
                throw new KeyNotFoundException($"Book with id {command.Id} not found.");

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }
    }
}
