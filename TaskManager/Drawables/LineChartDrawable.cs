public class LineChartDrawable : IDrawable
{
    private bool _dataLoaded;
    private DateTime _lastRefreshTime;
    private List<DailyTaskData> _taskData;

    // Constructor
    public LineChartDrawable()
    {
        _taskData = new List<DailyTaskData>();
        _lastRefreshTime = DateTime.Now;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (!_dataLoaded || _taskData.Count == 0)
        {
            DrawEmptyChart(canvas, dirtyRect);
            return;
        }

        // Chart dimensions and padding
        float padding = 50;
        float bottomPadding = 70; // Extra space for labels
        var chartWidth = dirtyRect.Width - padding * 2;
        var chartHeight = dirtyRect.Height - padding - bottomPadding;

        // Chart area
        var chartLeft = padding;
        var chartTop = padding;
        var chartBottom = chartTop + chartHeight;

        // Draw title
        canvas.FontSize = 16;
        canvas.FontColor = Colors.Black;
        canvas.DrawString("Task Trends Over Time", dirtyRect.Width / 2, 20, HorizontalAlignment.Center);

        // Find max value for scaling
        var maxCount = 10; // Minimum scale
        foreach (var data in _taskData)
        {
            var totalTasks = data.CompletedCount + data.PendingCount + data.FailedCount;
            maxCount = Math.Max(maxCount,
                Math.Max(totalTasks, Math.Max(data.CompletedCount, Math.Max(data.PendingCount, data.FailedCount))));
        }

        // Round up max value to nearest 10 for nicer scale
        maxCount = (int)Math.Ceiling(maxCount / 10.0) * 10;

        // Draw Y-axis and grid lines
        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 1;
        canvas.DrawLine(chartLeft, chartTop, chartLeft, chartBottom);

        // Y-axis labels and grid lines
        var yLabelCount = 5; // Number of horizontal grid lines
        canvas.FontSize = 10;
        for (var i = 0; i <= yLabelCount; i++)
        {
            var y = chartBottom - i * (chartHeight / yLabelCount);
            var value = i * maxCount / yLabelCount;

            // Draw grid line
            canvas.StrokeColor = Colors.LightGray;
            canvas.StrokeSize = 0.5f;
            canvas.DrawLine(chartLeft, y, chartLeft + chartWidth, y);

            // Draw label
            canvas.FontColor = Colors.DarkGray;
            canvas.DrawString(value.ToString(), chartLeft - 5, y, HorizontalAlignment.Right);
        }

        // X-axis
        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 1;
        canvas.DrawLine(chartLeft, chartBottom, chartLeft + chartWidth, chartBottom);

        // Calculate point spacing
        var pointSpacing = chartWidth / (_taskData.Count - 1);
        if (_taskData.Count == 1)
            pointSpacing = 0; // Handle single data point case

        // Draw data lines
        DrawDataLine(canvas, chartLeft, chartBottom, pointSpacing, chartHeight, maxCount,
            _taskData.Select(d => d.CompletedCount).ToList(), Colors.MediumSeaGreen, true);

        DrawDataLine(canvas, chartLeft, chartBottom, pointSpacing, chartHeight, maxCount,
            _taskData.Select(d => d.PendingCount).ToList(), Colors.DodgerBlue, true);

        DrawDataLine(canvas, chartLeft, chartBottom, pointSpacing, chartHeight, maxCount,
            _taskData.Select(d => d.FailedCount).ToList(), Colors.Crimson, true);

        // Draw X-axis labels (dates)
        canvas.FontColor = Colors.Black;
        canvas.FontSize = 10;

        // If we have many data points, we may want to show fewer labels
        var labelInterval = Math.Max(1, _taskData.Count / 7); // Show at most 7 labels

        for (var i = 0; i < _taskData.Count; i += labelInterval)
        {
            var x = chartLeft + i * pointSpacing;
            var dateLabel = _taskData[i].Date.ToString("MM/dd");
            canvas.DrawString(dateLabel, x, chartBottom + 15, HorizontalAlignment.Center);

            // Draw vertical grid line
            canvas.StrokeColor = Colors.LightGray;
            canvas.StrokeSize = 0.5f;
            canvas.DrawLine(x, chartTop, x, chartBottom);
        }

        // Draw last refresh time
        canvas.FontSize = 10;
        canvas.FontColor = Colors.DarkGray;
        canvas.DrawString($"Last updated: {_lastRefreshTime:HH:mm:ss}",
            dirtyRect.Width - padding, dirtyRect.Height - 15,
            HorizontalAlignment.Right);

        // Draw legend
        DrawLegend(canvas, dirtyRect);
    }

    // Method to update data from database
    public void UpdateData(List<DailyTaskData> data)
    {
        _taskData = data ?? new List<DailyTaskData>();
        _dataLoaded = true;
        _lastRefreshTime = DateTime.Now;
    }

    private void DrawDataLine(ICanvas canvas, float chartLeft, float chartBottom, float pointSpacing,
        float chartHeight, int maxValue, List<int> dataPoints,
        Color lineColor, bool fillPoints)
    {
        if (dataPoints.Count < 2)
            return;

        // Set line style
        canvas.StrokeColor = lineColor;
        canvas.StrokeSize = 2;

        // Draw the line segments
        for (var i = 0; i < dataPoints.Count - 1; i++)
        {
            var x1 = chartLeft + i * pointSpacing;
            var y1 = chartBottom - (float)dataPoints[i] / maxValue * chartHeight;

            var x2 = chartLeft + (i + 1) * pointSpacing;
            var y2 = chartBottom - (float)dataPoints[i + 1] / maxValue * chartHeight;

            canvas.DrawLine(x1, y1, x2, y2);
        }

        // Draw points
        if (fillPoints)
        {
            float pointRadius = 4;

            for (var i = 0; i < dataPoints.Count; i++)
            {
                var x = chartLeft + i * pointSpacing;
                var y = chartBottom - (float)dataPoints[i] / maxValue * chartHeight;

                canvas.FillColor = lineColor;
                canvas.FillCircle(x, y, pointRadius);

                canvas.StrokeColor = Colors.White;
                canvas.StrokeSize = 1;
                canvas.DrawCircle(x, y, pointRadius);
            }
        }
    }

    private void DrawLegend(ICanvas canvas, RectF dirtyRect)
    {
        var legendY1 = dirtyRect.Height - 50;
        var legendY2 = dirtyRect.Height - 30;
        var legendXStart = dirtyRect.Width / 2 - 120;
        float legendXSpacing = 80;

        // Completed legend
        canvas.StrokeColor = Colors.MediumSeaGreen;
        canvas.StrokeSize = 2;
        canvas.DrawLine(legendXStart, legendY1 + 6, legendXStart + 15, legendY1 + 6);
        canvas.FillColor = Colors.MediumSeaGreen;
        canvas.FillCircle(legendXStart + 7, legendY1 + 6, 4);
        canvas.FontColor = Colors.Black;
        canvas.FontSize = 12;
        canvas.DrawString("Completed", legendXStart + 20, legendY1 + 6, HorizontalAlignment.Left);

        // Pending legend
        canvas.StrokeColor = Colors.DodgerBlue;
        canvas.StrokeSize = 2;
        canvas.DrawLine(legendXStart + legendXSpacing, legendY1 + 6, legendXStart + legendXSpacing + 15, legendY1 + 6);
        canvas.FillColor = Colors.DodgerBlue;
        canvas.FillCircle(legendXStart + legendXSpacing + 7, legendY1 + 6, 4);
        canvas.FontColor = Colors.Black;
        canvas.FontSize = 12;
        canvas.DrawString("In Progress", legendXStart + legendXSpacing + 20, legendY1 + 6, HorizontalAlignment.Left);

        // Failed legend
        canvas.StrokeColor = Colors.Crimson;
        canvas.StrokeSize = 2;
        canvas.DrawLine(legendXStart + legendXSpacing * 2, legendY1 + 6, legendXStart + legendXSpacing * 2 + 15,
            legendY1 + 6);
        canvas.FillColor = Colors.Crimson;
        canvas.FillCircle(legendXStart + legendXSpacing * 2 + 7, legendY1 + 6, 4);
        canvas.FontColor = Colors.Black;
        canvas.FontSize = 12;
        canvas.DrawString("Failed", legendXStart + legendXSpacing * 2 + 20, legendY1 + 6, HorizontalAlignment.Left);
    }

    private void DrawEmptyChart(ICanvas canvas, RectF dirtyRect)
    {
        // Draw empty chart with message
        var centerX = dirtyRect.Width / 2;
        var centerY = dirtyRect.Height / 2;

        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 14;
        canvas.DrawString("No task trend data available",
            centerX, centerY, HorizontalAlignment.Center);

        // Draw title
        canvas.FontSize = 16;
        canvas.FontColor = Colors.Black;
        canvas.DrawString("Task Trends Over Time", dirtyRect.Width / 2, 20, HorizontalAlignment.Center);
    }
}

// Class to hold daily task data
public class DailyTaskData
{
    public DateTime Date { get; set; }
    public int CompletedCount { get; set; }
    public int PendingCount { get; set; }
    public int FailedCount { get; set; }
}