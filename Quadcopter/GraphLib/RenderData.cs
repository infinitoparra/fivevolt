using System;
using System.Drawing;

namespace GraphLib
{
    public class RenderData : IDisposable
    {
        public Graph Graph { get; set; }
        public Graphics g { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Pen AxisPen { get; set; }
        public Font AxisFont { get; set; }
        public Brush AxisBrush { get; set; }
        public Rectangle AxisXRect { get; set; }
        public Rectangle AxisYRect { get; set; }
        public Rectangle GraphRect { get; set; }
        public Rectangle LegendRect { get; set; }
        public string AxisStringFormat { get; set; }
        public int LargeTickLength { get; set; }
        public Pen[] SeriesPens { get; set; }
        public double XMin { get; set; }
        public double XMax { get; set; }
        public double XInterval { get; set; }
        public double XPixelInterval { get; set; }
        public double XPixelsPerValue { get; set; }
        public double YMin { get; set; }
        public double YMax { get; set; }
        public double YInterval { get; set; }
        public double YPixelInterval { get; set; }
        public double YPixelsPerValue { get; set; }
        public Font LegendFont { get; set; }
        public Brush LegendBrush { get; set; }
        public SizeF LegendFontMetrics { get; set; }

        public void Dispose()
        {
            AxisPen.Dispose();
            AxisFont.Dispose();
            AxisBrush.Dispose();
            LegendFont.Dispose();
            LegendBrush.Dispose();
            foreach (Pen pen in SeriesPens)
            {
                pen.Dispose();
            }
        }
    }
}
