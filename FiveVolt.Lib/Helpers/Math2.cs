using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiveVolt.Lib.Helpers
{
    public class Math2
    {
        public static double Clamp(double val, double min, double max)
        {
            return Math.Max(Math.Min(val, max), min);
        }

        public static int Clamp(int val, int min, int max)
        {
            return Math.Max(Math.Min(val, max), min);
        }
    }
}
