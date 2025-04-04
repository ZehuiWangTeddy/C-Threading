using TaskManager.Repositories;
using TaskManager.Views;
using System.IO;

namespace TaskManager
{
    public partial class App : Application
    {
        private const string DatabaseFileName = "TaskManager.db";
        public static string DatabasePath { get; private set; }
        public static TaskRepository TaskRepo { get; private set; }

        public App()
        {
            InitializeComponent();
            InitializeDatabase();

            // Set main page
            MainPage = new NavigationPage(new DashboardPage());
        }

        private void InitializeDatabase()
        {
            try
            {
                // Use cross-platform-safe location
                DatabasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);

                Console.WriteLine($"📁 Database path: {DatabasePath}");

                // Create the repo (this also creates tables)
                TaskRepo = new TaskRepository(DatabasePath);

                // Force a write to make sure file is physically created
                if (!File.Exists(DatabasePath))
                {
                    File.WriteAllText(DatabasePath, ""); // trigger write
                }

                if (File.Exists(DatabasePath))
                    Console.WriteLine("✅ Database file created successfully.");
                else
                    Console.WriteLine("❌ Database file was not created.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error initializing database: {ex}");
            }
        }
    }
}