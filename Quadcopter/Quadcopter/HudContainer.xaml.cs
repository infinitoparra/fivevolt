using System;
using System.Windows.Controls;

namespace Quadcopter
{
    public partial class HudContainer : UserControl
    {
        private DateTime _lastUpdate;
        private ComLink _comLink;

        public HudContainer()
        {
            InitializeComponent();
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
            }
        }

        void _comLink_TelemetryArrived(Telemetry telemetry)
        {
            if (DateTime.Now - _lastUpdate < TimeSpan.FromMilliseconds(100))
            {
                return;
            }
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!IsEnabled)
                {
                    return;
                }
                _hud.SuspendRedraw();
                _hud.Roll = telemetry.Roll;
                _hud.Pitch = telemetry.Pitch;
                _hud.Yaw = telemetry.Yaw;
                _hud.QuadcopterFrontLevel = telemetry.MotorFront;
                _hud.QuadcopterBackLevel = telemetry.MotorBack;
                _hud.QuadcopterLeftLevel = telemetry.MotorLeft;
                _hud.QuadcopterRightLevel = telemetry.MotorRight;
                _hud.BatteryLevel = telemetry.BatteryLevel;
                _hud.UserInputElevation = telemetry.UserThrottle;
                _hud.UserInputPitch = telemetry.UserPitch;
                _hud.UserInputRoll = telemetry.UserRoll;
                _hud.UserInputYaw = telemetry.UserYaw;
                _hud.ResumeRedraw();
                _lastUpdate = DateTime.Now;
            }));
        }
    }
}
