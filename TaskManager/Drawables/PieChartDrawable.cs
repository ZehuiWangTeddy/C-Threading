using System;
using Microsoft.Maui.Graphics;

public class PieChartDrawable : IDrawable
{
    private int _completedTasksCount;
    private int _pendingTasksCount;
    private int _failedTasksCount;
    private bool _dataLoaded = false;

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

    // Method to update data from database
    public void UpdateData(int completedTasksCount, int pendingTasksCount, int failedTasksCount)
    {
        _completedTasksCount = completedTasksCount;
        _pendingTasksCount = pendingTasksCount;
        _failedTasksCount = failedTasksCount;
        _dataLoaded = true;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        int total = _completedTasksCount + _pendingTasksCount + _failedTasksCount;
        
        // No data case - draw empty chart with message
        if (total == 0)
        {
            DrawEmptyChart(canvas, dirtyRect);
            return;
        }
        
        // Calculate percentages
        float completedPercentage = total > 0 ? ((float)_completedTasksCount / total * 100) : 0;
        float pendingPercentage = total > 0 ? ((float)_pendingTasksCount / total * 100) : 0;
        float failedPercentage = total > 0 ? ((float)_failedTasksCount / total * 100) : 0;
        
        // Data preparation
        var values = new[] { _completedTasksCount, _pendingTasksCount, _failedTasksCount };
        var percentages = new[] { completedPercentage, pendingPercentage, failedPercentage };
        var labels = new[] { "Completed", "In Progress", "Failed" };
        var colors = new[] { Colors.MediumSeaGreen, Colors.DodgerBlue, Colors.Crimson };
        
        // Chart dimensions
        float centerX = dirtyRect.Width / 2;
        float centerY = dirtyRect.Height / 2;
        float radius = Math.Min(centerX, centerY) - 40; // Reduced radius to fit better
        
        float startAngle = 0;
        
        // Draw each segment
        for (int i = 0; i < values.Length; i++)
        {
            // Skip zero values segments
            if (values[i] <= 0)
                continue;
                
            float sweepAngle = percentages[i] * 3.6f; // Convert percentage to degrees
            
            // Draw pie slice
            canvas.FillColor = colors[i];
            canvas.FillArc(centerX - radius, centerY - radius, 
                          radius * 2, radius * 2, 
                          startAngle, sweepAngle, true);
            
            // Calculate label position
            float midAngle = startAngle + (sweepAngle / 2);
            float textRadius = radius * 0.7f;
            
            // Convert angle to radians for correct math calculations
            double midAngleRadians = midAngle * Math.PI / 180;
            
            float textX = centerX + (float)(textRadius * Math.Cos(midAngleRadians));
            float textY = centerY + (float)(textRadius * Math.Sin(midAngleRadians));
            
            // Draw percentage label
            canvas.FontSize = 14;
            canvas.FontColor = Colors.White;
            canvas.DrawString($"{percentages[i]:F1}%", textX, textY, HorizontalAlignment.Center);
            
            startAngle += sweepAngle;
        }
        
        // Draw legend
        float legendX = 20;
        float legendY = dirtyRect.Height - 70;
        float legendSpacing = 22;
        
        for (int i = 0; i < labels.Length; i++)
        {
            canvas.FillColor = colors[i];
            canvas.FillRectangle(legendX, legendY + (i * legendSpacing), 12, 12);
            
            canvas.FontSize = 12;
            canvas.FontColor = Colors.Black;
            canvas.DrawString($"{labels[i]} ({values[i]})", legendX + 18, legendY + (i * legendSpacing) + 6, HorizontalAlignment.Left);
        }
    }
    
    private void DrawEmptyChart(ICanvas canvas, RectF dirtyRect)
    {
        // Draw empty chart with message
        float centerX = dirtyRect.Width / 2;
        float centerY = dirtyRect.Height / 2;
        
        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 2;
        canvas.DrawCircle(centerX, centerY, 100);
        
        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 14;
        canvas.DrawString("No task data available", centerX, centerY, HorizontalAlignment.Center);
        
        // Draw empty legend
        float legendX = 20;
        float legendY = dirtyRect.Height - 70;
        
        string[] labels = new[] { "Completed", "In Progress", "Failed" };
        Color[] colors = new[] { Colors.MediumSeaGreen, Colors.DodgerBlue, Colors.Crimson };
        
        for (int i = 0; i < labels.Length; i++)
        {
            canvas.FillColor = colors[i];
            canvas.FillRectangle(legendX, legendY + (i * 22), 12, 12);
            
            canvas.FontSize = 12;
            canvas.FontColor = Colors.Black;
            canvas.DrawString($"{labels[i]} (0)", legendX + 18, legendY + (i * 22) + 6, HorizontalAlignment.Left);
        }
    }
}