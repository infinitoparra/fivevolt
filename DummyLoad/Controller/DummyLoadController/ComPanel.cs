using System;
using System.Linq;
using System.IO.Ports;
using System.Text;
using DummyLoadController.Helpers;
using DummyLoadController.Services;
using WX;
using System.Xml.Linq;

namespace DummyLoadController
{
    public class ComPanel : Panel
    {
        private const int ID_Send = 1;
        private const int ID_SendText = 2;
        private const int ID_Connect = 3;
        private const int ID_Disconnect = 4;
        private const int ID_RefreshPorts = 5;

        private TextCtrl _textInput;
        private readonly TextCtrl _history;
        private readonly ComService _comService;
        private readonly Font _dataFont;
        private ComboBox _port;
        private Button _connect;
        private Button _disconnect;
        private BitmapButton _refreshPorts;
        private ComboBox _baudRate;
        private Button _send;

        public ComPanel(Window parent, ComService comService)
            : base(parent)
        {
            _comService = comService;
            _comService.DataArrived += ComDataArrived;
            _comService.StatusChanged += ComStatusChanged;

            var sizer = new BoxSizer(Orientation.Vertical);
            _dataFont = new Font(10, FontFamily.Teletype, FontStyle.Normal, FontWeight.Normal);

            Panel controlPanel = CreateControlPanel();
            sizer.Add(controlPanel, 0, SizerFlag.Expand | SizerFlag.All, 0);

            _history = new TextCtrl(this, "", WxDefaultPosition, WxDefaultSize,
                WindowStyles.TextCtrlMultiline | WindowStyles.TextCtrlReadonly)
                {
                    Font = _dataFont
                };
            sizer.Add(_history, 1, SizerFlag.Expand | SizerFlag.All, 0);

            Panel sendPanel = CreateSendPanel();
            sizer.Add(sendPanel, 0, SizerFlag.Expand | SizerFlag.All, 5);

            SetSizer(sizer);
            sizer.SetSizeHints(this);

            EvtSize((e, s) => Sizer.SetDimension(0, 0, Width, Height));
            EvtClose((e, s) => OnClose());

            DisableInput();
        }

        private Panel CreateSendPanel()
        {
            Panel panel = new Panel(this);
            BoxSizer sizer = new BoxSizer(Orientation.Horizontal);

            _textInput = new TextCtrl(panel, "", WxDefaultPosition, WxDefaultSize, WindowStyles.TextCtrlProcessEnter)
            {
                Font = _dataFont
            };
            sizer.Add(_textInput, 1, SizerFlag.Expand | SizerFlag.All, 0);
            _textInput.EvtKeyDown((s, e) =>
            {
                KeyEvent keyEvent = e as KeyEvent;
                if (keyEvent != null && keyEvent.KeyCode == 13)
                {
                    OnSend();
                }
                e.Skip(true);
            });

            _send = new Button(panel, ID_Send, "Send");
            EvtButton(ID_Send, (s, e) => OnSend());
            sizer.Add(_send, 0, SizerFlag.Expand | SizerFlag.All, 0);

            panel.SetSizer(sizer);
            sizer.SetSizeHints(panel);

            return panel;
        }

        private Panel CreateControlPanel()
        {
            Panel panel = new Panel(this);
            BoxSizer sizer = new BoxSizer(Orientation.Horizontal);

            _port = new ComboBox(panel, "", WxDefaultPosition, WxDefaultSize, new string[0],
                WindowStyles.ComboBoxDropDown | WindowStyles.ComboBoxReadonly);
            UpdatePorts();
            sizer.Add(_port, 0, SizerFlag.Expand | SizerFlag.All, 5);

            Bitmap refreshBitmap = GetType().Assembly.GetManifestResourceWxBitmap("DummyLoadController.Images.refresh.png");
            _refreshPorts = new BitmapButton(panel, ID_RefreshPorts, refreshBitmap);
            EvtButton(ID_RefreshPorts, (s, e) => UpdatePorts());
            sizer.Add(_refreshPorts, 0, SizerFlag.Expand | SizerFlag.All, 0);

            string[] baudRates = new[] { "300", "600", "1200", "2400", "4800", "9600", "14,400", "19,200", "28,800", "38,400", "57,600", "76,800", "115,200", "230,400" };
            _baudRate = new ComboBox(panel, "", WxDefaultPosition, WxDefaultSize, baudRates, WindowStyles.ComboBoxDropDown | WindowStyles.ComboBoxReadonly);
            _baudRate.Select(5);
            sizer.Add(_baudRate, 0, SizerFlag.Expand | SizerFlag.All, 5);

            _connect = new Button(panel, ID_Connect, "Connect");
            EvtButton(ID_Connect, (s, e) => Connect());
            sizer.Add(_connect, 0, SizerFlag.Expand | SizerFlag.All, 5);

            _disconnect = new Button(panel, ID_Disconnect, "Disconnect");
            EvtButton(ID_Disconnect, (s, e) => _comService.Disconnect());
            sizer.Add(_disconnect, 0, SizerFlag.Expand | SizerFlag.All, 5);

            panel.SetSizer(sizer);
            sizer.SetSizeHints(panel);

            return panel;
        }

