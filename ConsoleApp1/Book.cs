using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Book(string authors_first_name, string authors_last_name, string book_name, DateOnly book_release, int book_count)
    {
        public string Authors_first_name { get; } = authors_first_name;
        public string Authors_last_name { get; } = authors_last_name;
        public string Book_name { get; } = book_name;
        public DateOnly Book_release { get; } = book_release;
        public int Book_count { get; set; } = book_count;
    }
}
