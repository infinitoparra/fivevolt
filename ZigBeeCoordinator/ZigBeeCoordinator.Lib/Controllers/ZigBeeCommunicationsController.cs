
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Ports;
using log4net;
using ZigBeeCoordinator.Lib.Infrastructure;

namespace ZigBeeCoordinator.Lib.Controllers
{
    [Export(typeof(ZigBeeCommunicationsController))]
    public class ZigBeeCommunicationsController
    {
        private static readonly ILog _log = MyLogManager.GetLogger(typeof(ZigBeeCommunicationsController));
        private const byte StartDelimiter = 0x7e;
        private const int FrameIdLength = 1;
        private const int AddressLength = 8;
        private const int NetworkAddressLength = 2;
        public static readonly byte[] CoordinatorAddress = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public static readonly byte[] CoordinatorNetworkAddress = new byte[] { 0xff, 0xfe };

        public enum CmdIds : byte
        {
            ZigBeeReceivePacket = 0x90
        }

        private SerialPort _serialPort;
        private readonly List<byte> _buffer = new List<byte>();

        public delegate void PacketArrivedDelegate(ZigBeeCommunicationsController source, Packet packet);

        public event PacketArrivedDelegate PacketArrived;

        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }

        public ZigBeeCommunicationsController()
        {
            DataBits = 8;
            Parity = Parity.None;
            StopBits = StopBits.One;
        }

        public void Start()
        {
            if (IsStarted)
            {
                throw new Exception("Communications already started");
            }

            if (string.IsNullOrEmpty(PortName))
            {
                throw new Exception("You must configure the PortName");
            }
            if (BaudRate == default(int))
            {
                throw new Exception("You must configure the BaudRate");
            }

            _log.Info("Starting communications on port [" + PortName + "], baud rate [" + BaudRate + "]");
            _serialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
            _serialPort.DataReceived += _serialPort_DataReceived;
            _serialPort.Open();
            _log.Debug("Started");
        }

        public void Stop()
        {
            if (!IsStarted)
            {
                throw new Exception("Communications not started");
            }
            _log.Info("Stopping");
            _serialPort.Close();
            _serialPort = null;
            _log.Debug("Stopped");
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (_serialPort.BytesToRead > 0)
            {
                byte[] buffer = new byte[_serialPort.BytesToRead];
                _serialPort.Read(buffer, 0, buffer.Length);
                _buffer.AddRange(buffer);
            }
            ProcessBuffer();
        }

        private void ProcessBuffer()
        {
            const int HeaderLength = 3; // Start Delimiter (1) + Length (2)
            const int FooterLength = 1; // Checksum (1)
            const int HeaderAndFooterLength = HeaderLength + FooterLength;
            _buffer.RemoveUntil(StartDelimiter);
            if (_buffer.Count >= HeaderAndFooterLength && _buffer[0] == StartDelimiter)
            {
                int packetLength = (_buffer[1] << 8) | _buffer[2];
                if (_buffer.Count >= packetLength + HeaderAndFooterLength)
                {
                    DateTime receivedDateTime = DateTime.Now;
                    byte[] packetData = new byte[packetLength];
                    _buffer.CopyTo(HeaderLength, packetData, 0, packetLength);
                    byte checksum = _buffer[HeaderLength + packetLength];
                    _buffer.RemoveRange(0, HeaderLength + packetLength + FooterLength);
                    byte calculatedChecksum = CalculateChecksum(packetData);
                    Packet packet;
                    if (calculatedChecksum == checksum && packetData.Length > 0)
                    {
                        packet = DecodePacket(packetData, receivedDateTime);
                    }
                    else
                    {
                        packet = CreateInvalidPacket(packetData, receivedDateTime, "Invalid checksum, expected [" + calculatedChecksum + "] found [" + checksum + "]");
                    }
                    OnPacketArrived(packet);
                }
            }
        }

        private static InvalidPacket CreateInvalidPacket(byte[] packetData, DateTime receivedDateTime, string reason)
        {
            _log.Warn("Reveived invalid packet, reason \"" + reason + "\" [" + packetData.ToHexString() + "]");
            return new InvalidPacket(packetData, receivedDateTime, CoordinatorAddress, CoordinatorNetworkAddress, reason);
        }

