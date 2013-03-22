using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using Quadcopter.Infrastructure;
using System.Text;
using System.IO;

namespace Quadcopter
{
    public class ComLink
    {
        private const byte PT_TELEMETRY = 0x01;
        private const byte PT_SETTINGS = 0x02;
        private const byte PT_RECV = 0x11;
        private const byte PT_IMU = 0x21;

        private const int SIZE_SETTINGS = (3 * (3 * BinaryHelper.SIZE_FLOAT)) + (2 * BinaryHelper.SIZE_FLOAT);
        private const int SIZE_TELEMETRY = (12 * BinaryHelper.SIZE_FLOAT) + (1 * BinaryHelper.SIZE_LONG);
        private const int SIZE_RECV = 1 + (6 * BinaryHelper.SIZE_FLOAT);
        private const int SIZE_IMU = (1 * BinaryHelper.SIZE_LONG) + (9 * BinaryHelper.SIZE_FLOAT);

        public delegate void TelemetryDelegate(Telemetry telemetry);
        public event TelemetryDelegate TelemetryArrived;

        public delegate void DataDelegate(byte[] data);
        public event DataDelegate DataArrived;

        public delegate void ErrorDelegate(Exception ex);
        public event ErrorDelegate Error;

        public delegate void SettingsDelegate(SettingsPacket data);
        public event SettingsDelegate SettingsArrived;

        public delegate void ReceiverArrivedDelegate(RecieverPacket data);
        public event ReceiverArrivedDelegate ReceiverArrived;

        public delegate void ImuArrivedDelegate(ImuPacket data);
        public event ImuArrivedDelegate ImuArrived;

        readonly byte[] _header = new[] { (byte)'Q', (byte)'U', (byte)'A', (byte)'D' };
        readonly MemoryQueue _queue = new MemoryQueue();
        byte? _packetType;
        private SerialPort _serialPort;
        private Thread _thread;
        private bool _continue;
        private readonly FileStream _logging;

        protected string PortName { get; set; }
        protected int BaudRate { get; set; }

