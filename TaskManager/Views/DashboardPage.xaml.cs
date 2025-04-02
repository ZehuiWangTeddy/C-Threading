using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;

namespace TaskManager.Views
{
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage()
        {
            InitializeComponent();
            
            // Initialize chart drawables
            BarChartView.Drawable = new BarChartDrawable();
            PieChartView.Drawable = new PieChartDrawable();
            LineChartView.Drawable = new LineChartDrawable();
        }

        private async void OnTaskListClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TaskListPage());
        }

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
                    // Debugging: Check if the button has a BindingContext
                    await DisplayAlert("Error", "No task selected.", "OK");
                }
            }
        }
    }

    // Bar Chart Implementation
    public class BarChartDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Data for completed tasks
            int[] completedData = new int[] { 10, 15, 8, 20, 25, 12 };
            // Data for failed tasks
            int[] failedData = new int[] { 5, 8, 4, 7, 10, 6 };
            string[] months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
            
            float chartMargin = 20;
            float chartHeight = dirtyRect.Height - (2 * chartMargin);
            float chartWidth = dirtyRect.Width - (2 * chartMargin);
            
            // Calculate max value for scaling
            int maxValue = 0;
            for (int i = 0; i < completedData.Length; i++)
            {
                int total = completedData[i] + failedData[i];
                if (total > maxValue)
                    maxValue = total;
            }
            
            // Scale factor
            float yScale = chartHeight / (maxValue * 1.1f);
            
            // X-axis labels and ticks
            canvas.StrokeColor = Colors.Black;
            canvas.DrawLine(chartMargin, dirtyRect.Height - chartMargin, 
                           dirtyRect.Width - chartMargin, dirtyRect.Height - chartMargin);
            
            float barWidth = chartWidth / (completedData.Length * 2);
            float barSpacing = barWidth / 2;
            
            // Draw bars and labels
            for (int i = 0; i < completedData.Length; i++)
            {
                float x = chartMargin + (i * (barWidth * 2 + barSpacing));
                
                // Draw month label
                canvas.FontSize = 10;
                canvas.DrawString(months[i], x, dirtyRect.Height - chartMargin + 15, HorizontalAlignment.Center);
                
                // Draw completed task bar
                canvas.FillColor = Colors.DodgerBlue;
                float completedHeight = completedData[i] * yScale;
                canvas.FillRectangle(x, dirtyRect.Height - chartMargin - completedHeight, 
                                    barWidth, completedHeight);
                
                // Draw failed task bar
                canvas.FillColor = Colors.OrangeRed;
                float failedHeight = failedData[i] * yScale;
                canvas.FillRectangle(x + barWidth, dirtyRect.Height - chartMargin - failedHeight, 
                                    barWidth, failedHeight);
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
    }

    // Pie Chart Implementation
    public class PieChartDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Data (in percentages)
            float[] data = new float[] { 50, 25, 25 };
            string[] labels = new string[] { "Completed", "In Progress", "Failed" };
            Color[] colors = new Color[] { Colors.MediumSeaGreen, Colors.Blue, Colors.Crimson };
            
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
                canvas.DrawString($"{data[i]}%", textX, textY, HorizontalAlignment.Right);
                
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
                canvas.DrawString(labels[i], legendX + 15, legendY + (i * 20) + 5, HorizontalAlignment.Left);
            }
        }
    }

    // Line Chart Implementation
    public class LineChartDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Data
            double[] data = new double[] { 8, 12, 10, 15, 18, 14 };
            string[] weeks = new string[] { "Week 1", "Week 2", "Week 3", "Week 4", "Week 5", "Week 6" };
            
            float chartMargin = 20;
            float chartHeight = dirtyRect.Height - (2 * chartMargin);
            float chartWidth = dirtyRect.Width - (2 * chartMargin);
            
            // Calculate max value for scaling
            double maxValue = 0;
            foreach (double value in data)
            {
                if (value > maxValue)
                    maxValue = value;
            }
            
            // Scale factor
            float yScale = chartHeight / ((float)maxValue * 1.1f);
            
            // Draw X and Y axes
            canvas.StrokeColor = Colors.Gray;
            canvas.DrawLine(chartMargin, chartMargin, 
                           chartMargin, dirtyRect.Height - chartMargin);
            canvas.DrawLine(chartMargin, dirtyRect.Height - chartMargin, 
                           dirtyRect.Width - chartMargin, dirtyRect.Height - chartMargin);
            
            // Calculate point spacing
            float pointSpacing = chartWidth / (data.Length - 1);
            
            // Draw connecting lines
            PathF path = new PathF();
            
            for (int i = 0; i < data.Length; i++)
            {
                float x = chartMargin + (i * pointSpacing);
                float y = dirtyRect.Height - chartMargin - ((float)data[i] * yScale);
                
                if (i == 0)
                    path.MoveTo(x, y);
                else
                    path.LineTo(x, y);
                
                // Draw X-axis labels
                canvas.FontSize = 10;
                canvas.FontColor = Colors.Black;
                canvas.DrawString(weeks[i], x, dirtyRect.Height - chartMargin + 15, HorizontalAlignment.Center);
            }
            
            // Draw line
            canvas.StrokeColor = Colors.DodgerBlue;
            canvas.StrokeSize = 2;
            canvas.DrawPath(path);
            
            // Draw points
            for (int i = 0; i < data.Length; i++)
            {
                float x = chartMargin + (i * pointSpacing);
                float y = dirtyRect.Height - chartMargin - ((float)data[i] * yScale);
                
                canvas.FillColor = Colors.White;
                canvas.StrokeColor = Colors.DodgerBlue;
                canvas.StrokeSize = 2;
                canvas.FillCircle(x, y, 5);
                canvas.DrawCircle(x, y, 5);
                
                // Draw value labels
                canvas.FontSize = 10;
                canvas.FontColor = Colors.Black;
                canvas.DrawString(data[i].ToString(), x, y - 15, HorizontalAlignment.Center);
            }
        }
    }
}