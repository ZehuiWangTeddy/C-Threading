public class PieChartDrawable : IDrawable
{
    private int _completedTasksCount;
    private bool _dataLoaded;
    private int _failedTasksCount;
    private int _pendingTasksCount;

    public PieChartDrawable()
    {
        // Initialize with zero values
        _completedTasksCount = 0;
        _pendingTasksCount = 0;
        _failedTasksCount = 0;
    }

    public PieChartDrawable(int completedTasksCount, int pendingTasksCount, int failedTasksCount)
    {
        _completedTasksCount = completedTasksCount;
        _pendingTasksCount = pendingTasksCount;
        _failedTasksCount = failedTasksCount;
        _dataLoaded = true;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var total = _completedTasksCount + _pendingTasksCount + _failedTasksCount;

        // No data case - draw empty chart with message
        if (total == 0)
        {
            DrawEmptyChart(canvas, dirtyRect);
            return;
        }

        // Calculate percentages
        var completedPercentage = total > 0 ? (float)_completedTasksCount / total * 100 : 0;
        var pendingPercentage = total > 0 ? (float)_pendingTasksCount / total * 100 : 0;
        var failedPercentage = total > 0 ? (float)_failedTasksCount / total * 100 : 0;

        // Data preparation
        var values = new[] { _completedTasksCount, _pendingTasksCount, _failedTasksCount };
        var percentages = new[] { completedPercentage, pendingPercentage, failedPercentage };
        var labels = new[] { "Completed", "In Progress", "Failed" };
        var colors = new[] { Colors.MediumSeaGreen, Colors.DodgerBlue, Colors.Crimson };

        // Chart dimensions
        var centerX = dirtyRect.Width / 2;
        var centerY = dirtyRect.Height / 2;
        var radius = Math.Min(centerX, centerY) - 40; // Reduced radius to fit better

        float startAngle = 0;

        // Draw each segment
        for (var i = 0; i < values.Length; i++)
        {
            // Skip zero values segments
            if (values[i] <= 0)
                continue;

            var sweepAngle = percentages[i] * 3.6f; // Convert percentage to degrees

            // Draw pie slice
            canvas.FillColor = colors[i];
            canvas.FillArc(centerX - radius, centerY - radius,
                radius * 2, radius * 2,
                startAngle, sweepAngle, true);

            // Calculate label position
            var midAngle = startAngle + sweepAngle / 2;
            var textRadius = radius * 0.7f;

            // Convert angle to radians for correct math calculations
            var midAngleRadians = midAngle * Math.PI / 180;

            var textX = centerX + (float)(textRadius * Math.Cos(midAngleRadians));
            var textY = centerY + (float)(textRadius * Math.Sin(midAngleRadians));

            // Draw percentage label
            canvas.FontSize = 14;
            canvas.FontColor = Colors.White;
            canvas.DrawString($"{percentages[i]:F1}%", textX, textY, HorizontalAlignment.Center);

            startAngle += sweepAngle;
        }

        // Draw legend
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

    // Method to update data from database
    public void UpdateData(int completedTasksCount, int pendingTasksCount, int failedTasksCount)
    {
        _completedTasksCount = completedTasksCount;
        _pendingTasksCount = pendingTasksCount;
        _failedTasksCount = failedTasksCount;
        _dataLoaded = true;
    }

    private void DrawEmptyChart(ICanvas canvas, RectF dirtyRect)
    {
        // Draw empty chart with message
        var centerX = dirtyRect.Width / 2;
        var centerY = dirtyRect.Height / 2;

        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 2;
        canvas.DrawCircle(centerX, centerY, 100);

        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 14;
        canvas.DrawString("No task data available", centerX, centerY, HorizontalAlignment.Center);

        // Draw empty legend
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