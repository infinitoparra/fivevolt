namespace Quadcopter
{
    public class SettingsPacket
    {
        public PidObj PidRoll { get; set; }
        public PidObj PidPitch { get; set; }
        public PidObj PidYaw { get; set; }
        public float Gain { get; set; }
        public float WindupGuard { get; set; }
    }
}
