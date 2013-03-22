using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace FiveVolt.Lib.Services
{
    public abstract class ComServiceBase : IDisposable
    {
        private Thread _thread;
        private bool _run;
        private SerialPort _port;
        private readonly object _lock = new object();
        private Status _status = Status.Disconnected;
        public abstract string ResetStringPrefix { get; }
        public event EventHandler<DataArrivedArgs> DataArrived;
        public event EventHandler<StatusChangedArgs> StatusChanged;
        public bool EnableDataPolling { get; set; }
        public int DataPollFrequency { get; set; }

        protected ComServiceBase()
        {
            EnableDataPolling = true;
            DataPollFrequency = 100;
        }

        public Status Status
        {
            get { return _status; }
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
            SetStatus(Status.Connected);
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
                    if (!pendingDataRequest && EnableDataPolling)
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
                                    PacketType packetType = GetPacketType(data);

                                    if (packetType == PacketType.PollData)
                                    {
                                        pendingDataRequest = false;
                                    }
                                    else if (packetType == PacketType.Info)
                                    {
                                        string str = Encoding.ASCII.GetString(data);
                                        if (str.StartsWith("!" + ResetStringPrefix)) // reset
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
                Thread.Sleep(DataPollFrequency);
            }
        }

        protected virtual PacketType GetPacketType(byte[] data)
        {
            PacketType packetType;
            switch ((char)data[0])
            {
                case '!': packetType = PacketType.Info; break;
                case '*': packetType = PacketType.PollData; break;
                case '+': packetType = PacketType.Success; break;
                case '-': packetType = PacketType.Failure; break;
                default: packetType = PacketType.Unknown; break;
            }
            return packetType;
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

        private void OnDataArrived(ComServiceBase sender, DataArrivedArgs args)
        {
            if (DataArrived != null)
            {
                DataArrived(sender, args);
            }
        }

        private void OnStatusChanged(ComServiceBase sender, StatusChangedArgs args)
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
    }
}

