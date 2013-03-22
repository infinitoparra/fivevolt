using System;
using System.Linq;
using System.IO.Ports;
using System.Windows.Forms;

namespace FiveVolt.Lib.View
{
    public partial class ConnectDialog : Form
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }

        public ConnectDialog()
        {
            InitializeComponent();
            PortName = "COM1";
            BaudRate = 9600;
            Parity = Parity.None;
            DataBits = 8;
            StopBits = StopBits.One;
        }

        private void ConnectDialog_Load(object sender, EventArgs e)
        {
            _port.Items.Clear();
            _port.Items.AddRange(SerialPort.GetPortNames().OrderBy(s => s).ToArray());
            _port.SelectedItem = PortName;
            _baudRate.Text = BaudRate.ToString();
        }

        private void _ok_Click(object sender, EventArgs e)
        {
            PortName = (string)_port.SelectedItem;
            BaudRate = int.Parse(_baudRate.Text);
        }
    }
}
