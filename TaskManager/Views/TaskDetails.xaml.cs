using TaskManager.Repositories;
using TaskManager.Views;
using TaskStatus = TaskManager.Views.TaskStatus;

namespace TaskManager;

public partial class TaskDetails : ContentPage
{
    private readonly int _taskId;
    private readonly ITaskRepository _taskRepository;

    public TaskDetails(ITaskRepository taskRepository, int taskId)
    {
        InitializeComponent();
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _taskId = taskId;
        LoadTaskDetails();
    }

    private void LoadTaskDetails()
    {
        var task = _taskRepository.GetTaskById(_taskId);
        if (task == null)
        {
            DisplayAlert("Error", "Task details not found.", "OK");
            return;
        }

        var logMessages = task.Logger?.GetLogMessages();

        BindingContext = new TaskLog
        {
            TaskName = task.Name,
            ThreadId = task.ThreadId ?? 0,
            ExecutionTime = task.ExecutionTime?.ToString() ?? "N/A",
            Priority = Enum.TryParse<TaskPriority>(task.Priority.ToString(), out var priority)
                ? priority
                : TaskPriority.Medium,
            Status = Enum.TryParse<TaskStatus>(task.Status.ToString(), out var status)
                ? status
                : TaskStatus.Pending,
            ExecutionLog = logMessages != null && logMessages.Any()
                ? string.Join("\n", logMessages)
                : $"No log available for task: {task.Name}"
        };
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(); // Correct for modal navigation
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        LoadTaskDetails();
    }
}