using System;
using System.Windows;
using System.Windows.Controls;

namespace Quadcopter
{
    /// <summary>
    /// Interaction logic for SliderValue.xaml
    /// </summary>
    public partial class SliderValue : UserControl
    {
        private bool _inUpdate;

        public delegate void ValueChangedDelegate(object sender, double newValue);
        public event ValueChangedDelegate ValueChanged;

        public SliderValue()
        {
            InitializeComponent();
            Value = 0.0;
        }

        public double Value
        {
            get { return _sliderValue.Value; }
            set
            {
                if (double.IsNaN(value))
                {
                    _textValue.Text = "0.0";
                    _sliderValue.Value = 0.0;
                }
                else
                {
                    _textValue.Text = value.ToString("0.00");
                    _sliderValue.Value = value;
                }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_inUpdate)
            {
                return;
            }
            try
            {
                _inUpdate = true;
                _textValue.Text = e.NewValue.ToString("0.00");
            }
            finally
            {
                _inUpdate = false;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_inUpdate)
            {
                return;
            }
            try
            {
                _inUpdate = true;
                double result;
                if (double.TryParse(_textValue.Text, out result) && !double.IsNaN(result))
                {
                    _sliderValue.Value = result;
                }
            }
            finally
            {
                _inUpdate = false;
            }
        }

        private void OnValueChanged(double value)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, value);
            }
        }

        private void _ok_Click(object sender, RoutedEventArgs e)
        {
            OnValueChanged(_sliderValue.Value);
        }
    }
}
