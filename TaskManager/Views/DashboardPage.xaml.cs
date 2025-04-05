using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TaskManager.Models;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using TaskManager.Repositories;
using TaskManager.Services;

namespace TaskManager.Views
{
    public partial class DashboardPage : ContentPage
    {
        private TaskService _taskService;
        private ITaskRepository _taskRepository;
        
        // Lists to store task data
        private List<BaseTask> _completedTasks;
        private List<BaseTask> _pendingTasks;
        private List<BaseTask> _failedTasks;

        public DashboardPage()
        {
            InitializeComponent();
            
            // Initialize empty lists
            _completedTasks = new List<BaseTask>();
            _pendingTasks = new List<BaseTask>();
            _failedTasks = new List<BaseTask>();
            
            // Initialize chart drawables with empty data
            BarChartView.Drawable = new BarChartDrawable();
            PieChartView.Drawable = new PieChartDrawable();
            LineChartView.Drawable = new LineChartDrawable();
            
            // Load task statistics when the page appears
            this.Appearing += OnPageAppearing;
        }

        private void OnPageAppearing(object sender, EventArgs e)
        {
            LoadTaskData();
            UpdateCharts();
            UpdateSummaryLabels();
        }

        private void LoadTaskData()
        {
            try
            {
                // Get the repositories and services from DI
                _taskRepository = ServiceProvider.GetService<ITaskRepository>();
                _taskService = ServiceProvider.GetService<TaskService>();
                
                if (_taskRepository != null)
                {
                    // Load tasks by status
                    _completedTasks = _taskRepository.GetTasks(StatusType.Completed);
                    _pendingTasks = _taskRepository.GetTasks(StatusType.Pending);
                    _failedTasks = _taskRepository.GetTasks(StatusType.Failed);
                }
                else
                {
                    Debug.WriteLine("TaskRepository is null");
                    DisplayAlert("Error", "Could not initialize task repository", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading task data: {ex.Message}");
                DisplayAlert("Error", $"Failed to load task data: {ex.Message}", "OK");
            }
        }

        private void UpdateCharts()
        {
            try
            {
                // Update bar chart with hourly data
                BarChartView.Drawable = new BarChartDrawable(_completedTasks, _failedTasks);
                
                // Update pie chart with task distribution
                PieChartView.Drawable = new PieChartDrawable(
                    _completedTasks.Count,
                    _pendingTasks.Count,
                    _failedTasks.Count
                );
                
                // Update line chart with daily completed tasks
                LineChartView.Drawable = new LineChartDrawable(_completedTasks);
                
                // Invalidate to redraw the charts
                BarChartView.Invalidate();
                PieChartView.Invalidate();
                LineChartView.Invalidate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating charts: {ex.Message}");
            }
        }

        private void UpdateSummaryLabels()
        {
            try
            {
                // Update summary labels
                TotalTaskLabel.Text = (_completedTasks.Count + _pendingTasks.Count + _failedTasks.Count).ToString();
                CompletedTaskLabel.Text = _completedTasks.Count.ToString();
                FailedTaskLabel.Text = _failedTasks.Count.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating summary labels: {ex.Message}");
            }
        }

        private async void OnTaskListClicked(object sender, EventArgs e)
        {
            try
            {
                var taskListPage = new TaskListPage();
                await Navigation.PushAsync(taskListPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation failed: {ex.Message}");
                await DisplayAlert("Error", "Unable to open task list", "Confirm");
            }
        }
        
        private IServiceProvider ServiceProvider => 
            Application.Current.MainPage?.Handler?.MauiContext?.Services 
            ?? throw new InvalidOperationException("Unable to obtain service provider");

        private async void OnTaskDetailClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                var selectedTask = button.BindingContext as TaskLog;
                if (selectedTask != null)
                {
                    await Navigation.PushAsync(new TaskDetails(selectedTask));
                }
                else
                {
                    await DisplayAlert("Error", "No task selected.", "OK");
                }
            }
        }
    }
    
    // Bar Chart Implementation - Tasks by Hour
    public class BarChartDrawable : IDrawable
    {
        private readonly List<BaseTask> _completedTasks;
        private readonly List<BaseTask> _failedTasks;

