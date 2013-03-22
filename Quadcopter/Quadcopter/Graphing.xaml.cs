using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GraphLib;
using Point = System.Windows.Point;

namespace Quadcopter
{
    public partial class Graphing : UserControl
    {
        public static readonly DependencyProperty SeriesIndex = DependencyProperty.Register("SeriesIndex", typeof(int), typeof(Graphing), null);

        private const int SERIES_TELEM_ROLL = 0;
        private const int SERIES_TELEM_PITCH = 1;
        private const int SERIES_TELEM_YAW = 2;
        private const int SERIES_TELEM_MOTORFRONT = 3;
        private const int SERIES_TELEM_MOTORBACK = 4;
        private const int SERIES_TELEM_MOTORLEFT = 5;
        private const int SERIES_TELEM_MOTORRIGHT = 6;
        private const int SERIES_TELEM_USERROLL = 7;
        private const int SERIES_TELEM_USERPITCH = 8;
        private const int SERIES_TELEM_USERYAW = 9;
        private const int SERIES_TELEM_USERTHROTTLE = 10;
        private const int SERIES_IMU_GX = 11;
        private const int SERIES_IMU_GY = 12;
        private const int SERIES_IMU_GZ = 13;
        private const int SERIES_IMU_ACCEL_ROLL = 14;
        private const int SERIES_IMU_ACCEL_PITCH = 15;
        private const int SERIES_IMU_COMPASS_YAW = 16;
        private const int SERIES_IMU_ROLL = 17;
        private const int SERIES_IMU_PITCH = 18;
        private const int SERIES_IMU_YAW = 19;
        private const int SERIES_MAX = 20;

        private ComLink _comLink;
        private DateTime _lastRedraw;
        private readonly DateTime _startTime;
        private double _lastSecondsToGraph;
        private bool _paused;

        public Graphing()
        {
            Settings.Instance.SuspendSave();
            try
            {
                InitializeComponent();
                _startTime = DateTime.Now;
                _graph.Graph = new Graph();

                Settings.Instance.EnsureSeriesToGraphSize(SERIES_MAX);
                _graph.Graph.Series.Add(new Series { Title = "Roll", Visible = Settings.Instance.SeriesToGraph[SERIES_TELEM_ROLL], Color = Graph.DefaultColors[0] });
                _graph.Graph.Series.Add(new Series { Title = "Pitch", Visible = Settings.Instance.SeriesToGraph[SERIES_TELEM_PITCH], Color = Graph.DefaultColors[1] });
                _graph.Graph.Series.Add(new Series { Title = "Yaw", Visible = Settings.Instance.SeriesToGraph[SERIES_TELEM_YAW], Color = Graph.DefaultColors[2] });
                _graph.Graph.Series.Add(new Series { Title = "Motor Front", Visible = Settings.Instance.SeriesToGraph[SERIES_TELEM_MOTORFRONT], Color = Graph.DefaultColors[3] });
                _graph.Graph.Series.Add(new Series { Title = "Motor Back", Visible = Settings.Instance.SeriesToGraph[SERIES_TELEM_MOTORBACK], Color = Graph.DefaultColors[4] });
                _graph.Graph.Series.Add(new Series { Title = "Motor Left", Visible = Settings.Instance.SeriesToGraph[SERIES_TELEM_MOTORLEFT], Color = Graph.DefaultColors[5] });
                _graph.Graph.Series.Add(new Series { Title = "Motor Right", Visible = Settings.Instance.SeriesToGraph[SERIES_TELEM_MOTORRIGHT], Color = Graph.DefaultColors[6] });
                _graph.Graph.Series.Add(new Series { Title = "User Roll", Visible = Settings.Instance.SeriesToGraph[SERIES_TELEM_USERROLL], Color = Graph.DefaultColors[7] });
                _graph.Graph.Series.Add(new Series { Title = "User Pitch", Visible = Settings.Instance.SeriesToGraph[SERIES_TELEM_USERPITCH], Color = Graph.DefaultColors[8] });
                _graph.Graph.Series.Add(new Series { Title = "User Yaw", Visible = Settings.Instance.SeriesToGraph[SERIES_TELEM_USERYAW], Color = Graph.DefaultColors[9] });
                _graph.Graph.Series.Add(new Series { Title = "User Throttle", Visible = Settings.Instance.SeriesToGraph[SERIES_TELEM_USERTHROTTLE], Color = Graph.DefaultColors[10] });

                _graph.Graph.Series.Add(new Series { Title = "IMU Gx", Visible = Settings.Instance.SeriesToGraph[SERIES_IMU_GX], Color = Graph.DefaultColors[0] });
                _graph.Graph.Series.Add(new Series { Title = "IMU Gy", Visible = Settings.Instance.SeriesToGraph[SERIES_IMU_GY], Color = Graph.DefaultColors[1] });
                _graph.Graph.Series.Add(new Series { Title = "IMU Gz", Visible = Settings.Instance.SeriesToGraph[SERIES_IMU_GZ], Color = Graph.DefaultColors[2] });
                _graph.Graph.Series.Add(new Series { Title = "IMU Accel Roll", Visible = Settings.Instance.SeriesToGraph[SERIES_IMU_ACCEL_ROLL], Color = Graph.DefaultColors[3] });
                _graph.Graph.Series.Add(new Series { Title = "IMU Accel Pitch", Visible = Settings.Instance.SeriesToGraph[SERIES_IMU_ACCEL_PITCH], Color = Graph.DefaultColors[4] });
                _graph.Graph.Series.Add(new Series { Title = "IMU Compass Yaw", Visible = Settings.Instance.SeriesToGraph[SERIES_IMU_COMPASS_YAW], Color = Graph.DefaultColors[5] });
                _graph.Graph.Series.Add(new Series { Title = "IMU Roll", Visible = Settings.Instance.SeriesToGraph[SERIES_IMU_ROLL], Color = Graph.DefaultColors[6] });
                _graph.Graph.Series.Add(new Series { Title = "IMU Pitch", Visible = Settings.Instance.SeriesToGraph[SERIES_IMU_PITCH], Color = Graph.DefaultColors[7] });
                _graph.Graph.Series.Add(new Series { Title = "IMU Yaw", Visible = Settings.Instance.SeriesToGraph[SERIES_IMU_YAW], Color = Graph.DefaultColors[8] });

                for (int i = 0; i < SERIES_MAX; i++)
                {
                    CheckBox cb = new CheckBox
                    {
                        Content = _graph.Graph.Series[i].Title,
                        IsChecked = Settings.Instance.SeriesToGraph[i]
                    };
                    cb.Click += Series_CheckBox_Click;
                    cb.SetValue(SeriesIndex, i);
                    _series.Children.Add(cb);
                }

                _secondsToGraph.Value = Settings.Instance.SecondsToGraph;
            }
            finally
            {
                Settings.Instance.ResumeSave();
            }
        }

