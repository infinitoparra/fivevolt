using System;
using System.Drawing;

namespace HeadsUpDisplay.Helpers
{
    public static class MathHelper
    {
        public static Point RotatePoint(Point center, double angle, Point pt)
        {
            int x = pt.X - center.X;
            int y = pt.Y - center.Y;
            double r1 = Math.Sqrt(x * x + y * y);
            double t1 = (x == 0 && y == 0) ? 0 : (x >= 0 ? Math.Asin(y / r1) : -Math.Asin(y / r1) + Math.PI);
            int cx = (int)(r1 * Math.Cos(t1 + angle)) + center.X;
            int cy = (int)(r1 * Math.Sin(t1 + angle)) + center.Y;
            return new Point(cx, cy);
        }

        public static double Rad2Deg(double rad)
        {
            return rad * 180.0 / Math.PI;
        }

        public static double Deg2Rad(double deg)
        {
            return deg * Math.PI / 180.0;
        }

        public static double Clamp(double x, double min, double max)
        {
            return Math.Min(Math.Max(x, min), max);
        }
    }
}