        public BarChartDrawable()
        {
            // Initialize with empty lists - will be populated before drawing
            _completedTasks = new List<BaseTask>();
            _failedTasks = new List<BaseTask>();
        }

        public BarChartDrawable(List<BaseTask> completedTasks, List<BaseTask> failedTasks)
        {
            _completedTasks = completedTasks;
            _failedTasks = failedTasks;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Group tasks by hour and count them
            var completedByHour = GroupTasksByHour(_completedTasks);
            var failedByHour = GroupTasksByHour(_failedTasks);

            // Get hours for x-axis (from 0 to 23)
            var hours = Enumerable.Range(0, 24).ToArray();
            
            float chartMargin = 20;
            float chartHeight = dirtyRect.Height - (2 * chartMargin);
            float chartWidth = dirtyRect.Width - (2 * chartMargin);
            
            // Calculate max value for scaling
            int maxValue = 0;
            foreach (var hour in hours)
            {
                int completedCount = completedByHour.ContainsKey(hour) ? completedByHour[hour] : 0;
                int failedCount = failedByHour.ContainsKey(hour) ? failedByHour[hour] : 0;
                int total = completedCount + failedCount;
                if (total > maxValue)
                    maxValue = total;
            }

            // If no data, set default max value to avoid division by zero
            if (maxValue == 0)
                maxValue = 10;
            
            // Scale factor
            float yScale = chartHeight / (maxValue * 1.1f);
            
            // X-axis labels and ticks
            canvas.StrokeColor = Colors.Black;
            canvas.DrawLine(chartMargin, dirtyRect.Height - chartMargin, 
                           dirtyRect.Width - chartMargin, dirtyRect.Height - chartMargin);
            
            // Only show a subset of hours to avoid crowding
            var visibleHours = new[] { 0, 4, 8, 12, 16, 20 };
            
            float barWidth = chartWidth / (24 * 2); // 24 hours, 2 bars per hour
            float barSpacing = barWidth / 2;
            
            // Draw bars and labels
            for (int i = 0; i < 24; i++)
            {
                float x = chartMargin + (i * (barWidth * 2 + barSpacing));
                
                // Draw hour label (only for selected hours)
                if (visibleHours.Contains(i))
                {
                    canvas.FontSize = 10;
                    canvas.DrawString($"{i}:00", x, dirtyRect.Height - chartMargin + 15, HorizontalAlignment.Center);
                }
                
                // Draw completed task bar
                int completedCount = completedByHour.ContainsKey(i) ? completedByHour[i] : 0;
                canvas.FillColor = Colors.DodgerBlue;
                float completedHeight = completedCount * yScale;
                if (completedHeight > 0)
                {
                    canvas.FillRectangle(x, dirtyRect.Height - chartMargin - completedHeight, 
                                        barWidth, completedHeight);
                }
                
                // Draw failed task bar
                int failedCount = failedByHour.ContainsKey(i) ? failedByHour[i] : 0;
                canvas.FillColor = Colors.OrangeRed;
                float failedHeight = failedCount * yScale;
                if (failedHeight > 0)
                {
                    canvas.FillRectangle(x + barWidth, dirtyRect.Height - chartMargin - failedHeight, 
                                        barWidth, failedHeight);
                }
            }
            
            // Draw legend
            canvas.FillColor = Colors.DodgerBlue;
            canvas.FillRectangle(chartMargin, chartMargin, 10, 10);
            canvas.FontSize = 12;
            canvas.DrawString("Completed", chartMargin + 15, chartMargin + 5, HorizontalAlignment.Left);
            
            canvas.FillColor = Colors.OrangeRed;
            canvas.FillRectangle(chartMargin + 100, chartMargin, 10, 10);
            canvas.DrawString("Failed", chartMargin + 115, chartMargin + 5, HorizontalAlignment.Left);
        }

