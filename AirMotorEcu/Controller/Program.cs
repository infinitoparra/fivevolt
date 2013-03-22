using System;
using System.Windows.Forms;
using AirMotorEcuController.Services;

namespace AirMotorEcuController
{
    static class Program
    {
        private static readonly ComService _comService = new ComService();

        public static ComService ComService
        {
            get { return _comService; }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
