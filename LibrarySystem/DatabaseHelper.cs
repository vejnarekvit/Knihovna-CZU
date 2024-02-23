using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Runtime.CompilerServices;

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
                    CreateBooksTable(connection);
                    CreateUsersTable(connection);
                    CreateUserBooksTable(connection);
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
                book_release DATE NOT NULL,
                book_count INT NOT NULL CHECK (book_count >= 0)
            );";

            ExecuteSql(connection, create_table_book);
        }

        static void CreateUsersTable(SQLiteConnection connection)
        {
            string create_table_users = @"CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                first_name TEXT NOT NULL,
                last_name TEXT NOT NULL,
                author_birthdate DATE,
                status INT NOT NULL
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

        static void ExecuteSql(SQLiteConnection connection, string sql)
        {
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
