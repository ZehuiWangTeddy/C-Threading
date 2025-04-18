using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using TaskManager.Messages;
using TaskManager.Models;
using TaskManager.Models.Enums;
using TaskManager.Repositories;
using TaskManager.ViewModels;
using static TaskManager.Messages.DbOperationMessage;

namespace TaskManager.Views;

public partial class TaskListPage : ContentPage
{
    public TaskListPage()
    {
        InitializeComponent();
        var dataContext = IPlatformApplication.Current?.Services.GetRequiredService<TaskListViewModel>();
        BindingContext = dataContext;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TaskListViewModel viewModel) viewModel.OnPageAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is TaskListViewModel viewModel) viewModel.OnPageDisappearing();
    }

    private async void OnAddTaskClicked(object sender, EventArgs e)
    {
        try
        {
            {
                var addPage = new AddTaskPage();
                await Navigation.PushModalAsync(addPage);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Open Page Faild: {ex.Message}");
            await DisplayAlert("Error", "Cannot open add task page", "Confirm");
        }
    }

    private async void OnStartTask(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is TaskItem task)
        {
            if (task.NextRunTime != DateTime.MinValue)
            {
                await DisplayAlert("Task", "Cannot start regular task early", "OK");
                return;
            }

            if (task.Status == StatusType.Completed)
            {
                await DisplayAlert("Task Completed", $"Task {task.Name} has been completed", "OK");
                return;
            }

            await DisplayAlert("Task Start", $"Start: {task.Name}", "OK");

            WeakReferenceMessenger.Default.Send(new DbOperationMessage(nameof(TaskListPage),
                nameof(TaskListViewModel), task, OperationType.Update));
        }
    }

    private async void OnCancelTask(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is TaskItem task)
        {
            if (task.Status == StatusType.Completed)
            {
                await DisplayAlert("Task Completed", $"Task {task.Name} has been completed", "OK");
                return;
            }

            var confirm = await DisplayAlert("Confirm", $"Do you want to cancle {task.Name} ？", "YES", "NO");
            if (confirm)
                WeakReferenceMessenger.Default.Send(new DbOperationMessage(nameof(TaskListPage),
                    nameof(TaskListViewModel), task, OperationType.Delete));
        }
    }

    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is TaskItem task)
            try
            {
                // Retrieve the ITaskRepository instance from the DI container
                var taskRepository = IPlatformApplication.Current?.Services.GetRequiredService<ITaskRepository>();

                if (taskRepository != null)
                    // Navigate to the TaskDetails page with the repository and task ID
                    await Navigation.PushModalAsync(new TaskDetails(taskRepository, task.Id));
                else
                    await DisplayAlert("Error", "Task repository not found.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving task details: {ex.Message}");
                await DisplayAlert("Error", "Failed to retrieve task details.", "OK");
            }
    }
}