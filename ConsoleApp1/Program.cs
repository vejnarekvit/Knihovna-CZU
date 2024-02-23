using System.Security.Cryptography;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // setup person and availible books
            var p1 = new Person("Vít", "Vejnárek", new DateOnly(2003,9,12), true);
            var books = new List<Book>();
            books.Add(new Book("Petr", "Bezruč", "Slezské písně", new DateOnly(2000, 12, 12), 22));
            books.Add(new Book("Karel", "Čapek", "Válka s mloky", new DateOnly(2006, 5, 4), 2));
            books.Add(new Book("Karel", "Čapek", "Krakatit", new DateOnly(2007, 10, 10), 15));
            books.Add(new Book("Viktor", "Dyk", "Krysař", new DateOnly(1989, 9, 10), 7));
            char user_response;


            do 
            {
                Console.Clear();
                Console.WriteLine("Vítek's Library");
                Console.WriteLine("------------------------");
                Console.WriteLine("Borrow the book [b]");
                Console.WriteLine("Give back the book [g]");
                Console.WriteLine("Show my profile information [i]");
                Console.WriteLine("Delete my profile [d]");
                Console.WriteLine("End program [e]");
                Console.Write("Type action: ");
                user_response = char.ToLower(Console.ReadKey().KeyChar);

                switch (user_response)
                {
                    case 'b':
                        // Borrow the book
                        Console.Clear();
                        Console.WriteLine("Books ready to borrow");
                        Console.WriteLine("-------------------------");
                        foreach(var book in books)
                        {
                            if (book.Book_count != 0)
                            {
                                Console.WriteLine($"- {book.Book_name} [{books.IndexOf(book)}] - books left {book.Book_count}");
                            }
                        }
                        Console.WriteLine("");
                        Console.WriteLine("(To get back press any button except the books)");
                        Console.Write("Which of these book?: ");

                        // Try parse to check if users response is number
                        var user_response_borrow = Console.ReadLine();
                        if (int.TryParse(user_response_borrow, out int user_response_borrow_value) == false)
                        {
                            break;
                        }

                        // create integer from users response
                        int user_response_borrow_integer = int.Parse(user_response_borrow);
                        if (user_response_borrow_integer >= 0 && user_response_borrow_integer < books.Count)
                        {
                            books[user_response_borrow_integer].Book_count = books[user_response_borrow_integer].Book_count - 1;
                            p1.Borrowed_books.Add(books[user_response_borrow_integer]);
                        }
                        break;
                    case 'g':
                        // Give back the book
                        Console.Clear();
                        Console.WriteLine("Borrowed books:");
                        foreach (var book in p1.Borrowed_books)
                        {
                            Console.WriteLine($"- {book.Book_name} [{books.IndexOf(book)}]");
                        }
                        Console.WriteLine("");
                        Console.WriteLine("(To get back press any button except the books)");
                        Console.Write("Which of these book?: ");
                        // Try parse to check if users response is number
                        var user_response_back = Console.ReadLine();
                        if (int.TryParse(user_response_back, out int user_response_back_value) == false)
                        {
                            break;
                        }

                        // create integer from users response
                        int user_response_back_int = int.Parse(user_response_back);
                        if (user_response_back_int >= 0 && user_response_back_int < books.Count)
                        {
                            books[user_response_back_int].Book_count = books[user_response_back_int].Book_count + 1;
                            p1.Borrowed_books.Remove(books[user_response_back_int]);
                        }
                        break;
                    case 'i':
                        // Show my profile information
                        Console.Clear();
                        Console.WriteLine($"First name: {p1.First_name}");
                        Console.WriteLine($"Last name: {p1.Last_name}");
                        Console.WriteLine($"Birthday: {p1.Birthday}");
                        Console.WriteLine($"Student: {p1.Status}");
                        Console.WriteLine("Borrowed books:");
                        foreach (var book in p1.Borrowed_books)
                        {
                            Console.WriteLine($"   - {book.Book_name}");
                        }
                        Console.WriteLine("");
                        Console.WriteLine("Return - press any button");
                        Console.ReadKey();
                        break;
                    case 'd':
                        // Delete my profile
                        Console.Clear();
                        Console.WriteLine("Do you really want to delete your profile? [y][n]");
                        char user_response_delete = char.ToLower(Console.ReadKey().KeyChar);
                        if (user_response_delete == 'y')
                        {
                            if (p1.Borrowed_books.Count > 0)
                            {
                                Console.WriteLine("First you have to give back the books!");
                                Console.ReadKey();
                                break;
                            }
                            // delete acc
                        }
                        break;
                }
            } while (user_response != 'e');
        }
    }

    public class Person(string first_name, string last_name, DateOnly birthday, bool status)
    {
        public string First_name { get; } = first_name;
        public string Last_name { get; } = last_name;
        public DateOnly Birthday { get; } = birthday;
        public bool Status { get; } = status;
        public List<Book> Borrowed_books { get; } = new();

    }

    public class Book(string authors_first_name, string authors_last_name, string book_name, DateOnly book_release, int book_count)
    {
        public string Authors_first_name { get; } = authors_first_name;
        public string Authors_last_name { get; } = authors_last_name;
        public string Book_name { get; } = book_name;
        public DateOnly Book_release { get; } = book_release;
        public int Book_count { get; set; } = book_count;
    }
}
