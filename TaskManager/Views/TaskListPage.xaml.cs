using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using TaskManager.Views;

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
                StartTime = "09:00", 
                EndTime = "10:30" 
            });
            
            Tasks.Add(new TaskItem { 
                Name = "Project Fighting", 
                StartTime = "10:00", 
                EndTime = "11:00" 
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TaskItem
    {
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}