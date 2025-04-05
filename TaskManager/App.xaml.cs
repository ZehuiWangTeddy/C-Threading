using System.Diagnostics;
using TaskManager.Repositories;
using TaskManager.Models.DBModels;
using SQLite;
using System.IO;
using TaskManager.Models;
using TaskManager.Services;
using TaskManager.Views;

namespace TaskManager
{
    public partial class App : Application
    {
        private const string DatabaseFileName = "TaskManager.db";

        public App()
        {
                InitializeComponent();
                InitializeDatabase();

                MainPage = new NavigationPage(new DashboardPage());
        }


        private void InitializeDatabase()
        {
            var databasePath = @"C:\Users\james\AppData\Local\Packages\com.companyname.taskmanager_9zz4h110yvjzm\LocalCache\Local\TaskManager.db";
            
            string directoryPath = Path.GetDirectoryName(databasePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);  
            }
            
            Console.WriteLine($"Database file path: {databasePath}");

            var taskRepository = new TaskRepository(databasePath);
            Console.WriteLine($"Database and tables should now be created at {databasePath}");
        }
    }
}