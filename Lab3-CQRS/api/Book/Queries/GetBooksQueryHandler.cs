using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Book.Queries
{
    public class GetBooksQueryHandler
    {
        private readonly ApplicationDBContext _context;

        public GetBooksQueryHandler(ApplicationDBContext context) => _context = context;

        public async Task<IEnumerable<Book>> Handle(GetBooksQuery query)
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(query.Author))
                books = books.Where(b => b.Author.Contains(query.Author));

            books = query.SortBy?.ToLower() switch
            {
                "title" => query.Descending
                    ? books.OrderByDescending(b => b.Title)
                    : books.OrderBy(b => b.Title),
                "year" => query.Descending
                    ? books.OrderByDescending(b => b.Year)
                    : books.OrderBy(b => b.Year),
                _ => books.OrderBy(b => b.Id),
            };

            return await books
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();
        }
    }
}
