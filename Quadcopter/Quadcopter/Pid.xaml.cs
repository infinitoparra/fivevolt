using System;
using System.Windows;
using System.Windows.Controls;

namespace Quadcopter
{
    /// <summary>
    /// Interaction logic for Pid.xaml
    /// </summary>
    public partial class Pid : UserControl
    {
        public delegate void ValueChangedDelegate(object sender);
        public event ValueChangedDelegate ValueChanged;

        public Pid()
        {
            InitializeComponent();
            _p.ValueChanged += PidValueChanged;
            _i.ValueChanged += PidValueChanged;
            _d.ValueChanged += PidValueChanged;
        }

        private void PidValueChanged(object sender, double newValue)
        {
            OnValueChanged();
        }

        private void OnValueChanged()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this);
            }
        }

        public void UpdateValue(PidObj pid)
        {
            _p.Value = pid.P;
            _i.Value = pid.I;
            _d.Value = pid.D;
        }

        public PidObj GetPidValue()
        {
            return new PidObj
            {
                P = _p.Value,
                I = _i.Value,
                D = _d.Value
            };
        }
    }
}
