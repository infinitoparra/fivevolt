using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace Quadcopter
{
    /// <summary>
    /// Interaction logic for Raw.xaml
    /// </summary>
    public partial class Raw : UserControl
    {
        private ComLink _comLink;
        private DateTime _telemetryLastUpdate = DateTime.Now;
        private string _data;
        private DateTime _dataLastUpdate = DateTime.Now;
        private DateTime _lastLoopCountTime = DateTime.Now;
        private int _lastLoopCount;

        public Raw()
        {
            InitializeComponent();
            _rollPid.ValueChanged += _rollPid_ValueChanged;
            _pitchPid.ValueChanged += _pitchPid_ValueChanged;
            _yawPid.ValueChanged += _yawPid_ValueChanged;
        }

        void _yawPid_ValueChanged(object sender)
        {
            try
            {
                _comLink.UpdatePid(PidType.Yaw, _yawPid.GetPidValue());
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        void _pitchPid_ValueChanged(object sender)
        {
            try
            {
                _comLink.UpdatePid(PidType.Pitch, _pitchPid.GetPidValue());
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        void _rollPid_ValueChanged(object sender)
        {
            try
            {
                _comLink.UpdatePid(PidType.Roll, _rollPid.GetPidValue());
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public ComLink ComLink
        {
            get { return _comLink; }
            set
            {
                _comLink = value;
                _comLink.DataArrived += _comLink_DataArrived;
                _comLink.TelemetryArrived += _comLink_TelemetryArrived;
                _comLink.Error += _comLink_Error;
                _comLink.SettingsArrived += _comLink_SettingsArrived;
            }
        }

        void _comLink_SettingsArrived(SettingsPacket settings)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                _rollPid.UpdateValue(settings.PidRoll);
                _pitchPid.UpdateValue(settings.PidPitch);
                _yawPid.UpdateValue(settings.PidYaw);
                _windupGuard.Text = settings.WindupGuard.ToString();
                _userControlGain.Text = settings.Gain.ToString();
            }));
        }

        void _comLink_TelemetryArrived(Telemetry telemetry)
        {
            if ( DateTime.Now - _telemetryLastUpdate < TimeSpan.FromMilliseconds(100))
            {
                return;
            }
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!IsEnabled)
                {
                    return;
                }
                _motorLeft.Text = telemetry.MotorLeft.ToString("##0.00");
                _motorRight.Text = telemetry.MotorRight.ToString("##0.00");
                _motorFront.Text = telemetry.MotorFront.ToString("##0.00");
                _motorBack.Text = telemetry.MotorBack.ToString("##0.00");

                _userThrottle.Text = telemetry.UserThrottle.ToString("##0.00");
                _userRoll.Text = telemetry.UserRoll.ToString("##0.00");
                _userPitch.Text = telemetry.UserPitch.ToString("##0.00");
                _userYaw.Text = telemetry.UserYaw.ToString("##0.00");

                _telemYaw.Text = telemetry.Yaw.ToString("##0.00");
                _telemPitch.Text = telemetry.Pitch.ToString("##0.00");
                _telemRoll.Text = telemetry.Roll.ToString("##0.00");

                _battery.Text = telemetry.BatteryLevel.ToString("##0.00");

                if (DateTime.Now - _lastLoopCountTime > TimeSpan.FromMilliseconds(1000))
                {
                    double loopSpeed = (telemetry.LoopCount - _lastLoopCount) / (DateTime.Now - _lastLoopCountTime).TotalSeconds;
                    _lastLoopCount = telemetry.LoopCount;
                    _lastLoopCountTime = DateTime.Now;
                    _loopSpeed.Text = loopSpeed.ToString("##0.00") + "Hz";
                }

                _telemetryLastUpdate = DateTime.Now;
            }));
        }

        void _comLink_DataArrived(byte[] data)
        {
            AddToData(Encoding.ASCII.GetString(data));
        }

        private void AddToData(string str)
        {
            _data += str;
            if (DateTime.Now - _dataLastUpdate < TimeSpan.FromMilliseconds(100))
            {
                return;
            }
            Dispatcher.BeginInvoke((Action)(() =>
            {
                _comData.Text = _data;
                _comData.ScrollToEnd();
                _dataLastUpdate = DateTime.Now;
            }));

        }

        void _comLink_Error(Exception ex)
        {
            AddToData(ex + "\n");
        }

        private void Reconnect_Click(object sender, RoutedEventArgs e)
        {
            _comLink.Reconnect();
        }

        private void ClearCom_Click(object sender, RoutedEventArgs e)
        {
            _data = "";
            _comData.Text = "";
        }

        private void ReadSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _comLink.ReadSettings();
            }
            catch (Exception ex)
            {
                AddToData(ex.ToString());
            }
        }

        private void UserControlGain_Click(object sender, RoutedEventArgs e)
        {
            _comLink.UpdateUserControlGain(double.Parse(_userControlGain.Text));
        }

        private void WindupGuard_Click(object sender, RoutedEventArgs e)
        {
            _comLink.UpdateWindupGuard(double.Parse(_windupGuard.Text));
        }
    }
}
