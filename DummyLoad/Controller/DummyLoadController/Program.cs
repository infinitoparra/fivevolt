using System;
using System.Windows.Forms;

namespace DummyLoadController
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DummyLoadControllerApp app = new DummyLoadControllerApp();
            app.Run();
        }
    }
}
