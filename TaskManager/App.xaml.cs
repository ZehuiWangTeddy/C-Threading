
using TaskManager.Repositories;
using TaskManager.Models.DBModels;
using SQLite;
using System.IO;
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
            

            MainPage = new AppShell();
            MainPage = new TaskListPage();
        }

        private void InitializeDatabase()
        {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DatabaseFileName);
            var taskRepository = new TaskRepository(databasePath);
            Console.WriteLine($"Database and tables created at {databasePath}");
        }
    }
}
