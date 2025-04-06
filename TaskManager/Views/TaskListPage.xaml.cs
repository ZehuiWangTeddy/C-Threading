using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using TaskManager.Models;
using TaskManager.Repositories;
using TaskManager.Services;
using System.Diagnostics;
using TaskManager.Models.DBModels;
using TaskManager.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using TaskManager.Messages;
using static TaskManager.Messages.DbOperationMessage;
using System.Threading.Tasks;
using TaskManager.Models.Enums;

namespace TaskManager.Views
{
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
            if (BindingContext is TaskListViewModel viewModel)
            {
                viewModel.OnPageAppearing();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (BindingContext is TaskListViewModel viewModel)
            {
                viewModel.OnPageDisappearing();
            }
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
                if (task.Status == StatusType.Completed)
                {
                    await DisplayAlert("Task Completed", $"Task {task.Name} has been completed", "OK");
                    return;
                }
                await DisplayAlert("Task Start", $"Start: {task.Name}", "OK");

                WeakReferenceMessenger.Default.Send(new DbOperationMessage(nameof(TaskListPage), nameof(TaskListViewModel), task, OperationType.Update));

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
                bool confirm = await DisplayAlert("Confirm", $"Do you want to cancle {task.Name} ？", "YES", "NO");
                if (confirm)
                {
                    WeakReferenceMessenger.Default.Send(new DbOperationMessage(nameof(TaskListPage), nameof(TaskListViewModel), task, OperationType.Delete));
                }
            }
        }

        private async void OnDetailsClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is TaskItem task)
            {
                var taskLog = new TaskLog
                {
                    TaskName = task.Name,
                    ExecutionTime = task.ExecutionTime.ToString("HH:mm:ss"),
                    Priority = TaskPriority.Medium, // Example priority
                    Status = TaskStatus.Pending, // Example status
                    ThreadId = 1, // Example thread ID
                    ExecutionLog = "Example log" // Example log
                };

                //You need replace TaskLog -->TaskItem
                await Navigation.PushModalAsync(new NavigationPage(new TaskDetails(taskLog)));
            }
        }

        

    }
}