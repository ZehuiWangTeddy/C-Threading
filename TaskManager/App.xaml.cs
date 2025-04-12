using TaskManager.Models;
using TaskManager.Repositories;
using TaskManager.Services;
using TaskManager.Views;

namespace TaskManager;

public partial class App : Application, IDisposable
{
    private const string DatabaseFileName = "TaskManager.db";
    private bool _disposed;
    private TaskPollingService _taskPollingService;
    private ITaskRepository _taskRepository;
    private TaskService _taskService;
    private TaskUpdateService _taskUpdateService;
    private ThreadPoolManager _threadPoolManager;

    public App()
    {
        InitializeComponent();
        InitializeDatabase();


        MainPage = new NavigationPage(new AppShell());
        MainPage = new NavigationPage(new DashboardPage());
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        var databasePath =
            @"C:\Users\james\AppData\Local\Packages\com.companyname.taskmanager_9zz4h110yvjzm\LocalCache\Local\TaskManager.db";

        var directoryPath = Path.GetDirectoryName(databasePath);
        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

        Console.WriteLine($"Database file path: {databasePath}");

        var taskRepository = new TaskRepository(databasePath);
        Console.WriteLine($"Database and tables should now be created at {databasePath}");
    }


    private void InitializeDatabase()
    {
        var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            DatabaseFileName);

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
}