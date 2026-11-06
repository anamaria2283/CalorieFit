using Microsoft.Maui.Graphics;

namespace CalorieFit.Views;

public sealed class MacroPieChartDrawable : IDrawable
{
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
    public double FatGrams { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var total = ProteinGrams + CarbsGrams + FatGrams;
        var centerX = dirtyRect.Center.X;
        var centerY = dirtyRect.Center.Y;
        var diameter = Math.Min(dirtyRect.Width, dirtyRect.Height) - 30;
        var left = centerX - diameter / 2;
        var top = centerY - diameter / 2;

        canvas.FontSize = 14;
        canvas.FontColor = Colors.Gray;

        if (total <= 0)
        {
            canvas.FillColor = Color.FromArgb("#E0E0E0");
            canvas.FillCircle(centerX, centerY, diameter / 2);
            canvas.FontColor = Color.FromArgb("#616161");
            canvas.DrawString("Fără date", left, centerY - 12, diameter, 24, HorizontalAlignment.Center, VerticalAlignment.Center);
            return;
        }

        var values = new[] { ProteinGrams, CarbsGrams, FatGrams };
        var colors = new[]
        {
            Color.FromArgb("#43A047"),
            Color.FromArgb("#FB8C00"),
            Color.FromArgb("#D81B60")
        };
        var angle = -90f;

        for (var index = 0; index < values.Length; index++)
        {
            var percentage = values[index] / total;
            var sweep = (float)(percentage * 360);
            if (sweep <= 0)
            {
                continue;
            }

            canvas.FillColor = colors[index];
            var startRadians = angle * Math.PI / 180;
            var endRadians = (angle + sweep) * Math.PI / 180;
            var sector = new PathF();
            sector.MoveTo(centerX, centerY);
            sector.LineTo(
                centerX + (float)Math.Cos(startRadians) * diameter / 2,
                centerY + (float)Math.Sin(startRadians) * diameter / 2);

            var segmentCount = Math.Max(2, (int)Math.Ceiling(sweep / 6));
            for (var segment = 1; segment <= segmentCount; segment++)
            {
                var radians = startRadians + (endRadians - startRadians) * segment / segmentCount;
                sector.LineTo(
                    centerX + (float)Math.Cos(radians) * diameter / 2,
                    centerY + (float)Math.Sin(radians) * diameter / 2);
            }

            sector.Close();
            canvas.FillPath(sector);

            var labelAngle = (angle + sweep / 2) * Math.PI / 180;
            var labelRadius = diameter * 0.38;
            var labelX = centerX + (float)Math.Cos(labelAngle) * (float)labelRadius - 28;
            var labelY = centerY + (float)Math.Sin(labelAngle) * (float)labelRadius - 10;
            canvas.FontColor = Color.FromArgb("#263238");
            canvas.FontSize = 13;
            canvas.DrawString($"{percentage:P0}", labelX, labelY, 56, 20, HorizontalAlignment.Center, VerticalAlignment.Center);
            angle += sweep;
        }

        canvas.FillColor = Colors.White;
        canvas.FillCircle(centerX, centerY, diameter * 0.19f);
        canvas.FontColor = Color.FromArgb("#263238");
        canvas.FontSize = 13;
        canvas.DrawString($"{total:F0} g", centerX - 40, centerY - 10, 80, 20, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}

public sealed class ProteinTrendDrawable : IDrawable
{
    public IReadOnlyList<ProteinDay> Values { get; set; } = Array.Empty<ProteinDay>();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        const float left = 42;
        const float right = 12;
        const float top = 18;
        const float bottom = 34;
        var chartWidth = Math.Max(1f, dirtyRect.Width - left - right);
        var chartHeight = Math.Max(1f, dirtyRect.Height - top - bottom);
        var maxValue = Math.Max(10, Values.Count == 0 ? 0 : Values.Max(value => value.Grams));

        canvas.StrokeColor = Color.FromArgb("#CFD8DC");
        canvas.StrokeSize = 1;
        canvas.DrawLine(left, top, left, top + chartHeight);
        canvas.DrawLine(left, top + chartHeight, left + chartWidth, top + chartHeight);

        if (Values.Count == 0)
        {
            canvas.FontColor = Colors.Gray;
            canvas.DrawString("Fără date pentru perioada aleasă", 0, top + chartHeight / 2 - 12, dirtyRect.Width, 24, HorizontalAlignment.Center, VerticalAlignment.Center);
            return;
        }

        canvas.FontColor = Color.FromArgb("#607D8B");
        canvas.FontSize = 11;
        canvas.DrawString($"{maxValue:F0} g", 0, top - 8, left - 5, 20, HorizontalAlignment.Right, VerticalAlignment.Center);
        canvas.DrawString("0 g", 0, top + chartHeight - 8, left - 5, 20, HorizontalAlignment.Right, VerticalAlignment.Center);

        var points = new List<PointF>();
        for (var index = 0; index < Values.Count; index++)
        {
            var x = Values.Count == 1
                ? left + chartWidth / 2
                : left + chartWidth * index / (Values.Count - 1);
            var y = top + chartHeight - (float)(Values[index].Grams / maxValue * chartHeight);
            points.Add(new PointF(x, y));

            canvas.FillColor = Color.FromArgb("#1976D2");
            canvas.FillCircle(x, y, 4);
            canvas.FontColor = Color.FromArgb("#37474F");
            canvas.FontSize = 10;
            canvas.DrawString(Values[index].Date.ToString("dd.MM"), x - 24, top + chartHeight + 8, 48, 18, HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        if (points.Count > 1)
        {
            canvas.StrokeColor = Color.FromArgb("#1976D2");
            canvas.StrokeSize = 3;
            for (var index = 1; index < points.Count; index++)
            {
                canvas.DrawLine(points[index - 1], points[index]);
            }
        }
    }
}

public sealed record ProteinDay(DateTime Date, double Grams);
