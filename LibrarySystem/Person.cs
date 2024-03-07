using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{
    public class Person
    {
        public string First_name { get; }
        public string Last_name { get; }
        public DateOnly Birthday { get; }
        public bool Status { get; }
        public List<Book> Borrowed_books { get; } = new();

        public Person(string first_name, string last_name, DateOnly birthday, bool status)
        {
            First_name = first_name;
            Last_name = last_name;
            Birthday = birthday;
            Status = status;
        }
    }
}