        private Dictionary<int, int> GroupTasksByHour(List<BaseTask> tasks)
        {
            var tasksByHour = new Dictionary<int, int>();
            
            foreach (var task in tasks)
            {
                if (task.MarkCompleted)
                {
                    int hour = task.CompletionTime.Value.Hour;
                    if (tasksByHour.ContainsKey(hour))
                        tasksByHour[hour]++;
                    else
                        tasksByHour[hour] = 1;
                }
            }
            
            return tasksByHour;
        }
    }

    // Pie Chart Implementation - Task Status Distribution
    public class PieChartDrawable : IDrawable
    {
        private int _completedTasksCount;
        private int _pendingTasksCount;
        private int _failedTasksCount;

        public PieChartDrawable()
        {
            // Default values
            _completedTasksCount = 0;
            _pendingTasksCount = 0;
            _failedTasksCount = 0;
        }

        public PieChartDrawable(int completedTasksCount, int pendingTasksCount, int failedTasksCount)
        {
            _completedTasksCount = completedTasksCount;
            _pendingTasksCount = pendingTasksCount;
            _failedTasksCount = failedTasksCount;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            int total = _completedTasksCount + _pendingTasksCount + _failedTasksCount;
            
            // If no data, use default values
            if (total == 0)
            {
                _completedTasksCount = 1;
                _pendingTasksCount = 1;
                _failedTasksCount = 1;
                total = 3;
            }
            
            // Calculate percentages
            float completedPercentage = (float)_completedTasksCount / total * 100;
            float pendingPercentage = (float)_pendingTasksCount / total * 100;
            float failedPercentage = (float)_failedTasksCount / total * 100;
            
            // Data (in percentages)
            float[] data = new float[] { completedPercentage, pendingPercentage, failedPercentage };
            string[] labels = new string[] { "Completed", "In Progress", "Failed" };
            Color[] colors = new Color[] { Colors.MediumSeaGreen, Colors.DodgerBlue, Colors.Crimson };
            
            float centerX = dirtyRect.Width / 2;
            float centerY = dirtyRect.Height / 2;
            float radius = Math.Min(centerX, centerY) - 30;
            
            float startAngle = 0;
            
            for (int i = 0; i < data.Length; i++)
            {
                float sweepAngle = data[i] * 3.6f; // Convert percentage to degrees (360 / 100 = 3.6)
                
                // Draw pie slice
                canvas.FillColor = colors[i];
                canvas.FillArc(centerX - radius, centerY - radius, 
                              radius * 2, radius * 2, 
                              startAngle, sweepAngle, true);
                
                // Calculate text position (mid-point of the arc)
                float midAngle = startAngle + (sweepAngle / 2);
                float textRadius = radius * 0.7f;
                float textX = centerX + (float)(textRadius * Math.Cos(midAngle * Math.PI / 180));
                float textY = centerY + (float)(textRadius * Math.Sin(midAngle * Math.PI / 180));
                
                // Draw percentage text
                canvas.FontSize = 12;
                canvas.FontColor = Colors.White;
                canvas.DrawString($"{data[i]:F1}%", textX, textY, HorizontalAlignment.Center);
                
                startAngle += sweepAngle;
            }
            
            // Draw legend
            float legendX = 20;
            float legendY = dirtyRect.Height - 60;
            
            for (int i = 0; i < labels.Length; i++)
            {
                canvas.FillColor = colors[i];
                canvas.FillRectangle(legendX, legendY + (i * 20), 10, 10);
                
                canvas.FontSize = 12;
                canvas.FontColor = Colors.Black;
                canvas.DrawString($"{labels[i]} ({GetCountForIndex(i)})", legendX + 15, legendY + (i * 20) + 5, HorizontalAlignment.Left);
            }
        }

        private int GetCountForIndex(int index)
        {
            return index switch
            {
                0 => _completedTasksCount,
                1 => _pendingTasksCount,
                2 => _failedTasksCount,
                _ => 0
            };
        }
    }

    // Line Chart Implementation - Completed Tasks Daily
    public class LineChartDrawable : IDrawable
    {
        private readonly List<BaseTask> _completedTasks;
        private readonly int _daysToShow = 7; // Show last 7 days

        public LineChartDrawable()
        {
            // Initialize with empty list - will be populated before drawing
            _completedTasks = new List<BaseTask>();
        }

