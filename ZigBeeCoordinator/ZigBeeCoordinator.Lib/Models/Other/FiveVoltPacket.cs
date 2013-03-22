
using System;
using System.Text;
using ZigBeeCoordinator.Lib.Infrastructure;

namespace ZigBeeCoordinator.Lib.Models.Other
{
    public class FiveVoltPacket
    {
        protected const int PacketTypeLength = 1;

        public enum PacketType : byte
        {
            Capability = 0x01,
            LowBattery = 0x02,
            CapabilityData = 0x03,
            Message = 0x04
        }

        public static FiveVoltPacket Decode(byte[] packetData)
        {
            if (packetData.Length < PacketTypeLength)
            {
                return new InvalidFileVoltPacket("Invalid packet length. Expected at least [" + PacketTypeLength + "] found [" + packetData.Length + "]");
            }

            switch (packetData[0])
            {
                case (byte)PacketType.Capability:
                    return CapabilityFiveVoltPacket.Decode(packetData);
                case (byte)PacketType.CapabilityData:
                    return CapabilityDataFiveVoltPacket.Decode(packetData);
                case (byte)PacketType.Message:
                    return MessageFiveVoltPacket.Decode(packetData);
                default:
                    return new InvalidFileVoltPacket("Unhandled packet type [" + Convert.ToString(packetData[0], 16) + "]");
            }
        }
    }

    public class CapabilityFiveVoltPacket : FiveVoltPacket
    {
        public const int PacketLength = 3;
        private readonly UInt16 _capability;

        private CapabilityFiveVoltPacket(UInt16 capability)
        {
            _capability = capability;
        }

        public UInt16 Capability { get { return _capability; } }

        public new static FiveVoltPacket Decode(byte[] packetData)
        {
            if (packetData.Length != PacketLength)
            {
                return new InvalidFileVoltPacket("Invalid capability packet length. Expected [" + PacketLength + "] found [" + packetData.Length + "]");
            }

            return new CapabilityFiveVoltPacket((UInt16)((packetData[2] << 8) | packetData[1]));
        }
    }

    public class CapabilityDataFiveVoltPacket : FiveVoltPacket
    {
        private const int CapabilityLength = 2;
        private const int DataLengthLength = 2;
        private const int HeaderLength = PacketTypeLength + CapabilityLength + DataLengthLength;
        private readonly UInt16 _capability;
        private readonly byte[] _data;

        private CapabilityDataFiveVoltPacket(UInt16 capability, byte[] data)
        {
            _capability = capability;
            _data = data.Copy();
        }

        public byte[] Data { get { return _data; } }
        public UInt16 Capability { get { return _capability; } }

        public new static FiveVoltPacket Decode(byte[] packetData)
        {
            if (packetData.Length < HeaderLength)
            {
                return new InvalidFileVoltPacket("Invalid capability data packet length. Expected at least [" + HeaderLength + "] found [" + packetData.Length + "]");
            }

            return new CapabilityDataFiveVoltPacket((UInt16)((packetData[2] << 8) | packetData[1]), packetData.SelectRange(HeaderLength));
        }
    }

    public enum MessageLevel
    {
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4
    }

    public class MessageFiveVoltPacket : FiveVoltPacket
    {
        private const int LevelLength = 1;
        private const int HeaderLength = PacketTypeLength + LevelLength;

        public MessageLevel Level { get; set; }
        public string Message { get; set; }

        private MessageFiveVoltPacket(byte level, byte[] message)
        {
            Level = (MessageLevel)level;
            Message = Encoding.ASCII.GetString(message);
        }

        public new static FiveVoltPacket Decode(byte[] packetData)
        {
            if (packetData.Length < HeaderLength)
            {
                return new InvalidFileVoltPacket("Invalid message packet length. Expected at least [" + HeaderLength + "] found [" + packetData.Length + "]");
            }

            return new MessageFiveVoltPacket(packetData[1], packetData.SelectRange(HeaderLength));
        }
    }

    public class InvalidFileVoltPacket : FiveVoltPacket
    {
        private readonly string _reason;

        public InvalidFileVoltPacket(string reason)
        {
            _reason = reason;
        }

        public string Reason { get { return _reason; } }
    }
}