        public ComLink ComLink
        {
            get
            {
                return _comLink;
            }
            set
            {
                _comLink = value;
                _comLink.TelemetryArrived += _comLink_TelemetryArrived;
                _comLink.ImuArrived += _comLink_ImuArrived;
            }
        }

        public void _comLink_ImuArrived(ImuPacket data)
        {
            UpdateGraph(x =>
            {
                _graph.Graph.Series[SERIES_IMU_GX].DataPoints.Add(new DataPoint { XValue = x, YValue = data.Gx });
                _graph.Graph.Series[SERIES_IMU_GY].DataPoints.Add(new DataPoint { XValue = x, YValue = data.Gy });
                _graph.Graph.Series[SERIES_IMU_GZ].DataPoints.Add(new DataPoint { XValue = x, YValue = data.Gz });
                _graph.Graph.Series[SERIES_IMU_ACCEL_ROLL].DataPoints.Add(new DataPoint { XValue = x, YValue = data.AccelRoll });
                _graph.Graph.Series[SERIES_IMU_ACCEL_PITCH].DataPoints.Add(new DataPoint { XValue = x, YValue = data.AccelPitch });
                _graph.Graph.Series[SERIES_IMU_COMPASS_YAW].DataPoints.Add(new DataPoint { XValue = x, YValue = data.CompassYaw });
                _graph.Graph.Series[SERIES_IMU_ROLL].DataPoints.Add(new DataPoint { XValue = x, YValue = data.Roll });
                _graph.Graph.Series[SERIES_IMU_PITCH].DataPoints.Add(new DataPoint { XValue = x, YValue = data.Pitch });
                _graph.Graph.Series[SERIES_IMU_YAW].DataPoints.Add(new DataPoint { XValue = x, YValue = data.Yaw });
            });
        }

