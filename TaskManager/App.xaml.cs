using System;
using TaskManager.Repositories;
using TaskManager.Models;
using TaskManager.Views;
using TaskManager.Services;

namespace TaskManager
{
    public partial class App : Application, IDisposable
    {
        private const string DatabaseFileName = "TaskManager.db";
        private ThreadPoolManager _threadPoolManager;
        private ITaskRepository _taskRepository;
        private TaskService _taskService;
        private bool _disposed;

        public App()
        {
            InitializeComponent();
            InitializeDatabase();
            

            MainPage = new NavigationPage(new AppShell());
            MainPage = new NavigationPage(new DashboardPage());
        }

        private void InitializeDatabase()
        {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DatabaseFileName);
            
            // Initialize TaskRepository first (no dependencies)
            _taskRepository = new TaskRepository(databasePath);
            
            // Initialize ThreadPoolManager with repository
            _threadPoolManager = new ThreadPoolManager(_taskRepository);
            
            // Initialize TaskService and TaskScheduler (both need repository and thread pool)
            _taskService = new TaskService(_threadPoolManager, _taskRepository);
            
            Console.WriteLine($"Database and tables created at {databasePath}");
            Console.WriteLine("TaskScheduler initialized and running");
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            // Clean up resources when app goes to background
            _threadPoolManager?.Dispose();
        }

        protected override void OnResume()
        {
            base.OnResume();
            // Reinitialize components when app resumes
          
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _taskService?.Dispose();
                    _threadPoolManager?.Dispose();
                }

                // Clean up unmanaged resources and override finalizer
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
