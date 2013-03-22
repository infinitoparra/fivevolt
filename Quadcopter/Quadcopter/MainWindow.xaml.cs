using System;
using System.Windows;
using System.Windows.Controls;

namespace Quadcopter
{
    public partial class MainWindow : Window
    {
        private ComLink _comLink;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _comLink = new ComLink();
            _hud.ComLink = _comLink;
            _raw.ComLink = _comLink;
            _graphing.ComLink = _comLink;
            _comLink.Open();

            DataContext = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_comLink != null)
            {
                _comLink.Close();
            }
        }

        private void _tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _hud.IsEnabled = false;
            _raw.IsEnabled = false;
            _graphing.IsEnabled = false;

            switch ((string)((TabItem)_tabs.SelectedItem).Header)
            {
                case "HUD":
                    _hud.IsEnabled = true;
                    break;
                case "Raw":
                    _raw.IsEnabled = true;
                    break;
                case "Graphing":
                    _graphing.IsEnabled = true;
                    break;
            }
        }
    }
}
