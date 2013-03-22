using System.Windows.Forms;
using System;

namespace FiveVolt.Lib.Helpers
{
    public static class FormHelpers
    {
        private delegate void BeginInvokeDelegate();

        public static void BeginInvoke(this Form window, Action action)
        {
            if (window.IsDisposed || !window.IsHandleCreated)
            {
                return;
            }
            window.BeginInvoke(new BeginInvokeDelegate(action));
        }
    }
}
