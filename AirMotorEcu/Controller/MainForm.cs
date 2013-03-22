using System;
using System.Text;
using System.Windows.Forms;
using FiveVolt.Lib.Services;
using FiveVolt.Lib.View;
using FiveVolt.Lib.Helpers;
using AirMotorEcuController.Properties;
using System.IO;

namespace AirMotorEcuController
{
    public partial class MainForm : Form
    {
        private const string SettingsLogFile = "Settings.log";

        public MainForm()
        {
            InitializeComponent();
            _connectionStatus.Text = "Not Connected";
            _packetReceived.Text = "";

            _airOn.Value = Math2.Clamp(Settings.Default.IntakeOpen, _airOn.Minimum, _airOn.Maximum);
            _airOff.Value = Math2.Clamp(Settings.Default.IntakeClose, _airOff.Minimum, _airOff.Maximum);
            _exhaustOn.Value = Math2.Clamp(Settings.Default.ExhaustOpen, _exhaustOn.Minimum, _exhaustOff.Maximum);
            _exhaustOff.Value = Math2.Clamp(Settings.Default.ExhaustClose, _exhaustOff.Minimum, _exhaustOff.Maximum);

            UpdateValues();
            Program.ComService.StatusChanged += ComService_StatusChanged;
            Program.ComService.DataArrived += ComService_DataArrived;

            using (FileStream stream = File.Open(SettingsLogFile, FileMode.Create))
            using (TextWriter writer = new StreamWriter(stream))
            {
                string line = "RPMs,AirOn,AirOff,ExhaustOn,ExhaustOff";
                writer.WriteLine(line);
            }
        }

        void ComService_DataArrived(object sender, DataArrivedArgs eArg)
        {
            DataArrivedArgs e = eArg;
            this.BeginInvoke(() =>
            {
                try
                {
                    _packetReceived.Text = e.PacketType + " (" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ")";
                    if (e.PacketType == PacketType.PollData)
                    {
                        string str = Encoding.ASCII.GetString(e.Data);
                        str = str.Substring(1); // remove prefix
                        var parts = str.Split('\t');
                        int rpms = int.Parse(parts[0]);
                        _rpms.Text = rpms.ToString();
                    }
                }
                catch (Exception)
                {
                    // todo: for some reason this will throw if connection is lost
                }
            });
        }

        void ComService_StatusChanged(object sender, StatusChangedArgs e)
        {
            this.BeginInvoke(() =>
            {
                _connectionStatus.Text = e.Status.ToString();
            });
        }

        private void _exitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void _connectMenuItem_Click(object sender, EventArgs e)
        {
            ConnectDialog dlg = new ConnectDialog();
            dlg.PortName = Settings.Default.SerialPort;
            dlg.BaudRate = Settings.Default.SerialBaudRate;
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                Program.ComService.Connect(dlg.PortName, dlg.BaudRate, dlg.Parity, dlg.DataBits, dlg.StopBits);
                Settings.Default.SerialPort = dlg.PortName;
                Settings.Default.SerialBaudRate = dlg.BaudRate;
                Settings.Default.Save();
            }
        }

        private void _disconnectMenuItem_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        public void Disconnect()
        {
            Program.ComService.Disconnect();
        }

        private void UpdateValues()
        {
            _airOnValue.Text = FormatValue(_airOn.Value);
            _exhaustOnValue.Text = FormatValue(_exhaustOn.Value);
            _airOffValue.Text = FormatValue(_airOff.Value);
            _exhaustOffValue.Text = FormatValue(_exhaustOff.Value);
            Settings.Default.IntakeOpen = _airOn.Value;
            Settings.Default.IntakeClose = _airOff.Value;
            Settings.Default.ExhaustOpen = _exhaustOn.Value;
            Settings.Default.ExhaustClose = _exhaustOff.Value;
        }

        private void _airOn_Scroll(object sender, EventArgs e)
        {
            UpdateValues();
            SendNewParameters();
        }

        private void _exhaustOn_Scroll(object sender, EventArgs e)
        {
            UpdateValues();
            SendNewParameters();
        }

        private void _airOff_Scroll(object sender, EventArgs e)
        {
            UpdateValues();
            SendNewParameters();
        }

        private void _exhaustOff_Scroll(object sender, EventArgs e)
        {
            UpdateValues();
            SendNewParameters();
        }

        private void SendNewParameters()
        {
            if (Program.ComService.Status == Status.Connected)
            {
                Program.ComService.SendParameters(
                    _airOn.Value / 10.0,
                    _airOff.Value / 10.0,
                    _exhaustOn.Value / 10.0,
                    _exhaustOff.Value / 10.0);
            }
        }

        private string FormatValue(int i)
        {
            return i.ToString("##,##0");
        }

        private void _sendParameters_Click(object sender, EventArgs e)
        {
            SendNewParameters();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            _time.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.Save();
            Disconnect();
        }

        private void _writeSettings_Click(object sender, EventArgs e)
        {
            using (FileStream stream = File.Open(SettingsLogFile, FileMode.Append))
            using (TextWriter writer = new StreamWriter(stream))
            {
                string line = _rpms.Text 
                    + "," + _airOn.Value
                    + "," + _airOff.Value
                    + "," + _exhaustOn.Value 
                    + "," + _exhaustOff.Value;
                writer.WriteLine(line);
            }
        }
    }
}
