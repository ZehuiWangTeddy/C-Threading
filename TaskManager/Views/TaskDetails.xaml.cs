// TaskDetails.xaml.cs
                                    using TaskManager.Services;
                                    using TaskManager.Views;
                                    
                                    namespace TaskManager
                                    {
                                        public partial class TaskDetails : ContentPage
                                        {
                                            private readonly TaskDetailsService _taskDetailsService;
                                    
                                            public TaskDetails(TaskDetailsService taskDetailsService, int taskId)
                                            {
                                                InitializeComponent();
                                                _taskDetailsService = taskDetailsService;
                                                var task = _taskDetailsService.GetTaskDetails(taskId);
                                                if (task != null)
                                                {
                                                    BindingContext = new TaskLog
                                                    {
                                                        TaskName = task.Name,
                                                        ThreadId = task.ThreadId ?? 0,
                                                        ExecutionTime = task.ExecutionTime?.ToString() ?? "N/A",
                                                        Priority = (TaskPriority)task.Priority,
                                                        Status = (Views.TaskStatus)task.Status, // Fully qualified name
                                                        ExecutionLog = task.Logger?.GetLogMessages().Any() == true ? string.Join("\n", task.Logger.GetLogMessages()) : "No log available"
                                                    };
                                                }
                                            }
                                    
                                            private async void OnBackButtonClicked(object sender, EventArgs e)
                                            {
                                                await Navigation.PopModalAsync();
                                            }
                                        }
                                    }