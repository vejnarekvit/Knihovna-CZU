using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{
    public class Person
    {
        public string First_name { get; }
        public string Last_name { get; }
        public int Status { get; }
        public string Email { get; }
        public int ID { get; set; }

        public Person(string first_name, string last_name, int status, string email)
        {
            First_name = first_name;
            Last_name = last_name;
            Status = status;
            Email = email;
        }

        public void AddPersonToDatabase(SQLiteConnection connection, string password)
        {
            string sql = @"
    INSERT INTO users (first_name, last_name, status, password, email)
    VALUES (@FirstName, @LastName, @Status, @Password, @Email);";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@FirstName", this.First_name);
                command.Parameters.AddWithValue("@LastName", this.Last_name);
                command.Parameters.AddWithValue("@Status", this.Status);
                command.Parameters.AddWithValue("@Password", password);
                command.Parameters.AddWithValue("@Email", this.Email);
                command.ExecuteNonQuery();
            }
        }
    }
}
