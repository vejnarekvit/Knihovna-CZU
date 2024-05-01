using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
        public int Book_release_year { get; }
        public int ID { get; set; }

        public Book(string authors_first_name, string authors_last_name, string book_name, string genre, int book_release_year)
        {
            Authors_first_name = authors_first_name;
            Authors_last_name = authors_last_name;
            Book_name = book_name;
            Genre = genre;
            Book_release_year = book_release_year;
        }
        public void AddBookToDatabase(SQLiteConnection connection)
        {
            // SQL command to insert a new book into the books table
            string sql = @"INSERT INTO books (authors_first_name, authors_last_name, book_name, genre, book_release) VALUES (@AuthorsFirstName, @AuthorsLastName, @BookName, @Genre, @BookRelease);";

            // Creating the command and adding parameters to avoid SQL injection
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@AuthorsFirstName", this.Authors_first_name);
                command.Parameters.AddWithValue("@AuthorsLastName", this.Authors_last_name);
                command.Parameters.AddWithValue("@BookName", this.Book_name);
                command.Parameters.AddWithValue("@Genre", this.Genre);
                command.Parameters.AddWithValue("@BookRelease", this.Book_release_year);

                // Execute the insert command
                command.ExecuteNonQuery();
            }
        }
    }
}
