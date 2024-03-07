using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{
    public class Book
    {
        public string Authors_first_name { get; }
        public string Authors_last_name { get; }
        public string Book_name { get; }
        public string Genre { get; }
        public DateOnly Book_release { get; }
        public int Book_count { get; set; }

        public Book(string authors_first_name, string authors_last_name, string book_name, string genre, DateOnly book_release, int book_count)
        {
            Authors_first_name = authors_first_name;
            Authors_last_name = authors_last_name;
            Book_name = book_name;
            Genre = genre;
            Book_release = book_release;
            Book_count = book_count;
        }
    }
}
