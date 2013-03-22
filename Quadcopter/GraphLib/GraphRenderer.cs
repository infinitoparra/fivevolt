using System;
using System.Drawing;
using System.Linq;

namespace GraphLib
{
    public class GraphRenderer
    {
        private RenderData _lastData;

        public void Render(Graph graph, Graphics g, int width, int height)
        {
            g.Clear(Color.White);
            using (RenderData data = CreateRenderData(graph, g, width, height))
            {
                RenderYAxis(data);
                RenderXAxis(data);
                RenderGraphData(data);
                RenderLegend(data);
                _lastData = data;
            }
        }

        public RenderData CreateRenderData(Graph graph, Graphics g, int width, int height)
        {
            RenderData data = new RenderData
            {
                Graph = graph,
                g = g,
                Width = width,
                Height = height,
                AxisPen = new Pen(Color.Black),
                AxisFont = new Font(FontFamily.GenericSansSerif, 10.0f),
                AxisBrush = new SolidBrush(Color.Black),
                AxisStringFormat = "#,##0.##",
                LegendFont = new Font(FontFamily.GenericSansSerif, 10.0f),
                LegendBrush = new SolidBrush(Color.Black),
                LargeTickLength = 10,
                SeriesPens = graph.Series.Select(s => new Pen(s.Color)).ToArray()
            };

            data.LegendFontMetrics = g.MeasureString("Title of a Series", data.LegendFont);
            int legendWidth = (int)(data.LegendFontMetrics.Width + 20);
            data.LegendRect = new Rectangle(width - legendWidth, 0, legendWidth, height);

            SizeF axisFontMetrics = g.MeasureString(graph.ActualYMaximum.ToString(data.AxisStringFormat), data.AxisFont);
            data.AxisYRect = new Rectangle(0, 0, (int)(axisFontMetrics.Width + data.LargeTickLength), (int)(height - axisFontMetrics.Height - data.LargeTickLength));
            data.AxisXRect = new Rectangle(data.AxisYRect.Right, data.AxisYRect.Bottom, width - data.AxisYRect.Left - data.LegendRect.Width, (int)(axisFontMetrics.Height + data.LargeTickLength));
            data.GraphRect = new Rectangle(data.AxisYRect.Right, data.AxisYRect.Top, width - data.AxisYRect.Width - data.LegendRect.Width,
                                           height - data.AxisYRect.Top - data.AxisXRect.Height);

            data.XMin = data.Graph.ActualXMinimum;
            data.XMax = data.Graph.ActualXMaximum;
            data.XInterval = data.Graph.ActualXInterval;
            if (Math.Abs(data.XMax - data.XMin) < 0.0001)
            {
                data.XPixelInterval = width / 10.0;
                data.XPixelsPerValue = 0.0;
            }
            else
            {
                data.XPixelInterval = Math.Max(1.0, data.GraphRect.Width / ((data.XMax - data.XMin) / data.XInterval));
                data.XPixelsPerValue = data.GraphRect.Width / (data.XMax - data.XMin);
            }

            data.YMin = data.Graph.ActualYMinimum;
            data.YMax = data.Graph.ActualYMaximum;
            data.YInterval = data.Graph.ActualYInterval;
            if (Math.Abs(data.YMax - data.YMin) < 0.0001)
            {
                data.YPixelInterval = height / 10.0;
                data.YPixelsPerValue = 0.0;
            }
            else
            {
                data.YPixelInterval = Math.Max(1.0, data.GraphRect.Height / ((data.YMax - data.YMin) / data.YInterval));
                data.YPixelsPerValue = data.GraphRect.Height / (data.YMax - data.YMin);
            }

            return data;
        }

        public void RenderYAxis(RenderData data)
        {
            Graphics g = data.g;
            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center
            };
            using (stringFormat)
            {
                g.DrawLine(data.AxisPen, data.AxisYRect.Right, data.AxisYRect.Top, data.AxisYRect.Right, data.AxisYRect.Bottom);
                double yVal = data.YMin;
                for (double y = data.AxisYRect.Bottom; y >= data.AxisYRect.Top; y -= data.YPixelInterval)
                {
                    g.DrawLine(data.AxisPen, data.AxisYRect.Right, (int)y, data.AxisYRect.Right - data.LargeTickLength, (int)y);
                    if (data.YPixelsPerValue != 0.0)
                    {
                        g.DrawString(yVal.ToString(data.AxisStringFormat), data.AxisFont, data.AxisBrush,
                                     new PointF(data.AxisYRect.Right - data.LargeTickLength, (float)y), stringFormat);
                    }
                    yVal += data.YInterval;
                }
            }
        }

        public void RenderXAxis(RenderData data)
        {
            Graphics g = data.g;
            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };
            using (stringFormat)
            {
                g.DrawLine(data.AxisPen, data.AxisXRect.Left, data.AxisXRect.Top, data.AxisXRect.Right, data.AxisXRect.Top);
                double xVal = data.XMin;
                for (double x = data.AxisXRect.Left; x <= data.AxisXRect.Right; x += data.XPixelInterval)
                {
                    g.DrawLine(data.AxisPen, (int)x, data.AxisXRect.Top, (int)x, data.AxisXRect.Top + data.LargeTickLength);
                    if (data.XPixelsPerValue != 0.0)
                    {
                        g.DrawString(xVal.ToString(data.AxisStringFormat), data.AxisFont, data.AxisBrush,
                                     new PointF((float)x, data.AxisXRect.Top + data.LargeTickLength), stringFormat);
                    }
                    xVal += data.XInterval;
                }
            }
        }

        private void RenderGraphData(RenderData data)
        {
            Graphics g = data.g;

            int seriesCount = 0;
            foreach (Series series in data.Graph.Series)
            {
                if (series.Visible && series.DataPoints.Count > 0)
                {
                    Point lastPoint = ToPixels(data, series.DataPoints.First());
                    foreach (DataPoint dp in series.DataPoints)
                    {
                        Point pt = ToPixels(data, dp);
                        g.DrawLine(data.SeriesPens[seriesCount], lastPoint, pt);
                        lastPoint = pt;
                    }
                }
                seriesCount++;
            }
        }

        private void RenderLegend(RenderData data)
        {
            Graphics g = data.g;

            int seriesCount = 0;
            int y = 0;
            foreach (Series series in data.Graph.Series)
            {
                if (series.Visible)
                {
                    g.DrawLine(data.SeriesPens[seriesCount], data.LegendRect.Left, y + (data.LegendFontMetrics.Height / 2), data.LegendRect.Left + 20, y + (data.LegendFontMetrics.Height / 2));
                    g.DrawString(data.Graph.Series[seriesCount].Title, data.LegendFont, data.LegendBrush, data.LegendRect.Left + 22, y);
                    y += (int)(data.LegendFontMetrics.Height + 5);
                }
                seriesCount++;
            }
        }

        private Point ToPixels(RenderData data, DataPoint dp)
        {
            double x = data.GraphRect.Left + ((dp.XValue - data.XMin) * data.XPixelsPerValue);
            double y = data.GraphRect.Bottom - ((dp.YValue - data.YMin) * data.YPixelsPerValue);
            return new Point((int)x, (int)y);
        }

        public PointF PixelToGraphCoordinates(Point pt)
        {
            double x = ((pt.X - _lastData.GraphRect.Left) / _lastData.XPixelsPerValue) + _lastData.XMin;
            double y = ((_lastData.GraphRect.Bottom - pt.Y) / _lastData.YPixelsPerValue) + _lastData.YMin;
            return new PointF((float)x, (float)y);
        }
    }
}
