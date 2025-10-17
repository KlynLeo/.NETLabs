using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;
using api.Model;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Dtos;

namespace api.Repository
{
    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDBContext _context;

        public BookRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync(BookQuery query)
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(query.Author))
            {
                books = books.Where(b => b.Author.Contains(query.Author));
            }

            switch (query.SortBy)
            {
                case "title":
                    books = query.Descending ? books.OrderByDescending(b => b.Title) : books.OrderBy(b => b.Title);
                    break;
                case "year":
                    books = query.Descending ? books.OrderByDescending(b => b.Year) : books.OrderBy(b => b.Year);
                    break;
                /*I only allowed sorting by title and year to prevent SQL Injection attacks or sorting by
                 arbitrary fields that I do not want to expose. 
                */
            }

            return await books.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
        }

        public async Task<Book> GetBookByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task AddBookAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBookAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
        }
    }
}