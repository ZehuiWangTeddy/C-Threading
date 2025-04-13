using System.Diagnostics;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using TaskManager.Repositories;
using TaskManager.Services;

namespace TaskManager.Views;

public partial class DashboardPage : ContentPage
{
    private List<BaseTask> _completedTasks;
    private List<BaseTask> _failedTasks;
    private List<BaseTask> _pendingTasks;
    private ITaskRepository _taskRepository;
    private TaskService _taskService;

    public DashboardPage()
    {
        InitializeComponent();

        _completedTasks = new List<BaseTask>();
        _pendingTasks = new List<BaseTask>();
        _failedTasks = new List<BaseTask>();

        BarChartView.Drawable = new BarChartDrawable();
        PieChartView.Drawable = new PieChartDrawable();
        LineChartView.Drawable = new LineChartDrawable();
    }

    private IServiceProvider ServiceProvider =>
        Application.Current.MainPage?.Handler?.MauiContext?.Services
        ?? throw new InvalidOperationException("Unable to obtain service provider");

    private void OnPageRefresh(object sender, EventArgs e)
    {
        LoadTaskData();
        UpdateCharts();
        UpdateSummaryLabels();
        LoadRecentTasks();
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Delay(100);

        LoadTaskData();
        UpdateCharts();
        UpdateSummaryLabels();
        LoadRecentTasks();
    }


