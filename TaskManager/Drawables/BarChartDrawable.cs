using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;

public class BarChartDrawable : IDrawable
{
    private List<TimeIntervalData> _timeIntervalData;
    private DateTime _lastRefreshTime;
    private bool _dataLoaded = false;

    // Constructor
    public BarChartDrawable()
    {
        _timeIntervalData = new List<TimeIntervalData>();
        _lastRefreshTime = DateTime.Now;
    }

    // Method to update data from database
    public void UpdateData(List<TimeIntervalData> data)
    {
        _timeIntervalData = data ?? new List<TimeIntervalData>();
        _dataLoaded = true;
        _lastRefreshTime = DateTime.Now;
    }

    // Method to check if refresh is needed (15-minute intervals)
    public bool NeedsRefresh()
    {
        return (DateTime.Now - _lastRefreshTime).TotalMinutes >= 15;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (!_dataLoaded || _timeIntervalData.Count == 0)
        {
            DrawEmptyChart(canvas, dirtyRect);
            return;
        }

        // Chart dimensions and padding
        float padding = 50;
        float bottomPadding = 70; // Extra space for labels
        float chartWidth = dirtyRect.Width - (padding * 2);
        float chartHeight = dirtyRect.Height - padding - bottomPadding;

        // Chart area
        float chartLeft = padding;
        float chartTop = padding;
        float chartBottom = chartTop + chartHeight;

        // Draw title
        canvas.FontSize = 16;
        canvas.FontColor = Colors.Black;
        canvas.DrawString("Task Status (Last Hour)", dirtyRect.Width / 2, 20, HorizontalAlignment.Center);

        // Find max value for scaling
        int maxCount = 10; // Minimum scale
        foreach (var data in _timeIntervalData)
        {
            maxCount = Math.Max(maxCount, Math.Max(data.SuccessCount, data.FailedCount));
        }
        
        // Round up max value to nearest 5 or 10 for nicer scale
        maxCount = (int)Math.Ceiling(maxCount / 10.0) * 10;

        // Draw Y-axis and grid lines
        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 1;
        canvas.DrawLine(chartLeft, chartTop, chartLeft, chartBottom);

        // Y-axis labels and grid lines
        int yLabelCount = 5; // Number of horizontal grid lines
        canvas.FontSize = 10;
        for (int i = 0; i <= yLabelCount; i++)
        {
            float y = chartBottom - (i * (chartHeight / yLabelCount));
            int value = (i * maxCount / yLabelCount);
            
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

        // Calculate bar width based on intervals
        float intervalWidth = chartWidth / _timeIntervalData.Count;
        float barWidth = intervalWidth * 0.4f; // 40% of interval width
        float groupPadding = intervalWidth * 0.2f; // 20% padding between groups

        // Draw bars and X-axis labels
        for (int i = 0; i < _timeIntervalData.Count; i++)
        {
            var data = _timeIntervalData[i];
            float x = chartLeft + (i * intervalWidth) + (intervalWidth / 2) - barWidth;

            // Success bar
            canvas.FillColor = Colors.MediumSeaGreen;
            float successHeight = (float)data.SuccessCount / maxCount * chartHeight;
            canvas.FillRectangle(x - barWidth / 2, chartBottom - successHeight, barWidth, successHeight);

            // Failed bar
            canvas.FillColor = Colors.Crimson;
            float failedHeight = (float)data.FailedCount / maxCount * chartHeight;
            canvas.FillRectangle(x + barWidth / 2, chartBottom - failedHeight, barWidth, failedHeight);

            // Time interval label
            canvas.FontColor = Colors.Black;
            canvas.FontSize = 11;
            string timeLabel = data.TimeInterval.ToString("HH:mm");
            canvas.DrawString(timeLabel, chartLeft + (i * intervalWidth) + (intervalWidth / 2), 
                             chartBottom + 15, HorizontalAlignment.Center);
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

    private void DrawLegend(ICanvas canvas, RectF dirtyRect)
    {
        float legendY = dirtyRect.Height - 40;
        float legendX1 = dirtyRect.Width / 2 - 80;
        float legendX2 = dirtyRect.Width / 2 + 20;

        // Success legend
        canvas.FillColor = Colors.MediumSeaGreen;
        canvas.FillRectangle(legendX1, legendY, 12, 12);
        canvas.FontColor = Colors.Black;
        canvas.FontSize = 12;
        canvas.DrawString("Success", legendX1 + 18, legendY + 6, HorizontalAlignment.Left);

        // Failed legend
        canvas.FillColor = Colors.Crimson;
        canvas.FillRectangle(legendX2, legendY, 12, 12);
        canvas.FontColor = Colors.Black;
        canvas.FontSize = 12;
        canvas.DrawString("Failed", legendX2 + 18, legendY + 6, HorizontalAlignment.Left);
    }

    private void DrawEmptyChart(ICanvas canvas, RectF dirtyRect)
    {
        // Draw empty chart with message
        float centerX = dirtyRect.Width / 2;
        float centerY = dirtyRect.Height / 2;

        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 14;
        canvas.DrawString("No task data available for the last hour", 
                          centerX, centerY, HorizontalAlignment.Center);

        // Draw title
        canvas.FontSize = 16;
        canvas.FontColor = Colors.Black;
        canvas.DrawString("Task Status (Last Hour)", dirtyRect.Width / 2, 20, HorizontalAlignment.Center);
    }
}

// Class to hold time interval data
public class TimeIntervalData
{
    public DateTime TimeInterval { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
}