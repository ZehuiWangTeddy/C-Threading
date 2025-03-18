using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using TaskManager.Views;
using TaskManager.Models; 

namespace TaskManager.Views
{
    public partial class TaskListPage : ContentPage, INotifyPropertyChanged
    {
        public ObservableCollection<TaskItem> Tasks { get; } = new();

        public TaskListPage()
        {
            InitializeComponent();
            BindingContext = this;
            
            // dummy data
            Tasks.Add(new TaskItem { 
                Name = "Project Meeting", 
                TaskType = "Folder Watcher Task", 
                ExecutionTime = DateTime.Now.AddHours(2)
            });
            
            Tasks.Add(new TaskItem { 
                Name = "Project Fighting", 
                TaskType = "Folder Watcher Task", 
                ExecutionTime = DateTime.Now.AddHours(5)
            });
        }

        private async void OnAddTaskClicked(object sender, EventArgs e)
        {
            var addPage = new AddTaskPage();
            addPage.TaskAdded += (s, task) => 
            {
                Tasks.Add(task);
            };
            
            await Navigation.PushModalAsync(new NavigationPage(addPage));
        }

        private void OnStartTask(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is TaskItem task)
            {
                DisplayAlert("Task Started", $"Starting: {task.Name}", "OK");
            }
        }

        private void OnCancelTask(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is TaskItem task)
            {
                Tasks.Remove(task);
            }
        }
        
        private async void OnDetailsClicked(object sender, System.EventArgs e)
        {
            // 暂时显示提示信息
            await DisplayAlert("Coming Soon", "Task detail feature is under development", "OK");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}