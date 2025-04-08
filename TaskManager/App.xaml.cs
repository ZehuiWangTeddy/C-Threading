using System;
using TaskManager.Repositories;
using TaskManager.Models;
using TaskManager.Views;
using TaskManager.Services;
using TaskManager.ViewModels;
using Microsoft.Maui.Controls;

namespace TaskManager
{
    public partial class App : Application, IDisposable
    {
        private const string DatabaseFileName = "TaskManager.db";
        private ThreadPoolManager _threadPoolManager;
        private TaskService _taskService;
        private ITaskRepository _taskRepository;
        private TaskUpdateService _taskUpdateService;
        private TaskPollingService _taskPollingService;
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
            
            _taskRepository = new TaskRepository(databasePath);
            _taskUpdateService = new TaskUpdateService(_taskRepository);
            _taskPollingService = new TaskPollingService(_taskRepository, _taskUpdateService);
            _threadPoolManager = new ThreadPoolManager(_taskRepository);
            _taskService = new TaskService(_threadPoolManager, _taskRepository);
            Console.WriteLine($"Database and tables created at {databasePath}");
            
            _threadPoolManager.StartProcessingTasks();
            Console.WriteLine("ThreadPoolManager initialized and running");
            
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            // Remove or comment out this line if it's causing problems
            // _threadPoolManager?.Dispose();
    
            // Instead, use this if you want to temporarily pause
            Console.WriteLine("App going to sleep - NOT disposing ThreadPoolManager");
        }
        protected override void OnResume()
        {
            base.OnResume();
            // Reinitialize components when app resumes
            _threadPoolManager?.ResumeProcessing();
          
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _taskPollingService?.Dispose();
                    _threadPoolManager?.Dispose();
                    _taskUpdateService?.Dispose();
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
