using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Book.Commands
{
    public class CreateBookCommand
{
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public int Year { get; set; }
}
}