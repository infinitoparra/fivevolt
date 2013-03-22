using System.ComponentModel;

namespace Quadcopter
{
    public class PidObj : INotifyPropertyChanged
    {
        private double _p, _i, _d;
        public event PropertyChangedEventHandler PropertyChanged;

        public double P
        {
            get { return _p; }
            set { _p = value; OnPropertyChanged("P"); }
        }

        public double I
        {
            get { return _i; }
            set { _i = value; OnPropertyChanged("I"); }
        }

        public double D
        {
            get { return _d; }
            set { _d = value; OnPropertyChanged("D"); }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