        public LineChartDrawable(List<BaseTask> completedTasks)
        {
            _completedTasks = completedTasks;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-(_daysToShow - 1));
            
            // Group tasks by day and count them
            var tasksByDay = GroupTasksByDay(_completedTasks, startDate, today);
            
            // Create arrays for drawing
            var dates = new List<DateTime>();
            var counts = new List<int>();
            
            // Fill arrays with data for each day
            for (int i = 0; i < _daysToShow; i++)
            {
                var date = startDate.AddDays(i);
                dates.Add(date);
                
                int count = 0;
                if (tasksByDay.ContainsKey(date))
                    count = tasksByDay[date];
                
                counts.Add(count);
            }
            
            float chartMargin = 20;
            float chartHeight = dirtyRect.Height - (2 * chartMargin);
            float chartWidth = dirtyRect.Width - (2 * chartMargin);
            
            // Calculate max value for scaling
            int maxValue = counts.Count > 0 ? counts.Max() : 0;
            
            // If no data or all zeros, set default max value
            if (maxValue == 0)
                maxValue = 10;
            
            // Scale factor
            float yScale = chartHeight / (maxValue * 1.1f);
            
            // Draw X and Y axes
            canvas.StrokeColor = Colors.Gray;
            canvas.DrawLine(chartMargin, chartMargin, 
                           chartMargin, dirtyRect.Height - chartMargin);
            canvas.DrawLine(chartMargin, dirtyRect.Height - chartMargin, 
                           dirtyRect.Width - chartMargin, dirtyRect.Height - chartMargin);
            
            // Calculate point spacing
            float pointSpacing = chartWidth / (_daysToShow - 1);
            
            // Draw connecting lines
            PathF path = new PathF();
            
            for (int i = 0; i < _daysToShow; i++)
            {
                float x = chartMargin + (i * pointSpacing);
                float y = dirtyRect.Height - chartMargin - (counts[i] * yScale);
                
                if (i == 0)
                    path.MoveTo(x, y);
                else
                    path.LineTo(x, y);
                
                // Draw X-axis labels (date)
                canvas.FontSize = 10;
                canvas.FontColor = Colors.Black;
                canvas.DrawString(dates[i].ToString("MM/dd"), x, dirtyRect.Height - chartMargin + 15, HorizontalAlignment.Center);
            }
            
            // Draw line
            canvas.StrokeColor = Colors.DodgerBlue;
            canvas.StrokeSize = 2;
            canvas.DrawPath(path);
            
            // Draw points and value labels
            for (int i = 0; i < _daysToShow; i++)
            {
                float x = chartMargin + (i * pointSpacing);
                float y = dirtyRect.Height - chartMargin - (counts[i] * yScale);
                
                canvas.FillColor = Colors.White;
                canvas.StrokeColor = Colors.DodgerBlue;
                canvas.StrokeSize = 2;
                canvas.FillCircle(x, y, 5);
                canvas.DrawCircle(x, y, 5);
                
                // Draw value labels
                canvas.FontSize = 10;
                canvas.FontColor = Colors.Black;
                canvas.DrawString(counts[i].ToString(), x, y - 15, HorizontalAlignment.Center);
            }
            
            // Draw title
            canvas.FontSize = 12;
            canvas.FontColor = Colors.Black;
            canvas.DrawString("Completed Tasks - Last 7 Days", dirtyRect.Width / 2, chartMargin - 10, HorizontalAlignment.Center);
        }

        private Dictionary<DateTime, int> GroupTasksByDay(List<BaseTask> tasks, DateTime startDate, DateTime endDate)
        {
            var tasksByDay = new Dictionary<DateTime, int>();
            
            // Initialize dictionary with all dates in range (with zero counts)
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                tasksByDay[date.Date] = 0;
            }
            
            // Count tasks for each day
            foreach (var task in tasks)
            {
                if (task.CompletionTime.HasValue)
                {
                    DateTime completionDate = task.CompletionTime.Value.Date;
                    if (completionDate >= startDate && completionDate <= endDate)
                    {
                        tasksByDay[completionDate]++;
                    }
                }
            }
            
            return tasksByDay;
        }
    }
}