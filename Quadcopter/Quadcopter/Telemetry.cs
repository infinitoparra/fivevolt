using System;

namespace Quadcopter
{
    public class Telemetry
    {
        public double Pitch { get; set; }
        public double Roll { get; set; }
        public double Yaw { get; set; }
        public double MotorFront { get; set; }
        public double MotorBack { get; set; }
        public double MotorLeft { get; set; }
        public double MotorRight { get; set; }
        public double BatteryLevel { get; set; }
        public double UserThrottle { get; set; }
        public double UserPitch { get; set; }
        public double UserRoll { get; set; }
        public double UserYaw { get; set; }
        public int LoopCount { get; set; }
    }
}
