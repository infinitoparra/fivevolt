using System.Drawing;
using DummyLoadController.Services;
using WX;
using System.Xml.Linq;

namespace DummyLoadController
{
    public class FrontPanelPanel : Panel
    {
        private readonly ComService _comService;
        private const int ID_TargetCurrentSet = 1;
        private readonly ValueDisplay _externalVoltage;
        private readonly ValueDisplay _externalCurrent;
        private readonly ValueDisplay _loadVoltage;
        private readonly ValueDisplay _loadCurrent;
        private readonly TextCtrl _targetCurrent;
        private readonly Button _targetCurrentSet;
        private readonly ValueConverterService _valueConverterService = new ValueConverterService();
        private bool _targetCurrentHasFocus;

        public FrontPanelPanel(MainFrame parent, ComService comService)
            : base(parent)
        {
            _comService = comService;
            _comService.DataArrived += ComDataArrived;
            FlexGridSizer sizer = new FlexGridSizer(4, 0, 0);

            sizer.Add(new StaticText(this, "External\nVoltage", WxDefaultPosition, WxDefaultSize, WindowStyles.AlignRight), 0, SizerFlag.AlignCenterVertical | SizerFlag.All, 5);
            _externalVoltage = new ValueDisplay(this) { Value = "0.0", Unit = "mV" };
            sizer.Add(_externalVoltage, 0, SizerFlag.All, 5);

            sizer.Add(new StaticText(this, "Load\nVoltage", WxDefaultPosition, WxDefaultSize, WindowStyles.AlignRight), 0, SizerFlag.AlignCenterVertical | SizerFlag.All, 5);
            _loadVoltage = new ValueDisplay(this) { Value = "0.0", Unit = "mV" };
            sizer.Add(_loadVoltage, 0, SizerFlag.All, 5);

            sizer.Add(new StaticText(this, "External\nCurrent", WxDefaultPosition, WxDefaultSize, WindowStyles.AlignRight), 0, SizerFlag.AlignCenterVertical | SizerFlag.All, 5);
            _externalCurrent = new ValueDisplay(this) { Value = "0.0", Unit = "mA" };
            sizer.Add(_externalCurrent, 0, SizerFlag.All, 5);

            sizer.Add(new StaticText(this, "Load\nCurrent", WxDefaultPosition, WxDefaultSize, WindowStyles.AlignRight), 0, SizerFlag.AlignCenterVertical | SizerFlag.All, 5);
            _loadCurrent = new ValueDisplay(this) { Value = "0.0", Unit = "mV" };
            sizer.Add(_loadCurrent, 0, SizerFlag.All, 5);

            sizer.Add(0, 0);
            sizer.Add(0, 0);

            sizer.Add(new StaticText(this, "Target\nCurrent", WxDefaultPosition, WxDefaultSize, WindowStyles.AlignRight), 0, SizerFlag.AlignCenterVertical | SizerFlag.All, 5);
            Panel targetCurrentPanel = new Panel(this);
            var targetCurrentSizer = new FlexGridSizer(2, 5, 5);
            _targetCurrentSet = new Button(targetCurrentPanel, ID_TargetCurrentSet, "Set");
            EvtButton(ID_TargetCurrentSet, (s, e) => SetTargetCurrent());
            _targetCurrent = new TextCtrl(targetCurrentPanel, "0.0mA", WxDefaultPosition, new Size(70, _targetCurrentSet.Size.Height));
            targetCurrentSizer.Add(_targetCurrent);
            targetCurrentSizer.Add(_targetCurrentSet);
            targetCurrentPanel.SetSizer(targetCurrentSizer);
            targetCurrentSizer.SetSizeHints(targetCurrentPanel);
            sizer.Add(targetCurrentPanel, 0, SizerFlag.All, 5);

            SetSizer(sizer);
            sizer.SetSizeHints(this);

            EvtDestroy((s, e) => { OnDestory(); e.Skip(true); });
            _targetCurrent.EvtSetFocus((s, e) => _targetCurrentHasFocus = true);
            _targetCurrent.EvtKillFocus((s, e) => _targetCurrentHasFocus = false);
        }

        private void OnDestory()
        {
            _comService.DataArrived -= ComDataArrived;
        }

        private void ComDataArrived(object sender, ComService.DataArrivedArgs e)
        {
            if (e.PacketType != ComService.PacketType.Controller)
            {
                return;
            }

            ComService.ControllerData controllerData = _comService.ParseControllerData(e.Data);
            SetValue(_loadCurrent, controllerData.LoadCurrentSense, "A");
            SetValue(_loadVoltage, controllerData.LoadVoltage, "V");
            SetValue(_externalCurrent, controllerData.ExternalCurrent, "A");
            SetValue(_externalVoltage, controllerData.ExternalVoltage, "V");
            if (!_targetCurrentHasFocus)
            {
                _targetCurrent.Value = _valueConverterService.ToValueWithUnits(controllerData.LoadCurrentTarget, "A");
            }
        }

        private void SetValue(ValueDisplay valueDisplay, double val, string unitSuffix)
        {
            if (val > 1.0)
            {
                valueDisplay.Value = val.ToString("0.000");
                valueDisplay.Unit = unitSuffix;
            }
            else
            {
                valueDisplay.Value = (val * 1000.0).ToString("##0");
                valueDisplay.Unit = "m" + unitSuffix;
            }
        }

        private void SetTargetCurrent()
        {
            double targetCurrent;
            _valueConverterService.TryParseValueWithUnits(_targetCurrent.Value, out targetCurrent);
            _targetCurrent.Value = _valueConverterService.ToValueWithUnits(targetCurrent, "A");
            _comService.SetTargetCurrent(targetCurrent);
        }

        public void SaveSettings(XElement elem)
        {
        }

        public void LoadSettings(XElement elem)
        {
        }
    }
}