        private void Connect()
        {
            if (_port.Selection == -1)
            {
                MessageDialog.ShowModal(this, "Port not selected.", "Invalid Port", WindowStyles.IconExclamation);
                return;
            }
            string portName = _port.StringSelection;
            int baudRate = GetSelectedBaudRate();
            _comService.Connect(portName, baudRate, Parity.None, 8, StopBits.One);
        }

        private int GetSelectedBaudRate()
        {
            return int.Parse(_baudRate.StringSelection.Replace(",", ""));
        }

        private void UpdatePorts()
        {
            _port.Clear();
            foreach (ComService.Port port in _comService.GetPorts().OrderBy(s => s.Name))
            {
                _port.Insert(port.Name, _port.Count);
            }
        }

        private void OnClose()
        {
            _comService.DataArrived -= ComDataArrived;
        }

        private void OnSend()
        {
            try
            {
                AppendHistory(_textInput.Value + "\r\n");
                _comService.Send(_textInput.Value + "\n");
                _textInput.Value = "";
            }
            catch (Exception ex)
            {
                MessageDialog.ShowModal(this, ex.Message, "Error", WindowStyles.IconExclamation);
            }
        }

        private void AppendHistory(string str)
        {
            _history.Value += str;
            _history.ScrollLines(10);
        }

        private void ComDataArrived(object sender, ComService.DataArrivedArgs args)
        {
            if (args.PacketType != ComService.PacketType.Controller)
            {
                AppendHistory(BytesToString(args.Data));
            }
        }

        private void ComStatusChanged(object sender, ComService.StatusChangedArgs args)
        {
            if (args.Status == ComService.Status.Connecting || args.Status == ComService.Status.Connected)
            {
                _connect.Enabled = false;
                _port.Enabled = false;
                _baudRate.Enabled = false;
                _disconnect.Enabled = true;
                _textInput.Enabled = true;
                _send.Enabled = true;
            }
            else if (args.Status == ComService.Status.Disconnected)
            {
                DisableInput();
            }
            else
            {
                throw new Exception("Unhandled status change");
            }
        }

        private void DisableInput()
        {
            _connect.Enabled = true;
            _port.Enabled = true;
            _baudRate.Enabled = true;
            _disconnect.Enabled = false;
            _textInput.Enabled = false;
            _send.Enabled = false;
        }

        private string BytesToString(byte[] data)
        {
            return Encoding.ASCII.GetString(data);
        }

        public void SaveSettings(XElement elem)
        {
            if (_port.Selection >= 0)
            {
                string portName = _port.StringSelection;
                elem.Add(new XElement("PortName", new XText(portName)));
            }
            if (_baudRate.Selection >= 0)
            {
                int baudRate = GetSelectedBaudRate();
                elem.Add(new XElement("BaudRate", new XText(baudRate.ToString())));
            }
        }

        public void LoadSettings(XElement elem)
        {
            string portName = elem.GetElementTextOrDefault("PortName");
            if (portName != null)
            {
                _port.SetStringSelection(portName);
            }

            string baudRateStr = elem.GetElementTextOrDefault("BaudRate");
            if (baudRateStr != null)
            {
                int baudRate = int.Parse(baudRateStr);
                for (int i = 0; i < _baudRate.Count; i++)
                {
                    int itemVal = int.Parse(_baudRate.GetString(i).Replace(",", ""));
                    if (itemVal == baudRate)
                    {
                        _baudRate.Selection = i;
                        break;
                    }
                }
            }
        }
    }
}
