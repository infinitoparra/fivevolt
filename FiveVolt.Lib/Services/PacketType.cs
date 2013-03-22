namespace FiveVolt.Lib.Services
{
    public class PacketType
    {
        public static PacketType Info = new PacketType("Info", '!');
        public static PacketType Success = new PacketType("Success",'+');
        public static PacketType Failure = new PacketType("Failure",'-');
        public static PacketType Unknown = new PacketType();
        public static PacketType PollData = new PacketType("Poll Data", '*');
        private string _name;
        private char _prefix;

        private PacketType(string name, char prefix)
        {
            _name = name;
            _prefix = prefix;
        }

        private PacketType()
        {
        }

        public override string ToString()
        {
            return _name;
        }
    }
}