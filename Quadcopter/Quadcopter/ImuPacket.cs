using System;

namespace Quadcopter
{
    public class ImuPacket
    {
        public UInt32 Time { get; set; }
        public float AccelRoll { get; set; }
        public float AccelPitch { get; set; }
        public float CompassYaw { get; set; }
        public float Gx { get; set; }
        public float Gy { get; set; }
        public float Gz { get; set; }
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
    }
}
