using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Person(string first_name, string last_name, DateOnly birthday, bool status)
    {
        public string First_name { get; } = first_name;
        public string Last_name { get; } = last_name;
        public DateOnly Birthday { get; } = birthday;
        public bool Status { get; } = status;
        public List<Book> Borrowed_books { get; } = new();

    }
}
