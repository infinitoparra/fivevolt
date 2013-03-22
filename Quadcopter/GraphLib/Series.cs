using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GraphLib
{
    public class Series
    {
        private readonly List<DataPoint> _dataPoints =  new List<DataPoint>();

        public Series()
        {
            Visible = true;
        }

        public IList<DataPoint> DataPoints { get { return _dataPoints; } }
        public string Title { get; set; }
        public bool Visible { get; set; }
        public Color Color { get; set; }
        public double XMinimum { get { return _dataPoints.Count == 0 ? 0.0 : _dataPoints.Min(dp => dp.XValue); } }
        public double XMaximum { get { return _dataPoints.Count == 0 ? 0.0 : _dataPoints.Max(dp => dp.XValue); } }
        public double YMinimum { get { return _dataPoints.Count == 0 ? 0.0 : _dataPoints.Min(dp => dp.YValue); } }
        public double YMaximum { get { return _dataPoints.Count == 0 ? 0.0 : _dataPoints.Max(dp => dp.YValue); } }
    }
}
