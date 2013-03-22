using WX;
using Font = WX.Font;
using FontFamily = WX.FontFamily;
using FontStyle = WX.FontStyle;

namespace DummyLoadController
{
    public sealed class ValueDisplay : Panel
    {
        private readonly StaticText _text;
        private readonly StaticText _unit;

        public ValueDisplay(Window parent)
            : base(parent, WxDefaultPosition, WxDefaultSize, WindowStyles.BorderSimple)
        {
            BackgroundColour = Colour.WhiteColour;

            var sizer = new FlexGridSizer(2, 5, 5);

            Font displayFont = new Font(30, FontFamily.wxDefault, FontStyle.Normal, FontWeight.Normal);

            _text = new StaticText(this, "10.000", WxDefaultPosition, WxDefaultSize, WindowStyles.AlignRight | WindowStyles.StaticTextNoAutoResize);
            _text.SetFont(displayFont);
            sizer.Add(_text);

            _unit = new StaticText(this, "mV");
            sizer.Add(_unit, 1, SizerFlag.AlignBottom | SizerFlag.AlignRight | SizerFlag.Right | SizerFlag.Bottom, 5);

            SetSizer(sizer);
            sizer.SetSizeHints(this);
        }

        public string Value
        {
            get { return _text.Title; }
            set { _text.Title = value; }
        }

        public string Unit
        {
            get { return _unit.Title; }
            set { _unit.Title = value; }
        }
    }
}