        public static Packet DecodePacket(byte[] packetData, DateTime receivedDateTime)
        {
            switch (packetData[0])
            {
                case (byte)CmdIds.ZigBeeReceivePacket:
                    return DecodeZigBeeReceivePacket(packetData, receivedDateTime);
                default:
                    return CreateInvalidPacket(packetData, receivedDateTime, "Invalid packet cmdID [0x" + Convert.ToString(packetData[0], 16) + "]");
            }
        }

        private static Packet DecodeZigBeeReceivePacket(byte[] packetData, DateTime receivedDateTime)
        {
            const int ReceiveOptionsLength = 1;
            const int PacketHeaderLength = FrameIdLength + AddressLength + NetworkAddressLength + ReceiveOptionsLength;
            if (packetData.Length < PacketHeaderLength)
            {
                return CreateInvalidPacket(packetData, receivedDateTime, "ZigBee Receive Packet to small, must be at least [" + PacketHeaderLength + "] found [" + packetData.Length + "]");
            }
            byte[] address = new byte[AddressLength];
            byte[] networkAddress = new byte[NetworkAddressLength];
            byte[] receiveData = new byte[packetData.Length - PacketHeaderLength];

            Array.Copy(packetData, 0, address, 0, AddressLength);
            Array.Copy(packetData, AddressLength, networkAddress, 0, NetworkAddressLength);
            ReceiveOptions receiveOptions = (ReceiveOptions)packetData[AddressLength + NetworkAddressLength];
            Array.Copy(packetData, PacketHeaderLength, receiveData, 0, receiveData.Length);
            _log.Debug("Received ZigBeeReceivePacket from address [" + address.ToHexString("") + "], network address [" + networkAddress.ToHexString("") + "], receiveOptions [" + Convert.ToString((byte)receiveOptions, 16) + "], data [" + receiveData.ToHexString() + "]");
            return new ZigBeeReceivePacket(packetData, receivedDateTime, address, networkAddress, receiveOptions, receiveData);
        }

        private void OnPacketArrived(Packet packet)
        {
            if (PacketArrived != null)
            {
                PacketArrived(this, packet);
            }
        }

        private byte CalculateChecksum(IEnumerable<byte> packetData)
        {
            return (byte)(0xff - packetData.Aggregate((byte)0, (a, b) => (byte)(a + b)));
        }

        protected bool IsStarted
        {
            get { return _serialPort != null; }
        }

        public abstract class Packet
        {
            private readonly byte[] _packetData;
            private readonly DateTime _receivedDataTime;
            private readonly byte[] _address;
            private readonly byte[] _networkAddress;

            public byte[] PacketData { get { return _packetData; } }
            public DateTime ReceivedDataTime { get { return _receivedDataTime; } }
            public byte[] Address { get { return _address; } }
            public byte[] NetworkAddress { get { return _networkAddress; } }

            protected Packet(byte[] packetData, DateTime receivedDataTime, byte[] address, byte[] networkAddress)
            {
                _packetData = packetData.Copy();
                _receivedDataTime = receivedDataTime;
                _address = address.Copy();
                _networkAddress = networkAddress.Copy();
            }
        }

        [Flags]
        public enum ReceiveOptions : byte
        {
            Acknowledged = 0x01,
            Broadcast = 0x02,
            EncryptedWithAps = 0x20,
            FromEndDevice = 0x40
        }

        public class ZigBeeReceivePacket : Packet
        {
            private readonly ReceiveOptions _receiveOptions;
            private readonly byte[] _receiveData;

            public ReceiveOptions ReceiveOptions { get { return _receiveOptions; } }
            public byte[] ReceiveData { get { return _receiveData; } }

            public ZigBeeReceivePacket(byte[] packetData, DateTime receivedDataTime, byte[] address, byte[] networkAddress, ReceiveOptions receiveOptions, byte[] receiveData)
                : base(packetData, receivedDataTime, address, networkAddress)
            {
                _receiveOptions = receiveOptions;
                _receiveData = receiveData.Copy();
            }
        }

        public class InvalidPacket : Packet
        {
            private readonly string _reason;

            public string Reason { get { return _reason; } }

            public InvalidPacket(byte[] packetData, DateTime receivedDataTime, byte[] address, byte[] networkAddress, string reason)
                : base(packetData, receivedDataTime, address, networkAddress)
            {
                _reason = reason;
            }
        }
    }
}
