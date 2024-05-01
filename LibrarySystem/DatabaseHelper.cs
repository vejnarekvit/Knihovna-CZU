using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace LibrarySystem
{
    public static class DatabaseHelper
    {
        /*
        tato třída je taková univerzální třída, která pracuje s databází
        */

        // connection na DB (naleznete přímo v projektu ve složce data)
        private static string connection_string = @"Data Source=..\..\..\data\library.db";

        // metoda na tvorbu souboru typu .db, pokud ještě neexistuje a rovnou i vytvoří všechny tabulky a přidá example data
        public static void InitializeDatabase()
        {
            if (!File.Exists(@"..\..\..\data\library.db"))
            {
                SQLiteConnection.CreateFile(@"..\..\..\data\library.db");

                using (var connection = new SQLiteConnection(connection_string))
                {
                    connection.Open();
                    /* CREATE DB */
                    CreateBooksTable(connection);
                    CreateUsersTable(connection);
                    CreateUserBooksTable(connection);

                    /* ADD EXAMPLE RECORDS (BOOKS AND USERS) */
                    var books = new List<Book>();
                    books.Add(new Book("Petr", "Bezruč", "Slezské písně", "Balada", 2000));
                    books.Add(new Book("Karel", "Čapek", "Válka s mloky", "Sci-fi", 2006));
                    books.Add(new Book("Karel", "Čapek", "Krakatit", "Sci-fi", 2007));
                    books.Add(new Book("Viktor", "Dyk", "Krysař", "novela", 1989));
                    foreach (var book in books)
                    {
                        book.AddBookToDatabase(connection);
                    }
                    var p1 = new Person("Vít", "Vejnárek", 0, "vejnarekvitek@gmail.com");
                    var p2 = new Person("Petra", "Jarošová", 1, "petra.jarosova@gmail.com");
                    p1.AddPersonToDatabase(connection, HashPassword("123456"));
                    p2.AddPersonToDatabase(connection, HashPassword("123456"));


                    connection.Close();
                }
            }
        }

        // SQL pro vytvoření tabulky knih
        static void CreateBooksTable(SQLiteConnection connection)
        {
            string create_table_book = @"CREATE TABLE IF NOT EXISTS books (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                authors_first_name TEXT NOT NULL,
                authors_last_name TEXT NOT NULL,
                book_name TEXT NOT NULL,
                genre TEXT NOT NULL,
                book_release INT NOT NULL
            );";

            ExecuteSql(connection, create_table_book);
        }

        // SQL pro vytvoření tabulky uživatelů
        static void CreateUsersTable(SQLiteConnection connection)
        {
            string create_table_users = @"CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                first_name TEXT NOT NULL,
                last_name TEXT NOT NULL,
                status INT NOT NULL,
                password TEXT NOT NULL CHECK (length(password) >= 6),
                email TEXT NOT NULL UNIQUE
            );";

            ExecuteSql(connection, create_table_users);
        }

        // SQL pro vytvoření tabulky knihy uživatelů (tabulka slouží k propojení uživatele a knihy - půjčení)
        static void CreateUserBooksTable(SQLiteConnection connection)
        {
            string create_table_user_books = @"CREATE TABLE IF NOT EXISTS user_books (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                book_id INTEGER NOT NULL,
                FOREIGN KEY (user_id) REFERENCES users(id),
                FOREIGN KEY (book_id) REFERENCES books(id)
            );";

            ExecuteSql(connection, create_table_user_books);
        }

        // tato metoda executuje (spouští) sql commandy
        static void ExecuteSql(SQLiteConnection connection, string sql)
        {
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }


        // metoda, díky které dostanu connection i mimo tuto třídu, rovnou otevře connection
        public static SQLiteConnection GetConnection()
        {
            var connection = new SQLiteConnection(connection_string);
            connection.Open();
            return connection;
        }


        /* ZDE ZAČÍNAJÍ METODY, KTERÉ UŽ DĚLAJÍ ÚKONY NA ZÁKLADĚ UŽIVATELE, KTERÝ SI JE VYŽÁDAL */
        // Metoda získá všechny knihy (klidně i půjčené)
        public static List<Book> GetAllBooks()
        {
            var books = new List<Book>();
            string sql = "SELECT authors_first_name, authors_last_name, book_name, genre, book_release, id FROM books";

            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var book = new Book(
                                reader["authors_first_name"]?.ToString() ?? string.Empty,
                                reader["authors_last_name"]?.ToString() ?? string.Empty,
                                reader["book_name"]?.ToString() ?? string.Empty,
                                reader["genre"]?.ToString() ?? string.Empty,
                                Convert.ToInt32(reader["book_release"])
                            );
                            if (reader["id"] != DBNull.Value)
                            {
                                book.ID = Convert.ToInt32(reader["id"].ToString());
                            }
                            books.Add(book);
                        }
                    }
                }
            }
            return books;
        }


        // získá všechny knihy, které ještě nejsou půjčené
        public static List<Book> GetAllAvailableBooks()
        {
            var books = new List<Book>();
            string sql = @"
    SELECT b.authors_first_name, b.authors_last_name, b.book_name, b.genre, b.book_release, b.id
    FROM books b
    LEFT JOIN user_books ub ON b.id = ub.book_id
    WHERE ub.book_id IS NULL;";

            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var book = new Book(
                                reader["authors_first_name"]?.ToString() ?? string.Empty,
                                reader["authors_last_name"]?.ToString() ?? string.Empty,
                                reader["book_name"]?.ToString() ?? string.Empty,
                                reader["genre"]?.ToString() ?? string.Empty,
                                Convert.ToInt32(reader["book_release"])
                            );
                            if (reader["id"] != DBNull.Value)
                            {
                                book.ID = Convert.ToInt32(reader["id"].ToString());
                            }
                            books.Add(book);
                        }
                    }
                }
            }
            return books;
        }


        // získá všechny uživatele (návratová hodnota List person)
        public static List<Person> GetAllUsers()
        {
            var people = new List<Person>();
            string sql = "SELECT first_name, last_name, status, email FROM users";

            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var person = new Person(
                                reader["first_name"]?.ToString() ?? string.Empty,
                                reader["last_name"]?.ToString() ?? string.Empty,
                                reader["status"] != DBNull.Value ? Convert.ToInt32(reader["status"]) : 0,
                                reader["email"]?.ToString() ?? string.Empty
                            );
                            people.Add(person);
                        }
                    }
                }
            }
            return people;
        }

        // získá pouze customers, tedy uživatele se statusem 0
        public static List<Person> GetAllCustomers()
        {
            var people = new List<Person>();
            string sql = "SELECT id, first_name, last_name, email, status FROM users WHERE status = 0";

            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var person = new Person(
                                reader["first_name"]?.ToString() ?? string.Empty,
                                reader["last_name"]?.ToString() ?? string.Empty,
                                reader["status"] != DBNull.Value ? Convert.ToInt32(reader["status"]) : 0,
                                reader["email"]?.ToString() ?? string.Empty
                            );
                            if (reader["id"] != DBNull.Value)
                            {
                                person.ID = Convert.ToInt32(reader["id"].ToString());
                            }
                            people.Add(person);
                        }
                    }
                }
            }
            return people;
        }

        // hash hesla, potom se uloží do DB
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        // metzoda pro kontrolu, zdali je email v databázi (moc se mi líbí, že se dá použít reader.HasRows)
        public static bool isEmailUnique(string email)
        {
            bool unique = false;
            string sql = "SELECT email FROM users WHERE email = @Email";
            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    using (var reader = command.ExecuteReader())
                    {
                        if(!reader.HasRows)
                        {
                            unique = true;
                        }
                    }
                }
                connection.Close();
            }
            return unique;
        }

        // metoda na půjčení knihy, vytvoří se záznam do user_books, kde se přídá ID usera a ID knihy
        public static void BorrowBook(int userId, int bookId)
        {
            string sql = @"INSERT INTO user_books (user_id, book_id) VALUES (@UserId, @BookId);";
            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@BookId", bookId);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            Console.WriteLine("Book borrowed successfully.");
        }

        // Metoda na vracení knihy do knihovny, smaže záznam o půjčení z user_books
        public static void ReturnBook(int userId, int bookId)
        {
            string sql = @"DELETE FROM user_books WHERE user_id = @UserId AND book_id = @BookId;";
            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@BookId", bookId);
                    int affectedRows = command.ExecuteNonQuery();
                    // radši kontrola, jestli se opravdu něco smazalo
                    if (affectedRows > 0)
                    {
                        Console.WriteLine("Book returned successfully.");
                    }
                    else
                    {
                        Console.WriteLine("No book was returned. Check if the book ID and user ID are correct.");
                    }
                }
                connection.Close();
            }
        }


        // Metoda na smazání knihy (jde o odebrání z knihovny)
        public static void DeleteBook(int bookId)
        {
            string sql = @"DELETE FROM books WHERE id = @BookId;";
            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@BookId", bookId);
                    // radši kontrola, jestli se opravdu něco smazalo
                    int affectedRows = command.ExecuteNonQuery();
                    if (affectedRows > 0)
                    {
                        Console.WriteLine("Book deleted successfully.");
                    }
                    else
                    {
                        Console.WriteLine("No book was deleted. Check if the book ID is correct.");
                    }
                }
                connection.Close();
            }
        }


        // Metoda na získání půjčených knih dle ID uživatele (vrací rovnou list knih)
        public static List<Book> GetBorrowedBooks(int userId)
        {
            List<Book> borrowedBooks = new List<Book>();
            string sql = @"
    SELECT b.id, b.authors_first_name, b.authors_last_name, b.book_name, b.genre, b.book_release
    FROM books b
    JOIN user_books ub ON b.id = ub.book_id
    WHERE ub.user_id = @UserId;";
            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    if (connection.State != System.Data.ConnectionState.Open)
                        connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var book = new Book(
                                reader["authors_first_name"].ToString(),
                                reader["authors_last_name"].ToString(),
                                reader["book_name"].ToString(),
                                reader["genre"].ToString(),
                                Convert.ToInt32(reader["book_release"])
                            );

                            book.ID = Convert.ToInt32(reader["id"]);
                            borrowedBooks.Add(book);
                        }
                    }
                }
            }

            return borrowedBooks;
        }

        // Smazání uživatele z databáze podle ID uživatele
        public static void DeleteUser(int userId)
        {
            string sql = @"DELETE FROM users WHERE id = @UserId;";
            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            Console.WriteLine("Profile deleted successfuly.");
        }


        // Update statusu uživatele, toto může provést pouze knihovník. Tímto způsobem je možnost udělat ze zákazníka knihovníka
        public static void UpdateUserStatus(int userId, int status)
        {
            string sql = "UPDATE users SET status = @Status WHERE id = @UserId;";

            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Status", status);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            Console.WriteLine("User status updated successfully.");
        }
    }
}