        public ComLink()
        {
            PortName = "COM6";
            BaudRate = 19200;
            _logging = File.Open("c:/out.csv", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        }

        public void Open()
        {
            _serialPort = new SerialPort
            {
                BaudRate = BaudRate,
                PortName = PortName
            };
            _continue = true;
            _thread = new Thread(ProcessThread)
            {
                Name = "ComLink"
            };
            _thread.Start();
        }

        public void Close()
        {
            _continue = false;
            _serialPort.Close();
        }

        private void ProcessThread()
        {
            try
            {
                _serialPort.Open();
                while (_continue)
                {
                    try
                    {
                        ProcessSerialPortData();
                    }
                    catch (Exception ex)
                    {
                        if (!_continue)
                        {
                            return;
                        }
                        Trace.WriteLine(ex);
                        OnError(ex);
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex2)
            {
                if (!_continue)
                {
                    return;
                }
                Trace.WriteLine(ex2);
                OnError(ex2);
            }
        }

        private void ProcessSerialPortData()
        {
            byte[] buffer = new byte[1024];

            if (_serialPort.BytesToRead == 0)
            {
                Thread.Sleep(100);
            }

            while (true)
            {
                if (_serialPort.BytesToRead == 0)
                {
                    break;
                }
                int bytesRead = _serialPort.Read(buffer, 0, Math.Min(buffer.Length, _serialPort.BytesToRead));
                if (!_continue || bytesRead == 0)
                {
                    break;
                }
                _queue.Write(buffer, 0, bytesRead);

                bool stop = false;
                while (_queue.Length > _header.Length && !stop)
                {
                    if (_packetType == null)
                    {
                        byte[] otherData = _queue.DequeueUntil(_header);
                        if (otherData != null && otherData.Length > 0)
                        {
                            OnDataArrived(otherData);
                        }
                        if (!_queue.StartsWith(_header))
                        {
                            break;
                        }
                        _queue.Dequeue(_header.Length);
                        _packetType = _queue.Dequeue();
                    }

                    switch (_packetType.Value)
                    {
                        case PT_TELEMETRY:
                            if (_queue.Length >= SIZE_TELEMETRY)
                            {
                                byte[] telemetryBytes = _queue.Dequeue(SIZE_TELEMETRY);
                                _packetType = null;
                                int i = 0;
                                Telemetry telemetry = new Telemetry
                                {
                                    Yaw = BinaryHelper.ReadFloat(telemetryBytes, ref i),
                                    Pitch = BinaryHelper.ReadFloat(telemetryBytes, ref i),
                                    Roll = BinaryHelper.ReadFloat(telemetryBytes, ref i),
                                    MotorLeft = BinaryHelper.ReadFloat(telemetryBytes, ref i),
                                    MotorRight = BinaryHelper.ReadFloat(telemetryBytes, ref i),
                                    MotorFront = BinaryHelper.ReadFloat(telemetryBytes, ref i),
                                    MotorBack = BinaryHelper.ReadFloat(telemetryBytes, ref i),
                                    BatteryLevel = BinaryHelper.ReadFloat(telemetryBytes, ref i),
                                    UserThrottle = BinaryHelper.ReadFloat(telemetryBytes, ref i),
                                    UserPitch = BinaryHelper.ReadFloat(telemetryBytes, ref i),
                                    UserRoll = BinaryHelper.ReadFloat(telemetryBytes, ref i),
                                    UserYaw = BinaryHelper.ReadFloat(telemetryBytes, ref i),
                                    LoopCount = BinaryHelper.ReadLong(telemetryBytes, ref i)
                                };

                                WriteToLog(telemetry.Pitch + "," + telemetry.UserThrottle + "," + telemetry.MotorFront + "," + telemetry.MotorBack);

                                OnTelemetryArrived(telemetry);
                            }
                            else
                            {
                                stop = true;
                            }
                            break;

                        case PT_SETTINGS:
                            if (_queue.Length >= SIZE_SETTINGS)
                            {
                                byte[] pidBytes = _queue.Dequeue(SIZE_SETTINGS);
                                _packetType = null;
                                int i = 0;
                                SettingsPacket settings = new SettingsPacket
                                {
                                    PidRoll = new PidObj
                                    {
                                        P = BinaryHelper.ReadFloat(pidBytes, ref i),
                                        I = BinaryHelper.ReadFloat(pidBytes, ref i),
                                        D = BinaryHelper.ReadFloat(pidBytes, ref i)
                                    },
                                    PidPitch = new PidObj
                                    {
                                        P = BinaryHelper.ReadFloat(pidBytes, ref i),
                                        I = BinaryHelper.ReadFloat(pidBytes, ref i),
                                        D = BinaryHelper.ReadFloat(pidBytes, ref i)
                                    },
                                    PidYaw = new PidObj
                                    {
                                        P = BinaryHelper.ReadFloat(pidBytes, ref i),
                                        I = BinaryHelper.ReadFloat(pidBytes, ref i),
                                        D = BinaryHelper.ReadFloat(pidBytes, ref i)
                                    },
                                    Gain = BinaryHelper.ReadFloat(pidBytes, ref i),
                                    WindupGuard = BinaryHelper.ReadFloat(pidBytes, ref i)
                                };
                                OnSettingsArrived(settings);
                            }
                            else
                            {
                                stop = true;
                            }
                            break;

                        case PT_RECV:
                            if (_queue.Length >= SIZE_RECV)
                            {
                                byte[] recvBytes = _queue.Dequeue(SIZE_RECV);
                                _packetType = null;
                                int i = 0;
                                RecieverPacket recvData = new RecieverPacket
                                {
                                    Time = BinaryHelper.ReadUnsignedLong(recvBytes, ref i),
                                    HasSignal = BinaryHelper.ReadByte(recvBytes, ref i) == 0 ? false : true,
                                };
                                recvData.Channel[0] = BinaryHelper.ReadFloat(recvBytes, ref i);
                                recvData.Channel[1] = BinaryHelper.ReadFloat(recvBytes, ref i);
                                recvData.Channel[2] = BinaryHelper.ReadFloat(recvBytes, ref i);
                                recvData.Channel[3] = BinaryHelper.ReadFloat(recvBytes, ref i);
                                recvData.Channel[4] = BinaryHelper.ReadFloat(recvBytes, ref i);
                                recvData.Channel[5] = BinaryHelper.ReadFloat(recvBytes, ref i);

                                WriteToLog(recvData.Time + "," + recvData.HasSignal + "," + recvData.Channel[0] + "," + recvData.Channel[1] + "," + recvData.Channel[2] + "," + recvData.Channel[3] + "," + recvData.Channel[4] + "," + recvData.Channel[5]);
                                OnReceiverArrived(recvData);
                            }
                            else
                            {
                                stop = true;
                            }
                            break;

                        case PT_IMU:
                            if (_queue.Length >= SIZE_IMU)
                            {
                                byte[] recvBytes = _queue.Dequeue(SIZE_IMU);
                                _packetType = null;
                                int i = 0;
                                ImuPacket imuData = new ImuPacket
                                {
                                    Time = BinaryHelper.ReadUnsignedLong(recvBytes, ref i),
                                    AccelRoll = BinaryHelper.ReadFloat(recvBytes, ref i),
                                    AccelPitch = BinaryHelper.ReadFloat(recvBytes, ref i),
                                    CompassYaw = BinaryHelper.ReadFloat(recvBytes, ref i),
                                    Gx = BinaryHelper.ReadFloat(recvBytes, ref i),
                                    Gy = BinaryHelper.ReadFloat(recvBytes, ref i),
                                    Gz = BinaryHelper.ReadFloat(recvBytes, ref i),
                                    Roll = BinaryHelper.ReadFloat(recvBytes, ref i),
                                    Pitch = BinaryHelper.ReadFloat(recvBytes, ref i),
                                    Yaw = BinaryHelper.ReadFloat(recvBytes, ref i)
                                };

                                WriteToLog((imuData.Time / 1000000.0).ToString("0.00") + "," + imuData.Gx + "," + imuData.Gy + "," + imuData.Gz + "," + imuData.AccelRoll + "," + imuData.AccelPitch + "," + imuData.CompassYaw + "," + imuData.Roll + "," + imuData.Pitch + "," + imuData.Yaw);
                                OnImuArrived(imuData);
                            }
                            else
                            {
                                stop = true;
                            }
                            break;

                        default:
                            _packetType = null;
                            break;
                    }
                }
            }
        }

        private void WriteToLog(string str)
        {
            StringBuilder o = new StringBuilder();
            o.AppendLine(str);
            byte[] od = Encoding.ASCII.GetBytes(o.ToString());
            _logging.Write(od, 0, od.Length);
            _logging.Flush();
        }

        public void UpdatePid(PidType type, PidObj pid)
        {
            byte[] buffer = new byte[1 + 1 + (3 * BinaryHelper.SIZE_FLOAT) + 1];
            int i = 0;
            buffer[i++] = (byte)'P';
            buffer[i++] = PidTypeToCode(type);
            BinaryHelper.WriteFloat(pid.P, buffer, ref i);
            BinaryHelper.WriteFloat(pid.I, buffer, ref i);
            BinaryHelper.WriteFloat(pid.D, buffer, ref i);
            buffer[i++] = BinaryHelper.ComputeChecksumCRC8(buffer, 1, buffer.Length - 1);
            SendRequest(buffer, i);
        }

        public void UpdateUserControlGain(double val)
        {
            byte[] buffer = new byte[1 + (1 * BinaryHelper.SIZE_FLOAT) + 1];
            int i = 0;
            buffer[i++] = (byte)'G';
            BinaryHelper.WriteFloat(val, buffer, ref i);
            buffer[i++] = BinaryHelper.ComputeChecksumCRC8(buffer, 1, buffer.Length - 1);
            SendRequest(buffer, i);
        }

        public void UpdateWindupGuard(double val)
        {
            byte[] buffer = new byte[1 + (1 * BinaryHelper.SIZE_FLOAT) + 1];
            int i = 0;
            buffer[i++] = (byte)'W';
            BinaryHelper.WriteFloat(val, buffer, ref i);
            buffer[i++] = BinaryHelper.ComputeChecksumCRC8(buffer, 1, buffer.Length - 1);
            SendRequest(buffer, i);
        }

        private void SendRequest(byte[] buffer, int length)
        {
            for (int i = 0; i < length; i++)
            {
                _serialPort.Write(buffer, i, 1);
                // causes errors because we are not handling available bytes correctly on the quad: Thread.Sleep(5);
            }
        }

        private byte PidTypeToCode(PidType type)
        {
            switch (type)
            {
                case PidType.Yaw:
                    return (byte)'y';
                case PidType.Pitch:
                    return (byte)'p';
                case PidType.Roll:
                    return (byte)'r';
                default:
                    throw new Exception("Unhandled pid type '" + type + "'");
            }
        }

        private void OnTelemetryArrived(Telemetry telemetry)
        {
            if (TelemetryArrived != null)
            {
                TelemetryArrived(telemetry);
            }
        }

        private void OnDataArrived(byte[] data)
        {
            if (DataArrived != null)
            {
                DataArrived(data);
            }
        }

        private void OnSettingsArrived(SettingsPacket settings)
        {
            if (SettingsArrived != null)
            {
                SettingsArrived(settings);
            }
        }

        private void OnReceiverArrived(RecieverPacket recvData)
        {
            if (ReceiverArrived != null)
            {
                ReceiverArrived(recvData);
            }
        }

        private void OnImuArrived(ImuPacket imuData)
        {
            if (ImuArrived != null)
            {
                ImuArrived(imuData);
            }
        }

        private void OnError(Exception ex)
        {
            if (Error != null)
            {
                Error(ex);
            }
        }

        public void Reconnect()
        {
            Close();
            Open();
        }

        public void ReadSettings()
        {
            SendRequest(new[] { (byte)'s' }, 1);
        }
    }
}
