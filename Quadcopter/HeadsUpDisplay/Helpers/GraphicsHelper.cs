using System.Drawing;

namespace HeadsUpDisplay.Helpers
{
    public static class GraphicsHelper
    {
        public static void DrawLine(this Graphics g, Pen pen, Point center, double angle, Point pt1, Point pt2)
        {
            Point newPt1 = new Point(pt1.X, pt1.Y);
            Point newPt2 = new Point(pt2.X, pt2.Y);
            newPt1.Offset(-center.X, -center.Y);
            newPt2.Offset(-center.X, -center.Y);
            g.ResetTransform();
            g.TranslateTransform(g.ClipBounds.Left + center.X, g.ClipBounds.Top + center.Y);
            g.RotateTransform((float)MathHelper.Rad2Deg(angle));
            g.DrawLine(pen, newPt1, newPt2);
        }

        public static void DrawRect(this Graphics g, Pen pen, Point center, double angle, Rectangle rect)
        {
            DrawLine(g, pen, center, angle, new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Top));
            DrawLine(g, pen, center, angle, new Point(rect.Left, rect.Top), new Point(rect.Left, rect.Bottom));
            DrawLine(g, pen, center, angle, new Point(rect.Right, rect.Top), new Point(rect.Right, rect.Bottom));
            DrawLine(g, pen, center, angle, new Point(rect.Left, rect.Bottom), new Point(rect.Right, rect.Bottom));
        }

        public static void FillRect(this Graphics g, Brush brush, Point center, double angle, Rectangle rect)
        {
            Rectangle newRect = new Rectangle(rect.X - center.X, rect.Y - center.Y, rect.Width, rect.Height);
            g.ResetTransform();
            g.TranslateTransform(g.ClipBounds.Left + center.X, g.ClipBounds.Top + center.Y);
            g.RotateTransform((float)MathHelper.Rad2Deg(angle));
            g.FillRectangle(brush, newRect);
        }

        public static void DrawEllipse(this Graphics g, Pen pen, Point center, double angle, Rectangle rect)
        {
            Rectangle newRect = new Rectangle(rect.X - center.X, rect.Y - center.Y, rect.Width, rect.Height);
            g.ResetTransform();
            g.TranslateTransform(g.ClipBounds.Left + center.X, g.ClipBounds.Top + center.Y);
            g.RotateTransform((float)MathHelper.Rad2Deg(angle));
            g.DrawEllipse(pen, newRect);
        }

        public static void DrawString(this Graphics g, string str, Font font, Brush brush, Point center, double angle, Point pt, StringFormat stringFormat)
        {
            Point newPt = new Point(pt.X, pt.Y);
            newPt.Offset(-center.X, -center.Y);
            g.ResetTransform();
            g.TranslateTransform(g.ClipBounds.Left + center.X, g.ClipBounds.Top + center.Y);
            g.RotateTransform((float)MathHelper.Rad2Deg(angle));
            g.DrawString(str, font, brush, newPt, stringFormat);
        }
    }
}
