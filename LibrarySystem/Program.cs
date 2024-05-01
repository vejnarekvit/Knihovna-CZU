using LibrarySystem;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using static System.Reflection.Metadata.BlobBuilder;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LibrarySystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /*
                DB actions
                book.AddBookToDatabase(connection)
                DatabaseHelper.GetAllBooks();
                person.AddPersonToDatabase(connection);
                DatabaseHelper.GetAllUsers();
            */
            DatabaseHelper.InitializeDatabase();
            Auth();
            var user = UserManager.CurrentUser;
            var people = DatabaseHelper.GetAllUsers();
            
            Console.Clear();

            if (user != null)
            {
                Console.WriteLine("Vítek's Library");
                Console.WriteLine("------------------------");
                Console.WriteLine($"Welcome user  {user.First_name} {user.Last_name}! We are glad to see you!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();

                if (user.Status == 0)
                {
                    UserStatus0Actions();
                } else if (user.Status == 1)
                {
                    UserStatus1Actions();
                } else
                {
                    Console.WriteLine("Your Status has wrong value, please contact an administrator.");
                }
            } else
            {
                Console.WriteLine("You are not logged in!");
            }
        }

        public static void UserStatus0Actions()
        {
            var books = DatabaseHelper.GetAllBooks();
            var availableBooks = DatabaseHelper.GetAllAvailableBooks();
            char user_response;
            var user = UserManager.CurrentUser;

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
                        List<int> availableBooksId = new List<int>();
                        availableBooks = DatabaseHelper.GetAllAvailableBooks();
                        foreach (Book book in availableBooks)
                        {
                            Console.WriteLine($"{book.Book_name} [{book.ID}]");
                            availableBooksId.Add(book.ID);
                        }

                  
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
                        if (availableBooksId.Contains(user_response_borrow_integer))
                        {
                            DatabaseHelper.BorrowBook(user.ID, user_response_borrow_integer);
                        }
                        break;
                    case 'g':
                        // Give back the book
                        Console.Clear();
                        Console.WriteLine("Return the book");
                        Console.WriteLine("-------------------------");
                        List<Book> BooksToReturn = DatabaseHelper.GetBorrowedBooks(user.ID);
                        List<int> returnBooksId = new List<int>();

                        foreach (var book in BooksToReturn)
                        {
                            Console.WriteLine($"{book.Book_name} [{book.ID}]");
                            returnBooksId.Add(book.ID);
                        }
                        Console.WriteLine("(To get back press any button except the books)");
                        Console.Write("Which of these book you want to return?: ");

                        // Try parse to check if users response is number
                        var user_response_return = Console.ReadLine();
                        if (int.TryParse(user_response_return, out int user_response_return_value) == false)
                        {
                            break;
                        }

                        // create integer from users response
                        int user_response_return_integer = int.Parse(user_response_return);
                        if (returnBooksId.Contains(user_response_return_integer))
                        {
                            DatabaseHelper.ReturnBook(user.ID, user_response_return_integer);
                        }
                        break;
                    case 'i':
                        // Show my profile information
                        Console.Clear();
                        Console.WriteLine("Profile information");
                        Console.WriteLine("-------------------------");
                        Console.WriteLine($"First name: {user.First_name}");
                        Console.WriteLine($"Last name: {user.Last_name}");
                        Console.WriteLine($"E-mail: {user.Email}");
                        Console.WriteLine($"Status: {user.Status}");
                        Console.WriteLine("Borrowed books:");
                        List<Book> borrowedBooks = DatabaseHelper.GetBorrowedBooks(user.ID);
                        
                        foreach (var book in borrowedBooks)
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
                        Console.WriteLine("Delete profile");
                        Console.WriteLine("-------------------------");
                        Console.WriteLine("Do you really want to delete your profile? [y][n]");
                        char user_response_delete = char.ToLower(Console.ReadKey().KeyChar);
                        if (user_response_delete == 'y')
                        {
                            BooksToReturn = DatabaseHelper.GetBorrowedBooks(user.ID);
                            if (BooksToReturn.Count > 0)
                            {
                                Console.WriteLine("");
                                Console.WriteLine("First you have to return the books!");
                                Console.ReadKey();
                                break;
                            } else
                            {
                                // delete profile from database and exit
                                UserManager.Logout();
                                DatabaseHelper.DeleteUser(user.ID);
                                break;
                            }
                        }
                        break;
                }
            } while (user_response != 'e' && UserManager.CurrentUser != null);
        }

        public static void UserStatus1Actions()
        {
            var books = DatabaseHelper.GetAllBooks();
            char user_response;
            var user = UserManager.CurrentUser;

            do
            {
                Console.Clear();
                Console.WriteLine("Vítek's Library");
                Console.WriteLine("------------------------");
                Console.WriteLine("Show available books [b]");
                Console.WriteLine("Add book to Library [a]");
                Console.WriteLine("Delete book from Library [d]");
                Console.WriteLine("Change status of users [s]");
                Console.WriteLine("Show all customers [l]");
                Console.WriteLine("End program [e]");
                Console.Write("Type action: ");
                user_response = char.ToLower(Console.ReadKey().KeyChar);

                switch (user_response)
                {
                    case 'b':
                        // Show available books
                        Console.Clear();
                        Console.WriteLine("Books available books");
                        Console.WriteLine("-------------------------");
                        List<Book> availableBooksShow = DatabaseHelper.GetAllAvailableBooks();
                        foreach (Book book in availableBooksShow)
                        {
                            Console.WriteLine($"{book.Book_name}");
                        }


                        Console.WriteLine("(To get back press any button)");
                        Console.ReadKey();
                        
                        break;
                    case 'a':
                        // add book to database
                        Console.Clear();
                        Console.WriteLine("Vítek's Library");
                        Console.WriteLine("------------------------");
                        Console.Write("Book name: ");
                        var user_response_book_name = Console.ReadLine();

                        while (!IsValidBookName(user_response_book_name))
                        {
                            Console.Clear();
                            Console.WriteLine("Vítek's Library");
                            Console.WriteLine("------------------------");
                            Console.WriteLine("The name lenght must be between 2 - 21 characters and it cannot contain special chars!");
                            Console.Write("Book name: ");
                            user_response_book_name = Console.ReadLine();
                        }

                        Console.Clear();
                        Console.WriteLine("Vítek's Library");
                        Console.WriteLine("------------------------");
                        Console.Write("Author's first name: ");
                        var user_response_book_author_first_name = Console.ReadLine();

                        while (!IsValidName(user_response_book_author_first_name))
                        {
                            Console.Clear();
                            Console.WriteLine("Vítek's Library");
                            Console.WriteLine("------------------------");
                            Console.WriteLine("The name lenght must be between 2 - 21 characters and it cannot contain special chars and whitespaces!");
                            Console.Write("Author's first name: ");
                            user_response_book_author_first_name = Console.ReadLine();
                        }

                        Console.Clear();
                        Console.WriteLine("Vítek's Library");
                        Console.WriteLine("------------------------");
                        Console.Write("Author's last name: ");
                        var user_response_book_author_last_name = Console.ReadLine();

                        while (!IsValidName(user_response_book_author_last_name))
                        {
                            Console.Clear();
                            Console.WriteLine("Vítek's Library");
                            Console.WriteLine("------------------------");
                            Console.WriteLine("The name lenght must be between 2 - 21 characters and it cannot contain special chars and whitespaces!");
                            Console.Write("Author's last name: ");
                            user_response_book_author_last_name = Console.ReadLine();
                        }

                        Console.Clear();
                        Console.WriteLine("Vítek's Library");
                        Console.WriteLine("------------------------");
                        Console.Write("Genre (Horror, sci-fi, fantasy, etc.): ");
                        var user_response_book_genre = Console.ReadLine();

                        while (!IsValidName(user_response_book_genre))
                        {
                            Console.Clear();
                            Console.WriteLine("Vítek's Library");
                            Console.WriteLine("------------------------");
                            Console.WriteLine("Genre lenght must be between 2 - 21 characters and it cannot contain special chars and whitespaces!");
                            Console.Write("Genre (Horror, sci-fi, fantasy, etc.): ");
                            user_response_book_genre = Console.ReadLine();
                        }

                        Console.Clear();
                        Console.WriteLine("Vítek's Library");
                        Console.WriteLine("------------------------");
                        Console.Write("Release year: ");
                        var user_response_book_release_year = Console.ReadLine();

                        while (int.TryParse(user_response_book_release_year, out int user_response_book_release_year_value) == false)
                        {
                            Console.Clear();
                            Console.WriteLine("Vítek's Library");
                            Console.WriteLine("------------------------");
                            Console.WriteLine($"Please enter year as a number! Do not use year greater than {DateTime.Now.Year}!");
                            Console.Write("Release year: ");
                            user_response_book_release_year = Console.ReadLine();
                        }

                        int user_response_book_release_year_integer = int.Parse(user_response_book_release_year);
                        if (user_response_book_release_year_integer > DateTime.Now.Year)
                        {
                            Console.Clear();
                            Console.WriteLine("Vítek's Library");
                            Console.WriteLine("------------------------");
                            Console.WriteLine("Do you think its funny? Nah");
                            Console.ReadKey();
                            break;
                        }
                        var newBook = new Book(user_response_book_author_last_name, user_response_book_author_first_name, user_response_book_name, user_response_book_genre, user_response_book_release_year_integer);
                        using (var connection = DatabaseHelper.GetConnection())
                        {
                            newBook.AddBookToDatabase(connection);
                        }

                        break;
                    case 'd':
                        // delete book from database (only available books can be deleted)
                        Console.Clear();
                        Console.WriteLine("Choose book to be deleted");
                        Console.WriteLine("-------------------------");
                        List<int> availableBooksId = new List<int>();
                        List<Book> availableBooks = DatabaseHelper.GetAllAvailableBooks();
                        foreach (Book book in availableBooks)
                        {
                            Console.WriteLine($"{book.Book_name} [{book.ID}]");
                            availableBooksId.Add(book.ID);
                        }


                        Console.WriteLine("(To get back press any button except the books)");
                        Console.Write("Which of these book?: ");

                        var user_response_delete = Console.ReadLine();
                        if (int.TryParse(user_response_delete, out int user_response_delete_value) == false)
                        {
                            break;
                        }

                        int user_response_delete_integer = int.Parse(user_response_delete);
                        if (availableBooksId.Contains(user_response_delete_integer))
                        {
                            DatabaseHelper.DeleteBook(user_response_delete_integer);
                        }

                        break;
                    case 's':
                        // change status of customers
                        Console.Clear();
                        Console.WriteLine("Change status of user");
                        Console.WriteLine("-------------------------");
                        List<Person> peopleStatus0 = DatabaseHelper.GetAllCustomers();
                        List<int> poepleStatus0IDs = new List<int>();
                        foreach (var person in peopleStatus0)
                        {
                            Console.WriteLine($"User: {person.First_name} {person.Last_name} ({person.Email}) [{person.ID}]");
                            poepleStatus0IDs.Add(person.ID);
                        }
                        Console.WriteLine("Select user to update status by his id");
                        var user_response_user_to_update = Console.ReadLine();
                        if (int.TryParse(user_response_user_to_update, out int user_response_user_to_update_value) == false)
                        {
                            break;
                        }

                        int user_response_user_to_update_integer = int.Parse(user_response_user_to_update);
                        if (!poepleStatus0IDs.Contains(user_response_user_to_update_integer))
                        {
                            Console.WriteLine($"Select only existing user. Press any key to continue...");
                            Console.ReadKey();
                            break; //user_response_user_to_update_integer will be used
                        }
                        Console.WriteLine("");
                        Console.WriteLine("Choose status (0 - customer, 1 - librarian)");
                        var user_response_user_to_update_status = Console.ReadLine();
                        if (int.TryParse(user_response_user_to_update_status, out int user_response_user_to_update_status_value) == false)
                        {
                            break;
                        }
                        int user_response_user_to_update_status_integer = int.Parse(user_response_user_to_update_status);
                        if (user_response_user_to_update_status_integer != 0 || user_response_user_to_update_status_integer != 1)
                        {
                            DatabaseHelper.UpdateUserStatus(user_response_user_to_update_integer, user_response_user_to_update_status_integer);
                            Console.WriteLine($"Status was updated to {user_response_user_to_update_status_integer}. Press any key to continue...");
                            Console.ReadKey();
                            break;
                        }

                        break;
                    case 'l':
                        // show all customers
                        Console.Clear();
                        Console.WriteLine("List of customers");
                        Console.WriteLine("-------------------------");
                        List<Person> people = DatabaseHelper.GetAllCustomers();
                        foreach (var person in people)
                        {
                            Console.WriteLine($"Customer: {person.First_name} {person.Last_name} ({person.Email})");
                        }
                        Console.WriteLine("");
                        Console.WriteLine("(To get back press any button)");
                        Console.ReadKey();
                        break;
                }
            } while (user_response != 'e' && UserManager.CurrentUser != null);
        }

        public static void Auth()
        {
            char user_response;
            do
            {
                Console.Clear();
                Console.WriteLine("Vítek's Library");
                Console.WriteLine("------------------------");
                Console.WriteLine("Login [l]");
                Console.WriteLine("Register new account [r]");
                Console.WriteLine("End program [e]");
                Console.Write("Type action: ");
                user_response = char.ToLower(Console.ReadKey().KeyChar);

                switch(user_response)
                {
                    case 'l':
                        Console.Clear();
                        Console.WriteLine("Vítek's Library");
                        Console.WriteLine("------------------------");
                        Console.Write("E-mail: ");
                        var user_response_login_email = Console.ReadLine();

                        // check if user exists in database

                        Console.Clear();
                        Console.WriteLine("Vítek's Library");
                        Console.WriteLine("------------------------");
                        Console.Write("Password: ");
                        string user_response_login_password = ReadLineWithMask();

                        UserManager.Login(user_response_login_email, user_response_login_password);

                        break;
                    case 'r':
                        // registration
                        Console.Clear();
                        Console.WriteLine("Vítek's Library");
                        Console.WriteLine("------------------------");
                        Console.Write("First name: ");
                        var user_response_register_first_name = Console.ReadLine();

                        while(!IsValidName(user_response_register_first_name))
                        {
                            Console.Clear();
                            Console.WriteLine("Vítek's Library");
                            Console.WriteLine("------------------------");
                            Console.WriteLine("The name lenght must be between 2 - 21 characters and cant contain special chars and whitespaces!");
                            Console.Write("First name: ");
                            user_response_register_first_name = Console.ReadLine();
                        } 

                        Console.Clear();
                        Console.WriteLine("Vítek's Library");
                        Console.WriteLine("------------------------");
                        Console.Write("Last name: ");
                        var user_response_register_last_name = Console.ReadLine();

                        while (!IsValidName(user_response_register_last_name))
                        {
                            Console.Clear();
                            Console.WriteLine("Vítek's Library");
                            Console.WriteLine("------------------------");
                            Console.WriteLine("The name lenght must be between 2 - 21 characters and cant contain special chars and whitespaces!");
                            Console.Write("Last name: ");
                            user_response_register_last_name = Console.ReadLine();
                        }


                        Console.Clear();
                        Console.WriteLine("Vítek's Library");
                        Console.WriteLine("------------------------");
                        Console.Write("E-mail: ");
                        var user_response_register_email = Console.ReadLine();

                        while (!isValidEmail(user_response_register_email))
                        {
                            Console.Clear();
                            Console.WriteLine("Vítek's Library");
                            Console.WriteLine("------------------------");
                            if (!DatabaseHelper.isEmailUnique(user_response_register_email))
                            {
                                Console.WriteLine("E-mail is already in use!");
                            } else
                            {
                                Console.WriteLine("Wrong E-mail format!");
                            }
                            Console.Write("E-mail: ");
                            user_response_register_email = Console.ReadLine();
                        } 

                        string user_response_register_password;
                        string user_response_register_password_again;

                        Console.Clear();
                        Console.WriteLine("Vítek's Library");
                        Console.WriteLine("------------------------");
                        Console.Write("Password: ");
                        user_response_register_password = ReadLineWithMask();

                        while (string.IsNullOrEmpty(user_response_register_password) || user_response_register_password.Length < 6)
                        {
                            Console.Clear();
                            Console.WriteLine("Vítek's Library");
                            Console.WriteLine("------------------------");
                            Console.WriteLine("Password has to use 6 or more characters!");
                            Console.Write("Password: ");
                            user_response_register_password = ReadLineWithMask();
                        }

                        Console.Clear();
                        Console.WriteLine("Vítek's Library");
                        Console.WriteLine("------------------------");
                        Console.Write("Again password: ");
                        user_response_register_password_again = ReadLineWithMask();

                        while (user_response_register_password != user_response_register_password_again)
                        {
                            Console.Clear();
                            Console.WriteLine("Vítek's Library");
                            Console.WriteLine("------------------------");
                            Console.WriteLine("Passwords did not match. Try again!");
                            Console.Write("Password: ");
                            user_response_register_password = ReadLineWithMask();

                            while (string.IsNullOrEmpty(user_response_register_password) || user_response_register_password.Length < 6)
                            {
                                Console.Clear();
                                Console.WriteLine("Vítek's Library");
                                Console.WriteLine("------------------------");
                                Console.WriteLine("Password has to use 6 or more characters!");
                                Console.Write("Password: ");
                                user_response_register_password = ReadLineWithMask();
                            }

                            Console.Clear();
                            Console.WriteLine("Vítek's Library");
                            Console.WriteLine("------------------------");
                            Console.Write("Again password: ");
                            user_response_register_password_again = ReadLineWithMask();
                        }

                        Person loggedUser = new Person(user_response_register_first_name, user_response_register_last_name, 0, user_response_register_email);
                        UserManager.Register(loggedUser, user_response_register_password);
                        break;
                }

            } while (user_response != 'e' && UserManager.CurrentUser == null);
        }

        public static string ReadLineWithMask()
        {
            string pass = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);

            return pass;
        }

        public static bool isValidEmail(string email)
        {
            // check email format
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Regular expression for email
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            if(!regex.IsMatch(email))
            {
                return false;
            }

            // check if email is unique
            if (!DatabaseHelper.isEmailUnique(email))
            {
                return false;
            }

            return true;
        }

        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Regular expression to check string length and ensure it contains only letters
            string pattern = @"^[a-zA-Z]{2,21}$";
            Regex regex = new Regex(pattern);

            return regex.IsMatch(name);
        }

        public static bool IsValidBookName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            // Regular expression to check string length and ensure it contains only letters and whitespace
            // The pattern allows spaces but not at the beginning or end of the string
            string pattern = @"^[a-zA-Z]+(?:[ ][a-zA-Z]+)*$";

            Regex regex = new Regex(pattern);

            // Check if the string matches the pattern and its length is within the specified range
            return regex.IsMatch(name) && name.Length >= 2 && name.Length <= 21;
        }
    }
}