    private void LoadRecentTasks()
    {
        try
        {
            _taskRepository = ServiceProvider.GetService<ITaskRepository>();
            var recentTasks = _taskRepository.GetRecentTasks(5);

            RecentTasksContainer.Children.Clear();

            var index = 1;
            foreach (var task in recentTasks)
            {
                var taskName = string.IsNullOrWhiteSpace(task.Name) ? "Unnamed Task" : task.Name;
                var priority = task.Priority.ToString();
                var labelText = $"{index}. {taskName} – {priority}";

                RecentTasksContainer.Children.Add(new Label
                {
                    Text = labelText,
                    TextColor = Colors.Black,
                    FontSize = 14
                });

                index++;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load recent tasks: {ex.Message}");
        }
    }


    private void LoadTaskData()
    {
        try
        {
            _taskRepository = ServiceProvider.GetService<ITaskRepository>();
            _taskService = ServiceProvider.GetService<TaskService>();

            if (_taskRepository != null)
            {
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
        }
    }

    private void UpdateCharts()
    {
        try
        {
            // Update bar chart with hourly data
            var barChart = BarChartView.Drawable as BarChartDrawable;
            if (barChart == null)
            {
                barChart = new BarChartDrawable();
                BarChartView.Drawable = barChart;
            }

            barChart.UpdateData(_completedTasks, _failedTasks);

            // Update pie chart with task distribution
            var pieChart = PieChartView.Drawable as PieChartDrawable;
            if (pieChart == null)
            {
                pieChart = new PieChartDrawable();
                PieChartView.Drawable = pieChart;
            }

            pieChart.UpdateData(
                _completedTasks.Count,
                _pendingTasks.Count,
                _failedTasks.Count
            );

            // Update line chart with daily completed tasks
            var completedTasks = _taskRepository.GetCompletedTasks();
            var lineChartDrawable = new LineChartDrawable(completedTasks);

            LineChartView.Drawable = lineChartDrawable;
            LineChartView.Invalidate();

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
}

// Bar Chart Implementation - Tasks by Hour
public class BarChartDrawable : IDrawable
{
    private List<BaseTask> _completedTasks;
    private List<BaseTask> _failedTasks;

    public BarChartDrawable()
    {
        _completedTasks = new List<BaseTask>();
        _failedTasks = new List<BaseTask>();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var completedByHour = GroupTasksByHour(_completedTasks);
        var failedByHour = GroupTasksByHour(_failedTasks);

        var hours = Enumerable.Range(0, 24).ToArray();

        float chartMargin = 20;
        var chartHeight = dirtyRect.Height - 2 * chartMargin;
        var chartWidth = dirtyRect.Width - 2 * chartMargin;

        var maxValue = 0;
        foreach (var hour in hours)
        {
            var completedCount = completedByHour.ContainsKey(hour) ? completedByHour[hour] : 0;
            var failedCount = failedByHour.ContainsKey(hour) ? failedByHour[hour] : 0;
            var total = completedCount + failedCount;
            if (total > maxValue)
                maxValue = total;
        }

        if (maxValue == 0)
            maxValue = 10;

        var yScale = chartHeight / (maxValue * 1.1f);

        canvas.StrokeColor = Colors.Black;
        canvas.DrawLine(chartMargin, dirtyRect.Height - chartMargin,
            dirtyRect.Width - chartMargin, dirtyRect.Height - chartMargin);

        var visibleHours = new[] { 0, 4, 8, 12, 16, 20 };

        var barWidth = chartWidth / (24 * 2);
        var barSpacing = barWidth / 2;

        for (var i = 0; i < 24; i++)
        {
            var x = chartMargin + i * (barWidth * 2 + barSpacing);

            if (visibleHours.Contains(i))
            {
                canvas.FontSize = 10;
                canvas.DrawString($"{i}:00", x, dirtyRect.Height - chartMargin + 15, HorizontalAlignment.Center);
            }

            var completedCount = completedByHour.ContainsKey(i) ? completedByHour[i] : 0;
            canvas.FillColor = Colors.DodgerBlue;
            var completedHeight = completedCount * yScale;
            if (completedHeight > 0)
            {
                canvas.FillRectangle(x, dirtyRect.Height - chartMargin - completedHeight, barWidth,
                    completedHeight);
                canvas.FontSize = 10;
                canvas.FontColor = Colors.Black;
                canvas.DrawString(
                    completedCount.ToString(),
                    x + barWidth / 2,
                    dirtyRect.Height - chartMargin - completedHeight - 12,
                    HorizontalAlignment.Center
                );
            }

            var failedCount = failedByHour.ContainsKey(i) ? failedByHour[i] : 0;
            canvas.FillColor = Colors.OrangeRed;
            var failedHeight = failedCount * yScale;
            if (failedHeight > 0)
            {
                canvas.FillRectangle(x + barWidth, dirtyRect.Height - chartMargin - failedHeight, barWidth,
                    failedHeight);
                canvas.FontSize = 10;
                canvas.FontColor = Colors.Black;
                canvas.DrawString(
                    failedCount.ToString(),
                    x + barWidth + barWidth / 2,
                    dirtyRect.Height - chartMargin - failedHeight - 12,
                    HorizontalAlignment.Center
                );
            }
        }

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
            if (task.LastCompletionTime.HasValue && task.LastCompletionTime.Value.Date == DateTime.Today)
            {
                var hour = task.LastCompletionTime.Value.Hour;
                if (tasksByHour.ContainsKey(hour))
                    tasksByHour[hour]++;
                else
                    tasksByHour[hour] = 1;
            }

        return tasksByHour;
    }


    public void UpdateData(List<BaseTask> completedTasks, List<BaseTask> failedTasks)
    {
        _completedTasks = completedTasks;
        _failedTasks = failedTasks;
    }
}

//Pie Chart Implementation - By Task Status 
public class PieChartDrawable : IDrawable
{
    private int _completedTasksCount;
    private bool _dataLoaded;
    private int _failedTasksCount;
    private int _pendingTasksCount;

    public PieChartDrawable()
    {
        _completedTasksCount = 0;
        _pendingTasksCount = 0;
        _failedTasksCount = 0;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var total = _completedTasksCount + _pendingTasksCount + _failedTasksCount;

        if (total == 0)
        {
            DrawEmptyChart(canvas, dirtyRect);
            return;
        }

        var completedPercentage = total > 0 ? (float)_completedTasksCount / total * 100 : 0;
        var pendingPercentage = total > 0 ? (float)_pendingTasksCount / total * 100 : 0;
        var failedPercentage = total > 0 ? (float)_failedTasksCount / total * 100 : 0;

        var values = new[] { _completedTasksCount, _pendingTasksCount, _failedTasksCount };
        var percentages = new[] { completedPercentage, pendingPercentage, failedPercentage };
        var labels = new[] { "Completed", "In Progress", "Failed" };
        var colors = new[] { Colors.LimeGreen, Colors.DodgerBlue, Colors.Crimson };

        var centerX = dirtyRect.Width / 2;
        var centerY = dirtyRect.Height / 2;
        var radius = Math.Min(centerX, centerY) - 40;

        float startAngle = 0;

        for (var i = 0; i < values.Length; i++)
        {
            if (values[i] <= 0)
                continue;

            var sweepAngle = percentages[i] * 3.6f;

            canvas.FillColor = colors[i];
            canvas.FillArc(centerX - radius, centerY - radius,
                radius * 2, radius * 2,
                startAngle, sweepAngle, true);

            var midAngle = startAngle + sweepAngle / 2;
            var textRadius = radius * 0.6f;

            var midAngleRadians = midAngle * Math.PI / 180;

            var textX = centerX + (float)(textRadius * Math.Cos(midAngleRadians));
            var textY = centerY + (float)(textRadius * Math.Sin(midAngleRadians));

            canvas.FontSize = 14;
            canvas.FontColor = Colors.White;
            canvas.DrawString($"{percentages[i]:F1}%", textX, textY, HorizontalAlignment.Center);

            startAngle += sweepAngle;
        }

        float legendX = 20;
        var legendY = dirtyRect.Height - 70;
        float legendSpacing = 22;

        for (var i = 0; i < labels.Length; i++)
        {
            canvas.FillColor = colors[i];
            canvas.FillRectangle(legendX, legendY + i * legendSpacing, 12, 12);

            canvas.FontSize = 12;
            canvas.FontColor = Colors.Black;
            canvas.DrawString($"{labels[i]} ({values[i]})", legendX + 18, legendY + i * legendSpacing + 6,
                HorizontalAlignment.Left);
        }
    }

    public void UpdateData(int completedTasksCount, int pendingTasksCount, int failedTasksCount)
    {
        _completedTasksCount = completedTasksCount;
        _pendingTasksCount = pendingTasksCount;
        _failedTasksCount = failedTasksCount;
        _dataLoaded = true;
    }

    private void DrawEmptyChart(ICanvas canvas, RectF dirtyRect)
    {
        var centerX = dirtyRect.Width / 2;
        var centerY = dirtyRect.Height / 2;

        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 2;
        canvas.DrawCircle(centerX, centerY, 100);

        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 14;
        canvas.DrawString("No task data available", centerX, centerY, HorizontalAlignment.Center);
        
        float legendX = 20;
        var legendY = dirtyRect.Height - 70;

        string[] labels = new[] { "Completed", "In Progress", "Failed" };
        Color[] colors = new[] { Colors.MediumSeaGreen, Colors.DodgerBlue, Colors.Crimson };

        for (var i = 0; i < labels.Length; i++)
        {
            canvas.FillColor = colors[i];
            canvas.FillRectangle(legendX, legendY + i * 22, 12, 12);

            canvas.FontSize = 12;
            canvas.FontColor = Colors.Black;
            canvas.DrawString($"{labels[i]} (0)", legendX + 18, legendY + i * 22 + 6, HorizontalAlignment.Left);
        }
    }
}

// Line Chart Implementation - Completed Tasks Daily
public class LineChartDrawable : IDrawable
{
    private readonly List<BaseTask> _completedTasks;
    private readonly int _daysToShow = 7;

    public LineChartDrawable()
    {
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
        var tasksByDay = GroupTasksByDay(_completedTasks, startDate, today);
        var dates = new List<DateTime>();
        var counts = new List<int>();

        for (var i = 0; i < _daysToShow; i++)
        {
            var date = startDate.AddDays(i);
            dates.Add(date);

            var count = 0;
            if (tasksByDay.ContainsKey(date))
                count = tasksByDay[date];

            counts.Add(count);
        }

        float chartMargin = 20;
        var chartHeight = dirtyRect.Height - 2 * chartMargin;
        var chartWidth = dirtyRect.Width - 2 * chartMargin;

        var maxValue = counts.Count > 0 ? counts.Max() : 0;

        if (maxValue == 0)
            maxValue = 10;

        var yScale = chartHeight / (maxValue * 1.1f);

        canvas.StrokeColor = Colors.Gray;
        canvas.DrawLine(chartMargin, chartMargin,
            chartMargin, dirtyRect.Height - chartMargin);
        canvas.DrawLine(chartMargin, dirtyRect.Height - chartMargin,
            dirtyRect.Width - chartMargin, dirtyRect.Height - chartMargin);

        var pointSpacing = chartWidth / (_daysToShow - 1);
        var path = new PathF();

        for (var i = 0; i < _daysToShow; i++)
        {
            var x = chartMargin + i * pointSpacing;
            var y = dirtyRect.Height - chartMargin - counts[i] * yScale;

            if (i == 0)
                path.MoveTo(x, y);
            else
                path.LineTo(x, y);

            canvas.FontSize = 10;
            canvas.FontColor = Colors.Black;
            canvas.DrawString(dates[i].ToString("MM/dd"), x, dirtyRect.Height - chartMargin + 15,
                HorizontalAlignment.Center);
        }

        canvas.StrokeColor = Colors.DodgerBlue;
        canvas.StrokeSize = 2;
        canvas.DrawPath(path);

        for (var i = 0; i < _daysToShow; i++)
        {
            var x = chartMargin + i * pointSpacing;
            var y = dirtyRect.Height - chartMargin - counts[i] * yScale;

            canvas.FillColor = Colors.White;
            canvas.StrokeColor = Colors.DodgerBlue;
            canvas.StrokeSize = 2;
            canvas.FillCircle(x, y, 5);
            canvas.DrawCircle(x, y, 5);

            canvas.FontSize = 10;
            canvas.FontColor = Colors.Black;
            canvas.DrawString(counts[i].ToString(), x, y - 15, HorizontalAlignment.Center);
        }

        canvas.FontSize = 12;
        canvas.FontColor = Colors.Black;
        canvas.DrawString("Completed Tasks - Last 7 Days", dirtyRect.Width / 2, chartMargin - 10,
            HorizontalAlignment.Center);
    }

    private Dictionary<DateTime, int> GroupTasksByDay(List<BaseTask> tasks, DateTime startDate, DateTime endDate)
    {
        var tasksByDay = new Dictionary<DateTime, int>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1)) tasksByDay[date.Date] = 0;

        foreach (var task in tasks)
            if (task.LastCompletionTime.HasValue)
            {
                var completionDate = task.LastCompletionTime.Value.Date;
                if (completionDate >= startDate && completionDate <= endDate) tasksByDay[completionDate]++;
            }

        return tasksByDay;
    }
}