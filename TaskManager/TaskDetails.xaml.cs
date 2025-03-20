namespace TaskManager;

public partial class TaskDetails : ContentPage
{
    public TaskDetails(TaskLog task)
    {
        InitializeComponent();
        BindingContext = task; // Bind this page to the task details
    }
}