using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Book.Commands
{
    public class CreateBookCommandHandler
    {
        private readonly ApplicationDBContext _context;

        public CreateBookCommandHandler(ApplicationDBContext context) => _context = context;

        public async Task<int> Handle(CreateBookCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Title))
                throw new ArgumentException("Title cannot be empty.");
            if (string.IsNullOrWhiteSpace(command.Author))
                throw new ArgumentNullException(nameof(command.Author), "Author cannot be null.");

            var exists = await _context.Books.AnyAsync(b =>
                b.Title == command.Title && b.Author == command.Author
            );
            if (exists)
                throw new InvalidOperationException(
                    "A book with the same title and author already exists."
                );

            var book = new Book
            {
                Title = command.Title,
                Author = command.Author,
                Year = command.Year,
            };
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
            return book.Id;
        }
    }
}