        private void _comLink_TelemetryArrived(Telemetry telemetry)
        {
            UpdateGraph(x =>
            {
                _graph.Graph.Series[SERIES_TELEM_ROLL].DataPoints.Add(new DataPoint { XValue = x, YValue = telemetry.Roll });
                _graph.Graph.Series[SERIES_TELEM_PITCH].DataPoints.Add(new DataPoint { XValue = x, YValue = telemetry.Pitch });
                _graph.Graph.Series[SERIES_TELEM_YAW].DataPoints.Add(new DataPoint { XValue = x, YValue = telemetry.Yaw });
                _graph.Graph.Series[SERIES_TELEM_MOTORFRONT].DataPoints.Add(new DataPoint { XValue = x, YValue = telemetry.MotorFront });
                _graph.Graph.Series[SERIES_TELEM_MOTORBACK].DataPoints.Add(new DataPoint { XValue = x, YValue = telemetry.MotorBack });
                _graph.Graph.Series[SERIES_TELEM_MOTORLEFT].DataPoints.Add(new DataPoint { XValue = x, YValue = telemetry.MotorLeft });
                _graph.Graph.Series[SERIES_TELEM_MOTORRIGHT].DataPoints.Add(new DataPoint { XValue = x, YValue = telemetry.MotorRight });
                _graph.Graph.Series[SERIES_TELEM_USERROLL].DataPoints.Add(new DataPoint { XValue = x, YValue = telemetry.UserRoll });
                _graph.Graph.Series[SERIES_TELEM_USERPITCH].DataPoints.Add(new DataPoint { XValue = x, YValue = telemetry.UserPitch });
                _graph.Graph.Series[SERIES_TELEM_USERYAW].DataPoints.Add(new DataPoint { XValue = x, YValue = telemetry.UserYaw });
                _graph.Graph.Series[SERIES_TELEM_USERTHROTTLE].DataPoints.Add(new DataPoint { XValue = x, YValue = telemetry.UserThrottle });
            });
        }

        private void UpdateGraph(Action<double> action)
        {
            if (_paused)
            {
                return;
            }

            lock (_graph.Graph.Lock)
            {
                double x = (DateTime.Now - _startTime).TotalSeconds;
                double timeToRemove = x - _lastSecondsToGraph;

                foreach (Series series in _graph.Graph.Series)
                {
                    while (series.DataPoints.Count > 0 && series.DataPoints[0].XValue <= timeToRemove)
                    {
                        series.DataPoints.RemoveAt(0);
                    }
                }

                action(x);
            }

            if (DateTime.Now - _lastRedraw < TimeSpan.FromMilliseconds(100))
            {
                return;
            }

            Dispatcher.BeginInvoke((Action)(() =>
            {
                _lastSecondsToGraph = _secondsToGraph.Value;

                if (!IsEnabled)
                {
                    return;
                }
                _graph.Redraw();
                _lastRedraw = DateTime.Now;
            }));
        }

        private void Series_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox seriesCheckbox in GetSeriesCheckboxes())
            {
                int seriesIndex = (int)seriesCheckbox.GetValue(SeriesIndex);
                _graph.Graph.Series[seriesIndex].Visible = IsChecked(seriesCheckbox.IsChecked);
            }
            _graph.Redraw();

            if (!Settings.Instance.IsSaveSuspended)
            {
                foreach (CheckBox seriesCheckbox in GetSeriesCheckboxes())
                {
                    int seriesIndex = (int)seriesCheckbox.GetValue(SeriesIndex);
                    Settings.Instance.SeriesToGraph[seriesIndex] = IsChecked(seriesCheckbox.IsChecked);
                }
                Settings.Instance.Save();
            }
        }

        private IEnumerable<CheckBox> GetSeriesCheckboxes()
        {
            return _series.Children.OfType<CheckBox>();
        }

        private bool IsChecked(bool? isChecked)
        {
            if (isChecked == null)
            {
                return true;
            }
            return isChecked.Value;
        }

        private void _graph_MouseMove(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition(_graph);
            PointF loc = _graph.GraphRenderer.PixelToGraphCoordinates(new System.Drawing.Point((int)pt.X, (int)pt.Y));
            _mouseLocationX.Text = loc.X.ToString("#,##0.###");
            _mouseLocationY.Text = loc.Y.ToString("#,##0.###");
        }

        private void _clearData_Click(object sender, RoutedEventArgs e)
        {
            lock (_graph.Graph.Lock)
            {
                foreach (Series series in _graph.Graph.Series)
                {
                    series.DataPoints.Clear();
                }
            }
        }

        private void _secondsToGraph_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Settings.Instance.IsSaveSuspended)
            {
                Settings.Instance.SecondsToGraph = e.NewValue;
                Settings.Instance.Save();
            }
        }

        private void _pause_Click(object sender, RoutedEventArgs e)
        {
            _paused = _pause.IsChecked == null ? false : _pause.IsChecked.Value;
        }
    }
}
