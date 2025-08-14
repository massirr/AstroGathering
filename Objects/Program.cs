using Avalonia;
using System;
using AstroGathering.Database;
using System.Collections.Generic;

namespace AstroGathering.Objects;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Test database connection and methods
        TestDatabase();
        
        // Start the GUI application
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
    
    private static void TestDatabase()
    {
        Console.WriteLine("=== TESTING DATABASE CONNECTION ===\n");
        
        try
        {
            DatabaseOut db = new DatabaseOut();
            
            // Test GetAllUsers
            Console.WriteLine("Testing GetAllUsers()...");
            List<User> users = db.GetAllUsers();
            
            if (users.Count > 0)
            {
                Console.WriteLine($"✅ Success! Found {users.Count} users:");
                foreach (User user in users)
                {
                    Console.WriteLine($"  - ID: {user.UserId}, Email: {user.Email}, Name: {user.FirstName} {user.LastName}");
                }
            }
            else
            {
                Console.WriteLine("⚠️  No users found in database (this might be normal if database is empty)");
            }
            
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("Database test completed. Starting GUI...\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database test failed: {ex.Message}");
            Console.WriteLine("\nPress any key to continue to GUI anyway...");
            Console.ReadKey();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
