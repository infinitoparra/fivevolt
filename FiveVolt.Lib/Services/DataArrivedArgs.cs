using System;

namespace FiveVolt.Lib.Services
{
    public class DataArrivedArgs : EventArgs
    {
        private readonly byte[] _data;
        private readonly PacketType _packetType;

        public DataArrivedArgs(PacketType packetType, byte[] data)
        {
            _packetType = packetType;
            _data = data;
        }

        public PacketType PacketType { get { return _packetType; } }
        public byte[] Data { get { return _data; } }
    }
}