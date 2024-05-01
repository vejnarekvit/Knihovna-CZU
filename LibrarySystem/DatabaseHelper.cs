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
        private static string connection_string = @"Data Source=..\..\..\data\library.db";
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

        public static SQLiteConnection GetConnection()
        {
            var connection = new SQLiteConnection(connection_string);
            connection.Open();  // Optionally open the connection before returning, depending on how you manage connections
            return connection;
        }

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
                            // Set the ID property of the book
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

        public static List<Book> GetAllAvailableBooks()
        {
            var books = new List<Book>();
            // Adjusted SQL to only fetch books that are not currently borrowed
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
                            // Set the ID property of the book
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
                            // Set the ID property of the book
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

        static void ExecuteSql(SQLiteConnection connection, string sql)
        {
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            }
        }

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

        public static void BorrowBook(int userId, int bookId)
        {
            // SQL to insert a record into user_books
            string sql = @"INSERT INTO user_books (user_id, book_id) VALUES (@UserId, @BookId);";
            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    // Add parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@BookId", bookId);

                    // Execute the command
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            Console.WriteLine("Book borrowed successfully.");
        }

        public static void ReturnBook(int userId, int bookId)
        {
            // SQL to delete a record from user_books
            string sql = @"DELETE FROM user_books WHERE user_id = @UserId AND book_id = @BookId;";
            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    // Add parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@BookId", bookId);

                    // Execute the command
                    int affectedRows = command.ExecuteNonQuery();
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

        public static void DeleteBook(int bookId)
        {
            // SQL to delete a record from user_books
            string sql = @"DELETE FROM books WHERE id = @BookId;";
            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    // Add parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@BookId", bookId);

                    // Execute the command
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

                    // Open the connection if it's not already open
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

                            // Assuming there's a property ID in the Book class with a setter
                            book.ID = Convert.ToInt32(reader["id"]);
                            borrowedBooks.Add(book);
                        }
                    }
                }
            }

            return borrowedBooks;
        }

        public static void DeleteUser(int userId)
        {
            // SQL to insert a record into user_books
            string sql = @"DELETE FROM users WHERE id = @UserId;";
            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    // Add parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@UserId", userId);

                    // Execute the command
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            Console.WriteLine("Profile deleted successfuly.");
        }

        public static void UpdateUserStatus(int userId, int status)
        {
            string sql = "UPDATE users SET status = @Status WHERE id = @UserId;";

            using (var connection = new SQLiteConnection(connection_string))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    // Add parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Status", status);

                    // Execute the command
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            Console.WriteLine("User status updated successfully.");
        }
    }
}
