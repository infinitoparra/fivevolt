using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace DummyLoadController.Services
{
    public class ComService : IDisposable
    {
        private Thread _thread;
        private bool _run;
        private SerialPort _port;
        private readonly object _lock = new object();
        private Status _status = Status.Disconnected;
        public event EventHandler<DataArrivedArgs> DataArrived;
        public event EventHandler<StatusChangedArgs> StatusChanged;

        public void SetTargetCurrent(double targetCurrent)
        {
            Send("setload " + targetCurrent.ToString("##0.###") + "\n");
        }

        public void Send(string str)
        {
            lock (_lock)
            {
                if (_port == null)
                {
                    throw new Exception("Not connected");
                }

                byte[] bytes = Encoding.ASCII.GetBytes(str);
                _port.Write(bytes, 0, bytes.Length);
            }
        }

        public void Connect(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            Disconnect();

            SetStatus(Status.Connecting);
            _port = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _port.Open();
            SendEchoOff();

            EnsureThreadStarted();
        }

        private void SendEchoOff()
        {
            Send("echooff\n");
        }

        private void SetStatus(Status newStatus)
        {
            if (_status != newStatus)
            {
                _status = newStatus;
                OnStatusChanged(this, new StatusChangedArgs(_status));
            }
        }

        private void EnsureThreadStarted()
        {
            if (_thread != null && _thread.IsAlive)
            {
                return;
            }

            _run = true;
            _thread = new Thread(ThreadProc);
            _thread.Start();
        }

        private void SendDataRequest()
        {
            Send("*\n");
        }

        private void ThreadProc()
        {
            bool pendingDataRequest = false;
            byte[] buffer = new byte[1024];
            MemoryStream receiveBuffer = new MemoryStream();
            while (_run)
            {
                lock (_lock)
                {
                    if (!pendingDataRequest)
                    {
                        SendDataRequest();
                        pendingDataRequest = true;
                    }

                    while (_port.BytesToRead > 0)
                    {
                        int read = _port.Read(buffer, 0, Math.Min(buffer.Length, _port.BytesToRead));
                        for (int i = 0; i < read; i++)
                        {
                            receiveBuffer.WriteByte(buffer[i]);
                            if (buffer[i] == '\n')
                            {
                                byte[] data = receiveBuffer.ToArray();
                                if (data.Length > 0)
                                {
                                    PacketType packetType;
                                    switch ((char)data[0])
                                    {
                                        case '!': packetType = PacketType.Info; break;
                                        case '*':
                                            packetType = PacketType.Controller;
                                            pendingDataRequest = false;
                                            break;
                                        case '+': packetType = PacketType.Success; break;
                                        case '-': packetType = PacketType.Failure; break;
                                        default:
                                            packetType = PacketType.Unknown;
                                            break;
                                    }

                                    if (packetType == PacketType.Info)
                                    {
                                        string str = Encoding.ASCII.GetString(data);
                                        if (str.StartsWith("!DummyLoad")) // reset
                                        {
                                            pendingDataRequest = false;
                                            SendEchoOff();
                                        }
                                    }

                                    OnDataArrived(this, new DataArrivedArgs(packetType, data));
                                    receiveBuffer = new MemoryStream();
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        public void Disconnect()
        {
            lock (_lock)
            {
                _run = false;
                if (_port != null)
                {
                    _port.Close();
                    _port = null;
                }
                if (_thread != null)
                {
                    _thread.Join();
                    _thread = null;
                }
            }
            SetStatus(Status.Disconnected);
        }

        public IEnumerable<Port> GetPorts()
        {
            return SerialPort.GetPortNames().Select(portName => new Port(portName));
        }

        private void OnDataArrived(ComService sender, DataArrivedArgs args)
        {
            if (DataArrived != null)
            {
                DataArrived(sender, args);
            }
        }

        private void OnStatusChanged(ComService sender, StatusChangedArgs args)
        {
            if (StatusChanged != null)
            {
                StatusChanged(sender, args);
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        public ControllerData ParseControllerData(byte[] data)
        {
            string str = Encoding.ASCII.GetString(data);
            if (!str.StartsWith("*"))
            {
                throw new Exception("Invalid controller packet");
            }
            str = str.Substring(1);
            string[] parts = str.Split(',');
            return new ControllerData
            {
                LoadCurrentDac = int.Parse(parts[0]),
                LoadCurrentSense = double.Parse(parts[1]) / 1000.0,
                LoadCurrentTarget = double.Parse(parts[2]) / 1000.0,
                ExternalCurrent = double.Parse(parts[3]) / 1000.0,
                LoadVoltage = double.Parse(parts[4]) / 1000.0,
                ExternalVoltage = double.Parse(parts[5]) / 1000.0
            };
        }

        public class ControllerData
        {
            public int LoadCurrentDac { get; set; }
            public double LoadCurrentSense { get; set; }
            public double LoadCurrentTarget { get; set; }
            public double ExternalCurrent { get; set; }
            public double LoadVoltage { get; set; }
            public double ExternalVoltage { get; set; }
        }

        public enum PacketType
        {
            Info,
            Controller,
            Success,
            Failure,
            Unknown
        }

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

        public class Port
        {
            private readonly string _name;

            public Port(string name)
            {
                _name = name;
            }

            public string Name { get { return _name; } }
        }

        public class StatusChangedArgs : EventArgs
        {
            private readonly Status _status;

            public StatusChangedArgs(Status status)
            {
                _status = status;
            }

            public Status Status { get { return _status; } }
        }

        public enum Status
        {
            Connected,
            Disconnected,
            Connecting
        }
    }
}
// ReSharper restore MemberHidesStaticFromOuterClass
