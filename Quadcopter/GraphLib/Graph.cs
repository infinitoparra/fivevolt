using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Point = System.Windows.Point;

namespace GraphLib
{
    public class Graph
    {
        public static readonly Color[] DefaultColors = new[] {
            Color.FromArgb(65,111,166),
            Color.FromArgb(168,66,63),
            Color.FromArgb(134,164,74),
            Color.FromArgb(110,84,141),
            Color.FromArgb(61,150,174),
            Color.FromArgb(218,129,55),
            Color.FromArgb(142,165,203),
            Color.FromArgb(206,142,141),
            Color.FromArgb(181,202,146),
            Color.FromArgb(255,0,0),
            Color.FromArgb(0,255,0),
            Color.FromArgb(0,0,255)
        };

        private readonly List<Series> _series = new List<Series>();
        private readonly object _lock = new object();

        public object Lock { get { return _lock; } }

        public IList<Series> Series { get { return _series; } }
        public IEnumerable<Series> VisibleSeries { get { return _series.Where(s => s.Visible); } }

        public double? XMinimum { get; set; }
        public double? XMaximum { get; set; }
        public double? XInterval { get; set; }

        public double? YMinimum { get; set; }
        public double? YMaximum { get; set; }
        public double? YInterval { get; set; }

        public double ActualXMinimum
        {
            get
            {
                if (XMinimum != null)
                {
                    return XMinimum.Value;
                }
                if (VisibleSeries.Count() == 0)
                {
                    return 0.0;
                }
                return Series.Min(s => s.XMinimum);
            }
        }
        public double ActualXMaximum
        {
            get
            {
                if (XMaximum != null)
                {
                    return XMaximum.Value;
                }
                if (VisibleSeries.Count() == 0)
                {
                    return 0.0;
                }
                return Series.Max(s => s.XMaximum);
            }
        }
        public double ActualXInterval { get { return XInterval != null ? XInterval.Value : (ActualXMaximum - ActualXMinimum) / 10.0; } }

        public double ActualYMinimum
        {
            get
            {
                if (YMinimum != null)
                {
                    return YMinimum.Value;
                }
                if (VisibleSeries.Count() == 0)
                {
                    return 0.0;
                }
                return Series.Min(s => s.YMinimum);
            }
        }
        public double ActualYMaximum
        {
            get
            {
                if (YMaximum != null)
                {
                    return YMaximum.Value;
                }
                if (VisibleSeries.Count() == 0)
                {
                    return 0.0;
                }
                return Series.Max(s => s.YMaximum);
            }
        }
        public double ActualYInterval { get { return YInterval != null ? YInterval.Value : (ActualYMaximum - ActualYMinimum) / 10.0; } }
    }
}
