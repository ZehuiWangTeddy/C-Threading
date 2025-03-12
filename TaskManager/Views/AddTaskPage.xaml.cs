using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace TaskManager.Views
{
    public partial class AddTaskPage : ContentPage, INotifyPropertyChanged
    {
        public event EventHandler<TaskItem> TaskAdded;

        private string _taskName;
        public string TaskName
        {
            get => _taskName;
            set
            {
                _taskName = value;
                OnPropertyChanged();
            }
        }

        private string _startTime;
        public string StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                OnPropertyChanged();
            }
        }

        private string _endTime;
        public string EndTime
        {
            get => _endTime;
            set
            {
                _endTime = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConfirmCommand { get; }

        public AddTaskPage()
        {
            InitializeComponent();
            BindingContext = this;

            ConfirmCommand = new Command(() =>
            {
                if (!string.IsNullOrWhiteSpace(TaskName))
                {
                    TaskAdded?.Invoke(this, new TaskItem
                    {
                        Name = TaskName,
                        StartTime = StartTime,
                        EndTime = EndTime
                    });
                    Navigation.PopModalAsync();
                }
            });
        }
        
        private async void OnCancelClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Failed", "Close window failed, please try again.", "confirm");
                Console.WriteLine(ex);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}