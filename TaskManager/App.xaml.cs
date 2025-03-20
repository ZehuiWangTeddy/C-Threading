namespace TaskManager;

public partial class App : Application
{
	public App()
	{
		
		var sampleTask = new TaskLog
		{
			TaskName = "Data Processing",
			ThreadId = 12345,
			ExecutionTime = "00:02:30",
			Priority = TaskPriority.High,
			Status = TaskStatus.Running,
			ExecutionLog = "Task started successfully.\nProcessing data...\nTask completed."
		};
		InitializeComponent();

		MainPage = new TaskDetails(sampleTask);
	}
}
