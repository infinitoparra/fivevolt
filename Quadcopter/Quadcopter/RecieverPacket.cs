using System;
namespace Quadcopter
{
    public class RecieverPacket
    {
        public RecieverPacket()
        {
            Channel = new float[6];
        }

        public UInt32 Time { get; set; }
        public bool HasSignal { get; set; }
        public float[] Channel { get; set; }
    }
}
