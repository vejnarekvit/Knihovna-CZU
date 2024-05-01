public static class UserManager
{
    public static Person CurrentUser { get; private set; }

    public static void Login(string username, string password)
    {
        // Simulate login logic
        if (username == "admin" && password == "admin")
        {
            CurrentUser = new Person("John", "Doe", new DateTime(1985, 5, 22), 1, "admin"); // Replace with actual data retrieval logic
            Console.WriteLine("Login successful.");
        }
        else
        {
            Console.WriteLine("Login failed.");
            CurrentUser = null;
        }
    }

    public static void Logout()
    {
        CurrentUser = null;
        Console.WriteLine("User logged out.");
    }
}
