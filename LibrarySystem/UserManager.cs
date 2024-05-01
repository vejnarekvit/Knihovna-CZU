using LibrarySystem;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{
    public static class UserManager
    {
        // Tato třída zajištujě vytvoření uživatele, ke kterému budu mít přístup pořád, zde se uloží do CurrentUser přihlášený uživatel
        public static Person? CurrentUser { get; private set; }


        // Metoda na login
        public static void Login(string email, string password)
        {
            string sql = "SELECT id, first_name, last_name, status, password FROM users WHERE email = @Email";

            using (var connection = DatabaseHelper.GetConnection())
            {
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // vytvořím dve promenne s hesly k porovnaní
                            string storedPasswordHash = reader["password"].ToString();
                            string inputPasswordHash = DatabaseHelper.HashPassword(password);

                            if (inputPasswordHash == storedPasswordHash)
                            {
                                string firstName = reader["first_name"].ToString();
                                string lastName = reader["last_name"].ToString();
                                int status = Convert.ToInt32(reader["status"]);
                                int ID = Convert.ToInt32(reader["id"].ToString());

                                var loggedUser = new Person(firstName, lastName, status, email);
                                loggedUser.ID = ID;
                                CurrentUser = loggedUser;
                                Console.Clear();
                            }
                            else
                            {
                                Console.Clear();
                                Console.WriteLine("Login failed. Incorrect password.");
                                Console.ReadKey();
                            }
                        }
                        else
                        {
                            Console.Clear();
                            Console.WriteLine("Login failed. Email not found.");
                            Console.ReadKey();
                        }
                    }
                }
                connection.Close();
            }
        }

        // odhlášení uživatele
        public static void Logout()
        {
            CurrentUser = null;
            Console.WriteLine("");
            Console.WriteLine("User logged out.");
        }

        // registrace uživatele s heslem
        public static void Register(Person user, string password)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                user.AddPersonToDatabase(connection, DatabaseHelper.HashPassword(password));
                Console.WriteLine("");
                Console.WriteLine("Now you can log in! Profile has been created! Congrats!");
                Console.WriteLine("Press any button to continue...");
                Console.ReadKey();
            }
        }
    }
}
