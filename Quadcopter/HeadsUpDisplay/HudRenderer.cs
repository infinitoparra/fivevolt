using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using HeadsUpDisplay.Helpers;

namespace HeadsUpDisplay
{
    public class HudRenderer
    {
        private Pen _pen;
        private Pen _penDash;
        private Font _font;
        private Color _lineColor = Color.Green;
        private Color _warningColor = Color.Red;
        private float _lineThickness = 1;
        private Rectangle _rect;
        private Rectangle _rollRect;
        private Rectangle _virtualHorzRect;
        private Rectangle _airspeedRect;
        private Rectangle _altitudeRect;
        private Rectangle _compassRect;
        private Rectangle _destinationRect;
        private Rectangle _aux1Rect;
        private Rectangle _aux2Rect;
        private Rectangle _aux3Rect;
        private Rectangle _aux4Rect;
        private Brush _brush;
        private Brush _brushWarning;
        private Brush _skyBrush;
        private Brush _groundBrush;
        private readonly double _angle45 = MathHelper.Deg2Rad(45.0);
        private readonly double _angle90 = MathHelper.Deg2Rad(90.0);
        private readonly double _angle180 = MathHelper.Deg2Rad(180.0);
        private readonly double _angle270 = MathHelper.Deg2Rad(270.0);
        private readonly double _angle360 = MathHelper.Deg2Rad(360.0);

        public HudRenderer()
        {
            RecreatePen();
            RecreateFont();
            RecreateBrush();
            ShowRoll = true;
            ShowCompass = true;
            ShowVirtualHorizon = true;
            ShowDestination = true;
            ShowBackground = true;
            Aux1 = HudPanel.NumericReadout;
            Aux2 = HudPanel.UserInput;
            Aux3 = HudPanel.Battery;
            Aux4 = HudPanel.Quadcopter;
            Roll = 0.0;
            Pitch = 0.0;
            Yaw = 0.0;
            BatteryLevel = 0.0;
            BatteryWarningLevel = 0.25;
            QuadcopterFrontLevel = 0.5;
            QuadcopterBackLevel = 0.5;
            QuadcopterLeftLevel = 0.5;
            QuadcopterRightLevel = 0.5;
        }

        public Color LineColor
        {
            set { _lineColor = value; RecreatePen(); RecreateBrush(); }
            get { return _lineColor; }
        }

        public Color WarningColor
        {
            set { _warningColor = value; RecreatePen(); RecreateBrush(); }
            get { return _warningColor; }
        }

        public float LineThickness
        {
            set { _lineThickness = value; RecreatePen(); }
            get { return _lineThickness; }
        }

        public bool ShowBackground { get; set; }
        public bool ShowRoll { get; set; }
        public bool ShowCompass { get; set; }
        public bool ShowVirtualHorizon { get; set; }
        public bool ShowDestination { get; set; }
        public HudPanel Aux1 { get; set; }
        public HudPanel Aux2 { get; set; }
        public HudPanel Aux3 { get; set; }
        public HudPanel Aux4 { get; set; }

        public double Roll { get; set; }
        public double Pitch { get; set; }
        public double Yaw { get; set; }
        public double BatteryLevel { get; set; }
        public double BatteryWarningLevel { get; set; }
        public double QuadcopterFrontLevel { get; set; }
        public double QuadcopterBackLevel { get; set; }
        public double QuadcopterLeftLevel { get; set; }
        public double QuadcopterRightLevel { get; set; }
        public double UserInputRoll { get; set; }
        public double UserInputPitch { get; set; }
        public double UserInputYaw { get; set; }
        public double UserInputElevation { get; set; }

        private void RecreatePen()
        {
            if (_pen != null)
            {
                _pen.Dispose();
            }
            _pen = new Pen(LineColor, _lineThickness);

            if (_penDash != null)
            {
                _penDash.Dispose();
            }
            _penDash = new Pen(LineColor, _lineThickness)
            {
                DashStyle = DashStyle.Custom,
                DashPattern = new float[] { 10f, 10f }
            };
        }

        private void RecreateFont()
        {
            if (_font != null)
            {
                _font.Dispose();
            }
            _font = new Font("PF Tempesta Seven", 12.0f, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        private void RecreateBrush()
        {
            if (_brush != null)
            {
                _brush.Dispose();
            }
            _brush = new SolidBrush(LineColor);

            if (_brushWarning != null)
            {
                _brushWarning.Dispose();
            }
            _brushWarning = new SolidBrush(WarningColor);

            if (_skyBrush != null)
            {
                _skyBrush.Dispose();
            }
            _skyBrush = new SolidBrush(Color.FromArgb(140, 240, 255));

            if (_groundBrush != null)
            {
                _groundBrush.Dispose();
            }
            _groundBrush = new SolidBrush(Color.FromArgb(175, 109, 8));
        }

        public void Render(Graphics g, int width, int height)
        {
            g.Clear(Color.Transparent);
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            UpdateRects(g, width, height);
            if (ShowBackground)
            {
                DrawBackground(g);
            }
            if (ShowRoll)
            {
                DrawRoll(g);
            }
            if (ShowCompass)
            {
                DrawCompass(g);
            }
            if (ShowVirtualHorizon)
            {
                DrawVirtualHorz(g);
            }
            if (ShowDestination)
            {
                DrawDestination(g);
            }
            DrawAux(g, Aux1, _aux1Rect);
            DrawAux(g, Aux2, _aux2Rect);
            DrawAux(g, Aux3, _aux3Rect);
            DrawAux(g, Aux4, _aux4Rect);
        }

        private void UpdateRects(Graphics g, int width, int height)
        {
            _rect = new Rectangle(0, 0, width, height);

            _airspeedRect = new Rectangle(0, (int)(height / 6.0), (int)(width / 5.0), (int)(height - (2.0 * height / 6.0)));
            _altitudeRect = new Rectangle(width - _airspeedRect.Width, _airspeedRect.Top, _airspeedRect.Width, _airspeedRect.Height);

            _virtualHorzRect = new Rectangle(_airspeedRect.Right, (int)(height / 6.0), _altitudeRect.Left - _airspeedRect.Right, (int)(height - (2.0 * height / 6.0)));
            _rollRect = new Rectangle(_airspeedRect.Right, 0, _virtualHorzRect.Width, _virtualHorzRect.Top);
            _compassRect = new Rectangle(_virtualHorzRect.Left, _virtualHorzRect.Bottom, _virtualHorzRect.Width, height - _virtualHorzRect.Bottom);

            _aux1Rect = new Rectangle(_airspeedRect.Left, 0, _airspeedRect.Width, _airspeedRect.Top);
            _aux1Rect.Inflate(-5, -5);
            _aux2Rect = new Rectangle(_altitudeRect.Left, 0, _altitudeRect.Width, _altitudeRect.Top);
            _aux2Rect.Inflate(-5, -5);
            _aux3Rect = new Rectangle(_airspeedRect.Left, _airspeedRect.Bottom, _airspeedRect.Width, height - _airspeedRect.Bottom);
            _aux3Rect.Inflate(-5, -5);
            _aux4Rect = new Rectangle(_altitudeRect.Left, _altitudeRect.Bottom, _altitudeRect.Width, height - _altitudeRect.Bottom);
            _aux4Rect.Inflate(-5, -5);

            int destSize = Math.Min(width / 20, height / 20);
            _destinationRect = new Rectangle((width - destSize) / 2, (height - destSize) / 2, destSize, destSize);
        }

        private void DrawAux(Graphics g, HudPanel panel, Rectangle rect)
        {
            switch (panel)
            {
                case HudPanel.NumericReadout:
                    DrawNumericReadout(g, rect);
                    break;
                case HudPanel.Battery:
                    DrawBattery(g, rect);
                    break;
                case HudPanel.Quadcopter:
                    DrawQuadcopter(g, rect);
                    break;
                case HudPanel.UserInput:
                    DrawUserInput(g, rect);
                    break;
            }
        }

        private void DrawQuadcopter(Graphics g, Rectangle rect)
        {
            g.ResetClip();
            g.ResetTransform();
            g.SetClip(rect);
            Point center = new Point(rect.Width / 2, rect.Height / 2);
            int offset = Math.Min(rect.Width / 4, rect.Height / 4);
            int bladeSize = (int)(offset / Math.Sin(_angle45));
            int bladeSizeHalf = bladeSize / 2;

            //g.DrawLine(_pen, center, 0.0, new Point(0, 0), new Point(rect.Width, rect.Height));

            // front
            Rectangle r = new Rectangle(rect.X + center.X - bladeSizeHalf, rect.Y + center.Y - (offset + bladeSizeHalf), bladeSize, bladeSize);
            DrawBlade(g, r, QuadcopterFrontLevel);

            // back
            r = new Rectangle(rect.X + center.X - bladeSizeHalf, rect.Y + center.Y + (offset - bladeSizeHalf), bladeSize, bladeSize);
            DrawBlade(g, r, QuadcopterBackLevel);

            // left
            r = new Rectangle(rect.X + center.X - bladeSizeHalf - offset, rect.Y + center.Y - bladeSizeHalf, bladeSize, bladeSize);
            DrawBlade(g, r, QuadcopterLeftLevel);

            // right
            r = new Rectangle(rect.X + center.X + (offset - bladeSizeHalf), rect.Y + center.Y - bladeSizeHalf, bladeSize, bladeSize);
            DrawBlade(g, r, QuadcopterRightLevel);
        }

        private void DrawBlade(Graphics g, Rectangle rect, double level)
        {
            g.ResetClip();
            g.ResetTransform();
            g.SetClip(rect);
            Point center = new Point(rect.Width / 2, rect.Height / 2);
            int barWidth = rect.Width / 5;
            int barHeight = (int)(rect.Width * 0.7);
            int radius = rect.Width / 2;
            double barOffset = Math.Cos(Math.Asin((barHeight / 2.0) / radius)) * radius;
            if (double.IsNaN(barOffset))
            {
                barOffset = 0;
            }

            // circle
            g.DrawEllipse(_pen, center, 0.0, new Rectangle(0, 0, rect.Width - 1, rect.Height - 1));

            // bar
            int fill = barHeight - (int)(barHeight * level);
            g.FillRect(_brush, center, 0.0, new Rectangle((int)(center.X - barOffset), (rect.Height - barHeight) / 2 + fill, barWidth, barHeight - fill));
            g.DrawRect(_pen, center, 0.0, new Rectangle((int)(center.X - barOffset), (rect.Height - barHeight) / 2, barWidth, barHeight));

            // text
            StringFormat stringFormat = new StringFormat
            {
                LineAlignment = StringAlignment.Center
            };
            g.DrawString((level * 100).ToString("00"), _font, _brush, center, 0.0, new Point((int)(center.X - barOffset + barWidth + 5), center.Y), stringFormat);
        }

        private void DrawBattery(Graphics g, Rectangle rect)
        {
            g.ResetClip();
            g.ResetTransform();
            g.SetClip(rect);
            Point center = new Point(0, 0);
            int batteryTopHeight = rect.Height / 20;
            int batteryHeight = rect.Height - batteryTopHeight - 1;
            int batteryWidth = batteryHeight / 3;
            int batteryTopWidth = batteryWidth / 3;
            bool warning = BatteryLevel <= BatteryWarningLevel;

            // draw fill/level
            double batteryLevel = MathHelper.Clamp(BatteryLevel, 0.0, 1.0);
            int fill = (int)(batteryHeight * Math.Max(0.05, batteryLevel));
            g.FillRect(warning ? _brushWarning : _brush, center, 0.0, new Rectangle(0, batteryHeight - fill + batteryTopHeight, batteryWidth, fill));

            // draw outline
            g.DrawRect(_pen, center, 0.0, new Rectangle((batteryWidth - batteryTopWidth) / 2, 0, batteryTopWidth, batteryTopHeight));
            g.DrawRect(_pen, center, 0.0, new Rectangle(0, batteryTopHeight, batteryWidth, batteryHeight));

            // draw value
            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Far
            };
            g.DrawString((BatteryLevel * 100.0).ToString("000"), _font, warning ? _brushWarning : _brush, center, 0.0, new Point(batteryWidth + 5, rect.Height), stringFormat);
        }

        private void DrawNumericReadout(Graphics g, Rectangle rect)
        {
            g.ResetClip();
            g.ResetTransform();
            g.SetClip(rect);
            StringFormat stringFormatName = new StringFormat
            {
                Alignment = StringAlignment.Far
            };
            StringFormat stringFormatNumber = new StringFormat();
            Point center = new Point(0, 0);
            SizeF fontMetrics = g.MeasureString("PITCH:W", _font, 1000, stringFormatNumber);

            g.DrawString("ROLL:", _font, _brush, center, 0.0, new Point((int)fontMetrics.Width, (int)(0 * fontMetrics.Height)), stringFormatName);
            g.DrawString(MathHelper.Rad2Deg(Roll).ToString("000"), _font, _brush, center, 0.0, new Point((int)fontMetrics.Width, (int)(0 * fontMetrics.Height)), stringFormatNumber);
            g.DrawString("PITCH:", _font, _brush, center, 0.0, new Point((int)fontMetrics.Width, (int)(1 * fontMetrics.Height)), stringFormatName);
            g.DrawString(MathHelper.Rad2Deg(Pitch).ToString("000"), _font, _brush, center, 0.0, new Point((int)fontMetrics.Width, (int)(1 * fontMetrics.Height)), stringFormatNumber);
            g.DrawString("YAW:", _font, _brush, center, 0.0, new Point((int)fontMetrics.Width, (int)(2 * fontMetrics.Height)), stringFormatName);
            g.DrawString(MathHelper.Rad2Deg(Yaw).ToString("000"), _font, _brush, center, 0.0, new Point((int)fontMetrics.Width, (int)(2 * fontMetrics.Height)), stringFormatNumber);
        }

        private void DrawUserInput(Graphics g, Rectangle rect)
        {
            g.ResetClip();
            g.ResetTransform();
            g.SetClip(rect);
            StringFormat stringFormatName = new StringFormat
            {
                Alignment = StringAlignment.Far
            };
            StringFormat stringFormatNumber = new StringFormat();
            Point center = new Point(0, 0);
            SizeF fontMetrics = g.MeasureString("PITCH:W", _font, 1000, stringFormatNumber);

            g.DrawString("ROLL:", _font, _brush, center, 0.0, new Point((int)fontMetrics.Width, (int)(0 * fontMetrics.Height)), stringFormatName);
            g.DrawString(UserInputRoll.ToString("000"), _font, _brush, center, 0.0, new Point((int)fontMetrics.Width, (int)(0 * fontMetrics.Height)), stringFormatNumber);
            g.DrawString("PITCH:", _font, _brush, center, 0.0, new Point((int)fontMetrics.Width, (int)(1 * fontMetrics.Height)), stringFormatName);
            g.DrawString(UserInputPitch.ToString("000"), _font, _brush, center, 0.0, new Point((int)fontMetrics.Width, (int)(1 * fontMetrics.Height)), stringFormatNumber);
            g.DrawString("YAW:", _font, _brush, center, 0.0, new Point((int)fontMetrics.Width, (int)(2 * fontMetrics.Height)), stringFormatName);
            g.DrawString(UserInputYaw.ToString("000"), _font, _brush, center, 0.0, new Point((int)fontMetrics.Width, (int)(2 * fontMetrics.Height)), stringFormatNumber);
            g.DrawString("ELEV:", _font, _brush, center, 0.0, new Point((int)fontMetrics.Width, (int)(3 * fontMetrics.Height)), stringFormatName);
            g.DrawString(UserInputElevation.ToString("000"), _font, _brush, center, 0.0, new Point((int)fontMetrics.Width, (int)(3 * fontMetrics.Height)), stringFormatNumber);
        }

        private void DrawBackground(Graphics g)
        {
            g.ResetClip();
            g.ResetTransform();
            g.SetClip(_rect);
            Point center = new Point(_rect.Width / 2, _rect.Height / 2);
            double roll = Roll;
            double scale = 12;
            double pitch = MathHelper.Rad2Deg(Pitch) * scale;
            int y = (int)(center.Y + pitch);
            g.FillRect(_skyBrush, center, roll, new Rectangle(0, 0, _rect.Width, y));
            g.FillRect(_groundBrush, center, roll, new Rectangle(0, y, _rect.Width, _rect.Height - y));
        }

        private void DrawVirtualHorz(Graphics g)
        {
            g.ResetClip();
            g.ResetTransform();
            g.SetClip(_virtualHorzRect);
            Point center = new Point(_virtualHorzRect.Width / 2, _virtualHorzRect.Height / 2);
            double roll = Roll;
            int nonZeroLeft = _virtualHorzRect.Width / 5;
            int nonZeroRight = _virtualHorzRect.Width - nonZeroLeft;
            int centerLeft = center.X - _destinationRect.Width / 2;
            int centerRight = center.X + _destinationRect.Width / 2;
            int sideTickLength = _virtualHorzRect.Height / 30;
            double scale = 12;
            double pitch = MathHelper.Rad2Deg(Pitch) * scale;
            StringFormat stringFormat = new StringFormat();

            // draw horizon
            int y = (int)(center.Y + pitch);
            g.DrawLine(_pen, center, roll, new Point(-_virtualHorzRect.Width, y), new Point(centerLeft, y));
            g.DrawLine(_pen, center, roll, new Point(centerRight, y), new Point(_virtualHorzRect.Width * 2, y));

            // draw positive
            stringFormat.LineAlignment = StringAlignment.Far;
            for (int angle = 10; angle < 180; angle += 10)
            {
                y = (int)(center.Y - (angle * scale - pitch));
                g.DrawLine(_pen, center, roll, new Point(nonZeroLeft, y), new Point(centerLeft, y));
                g.DrawLine(_pen, center, roll, new Point(nonZeroLeft, y), new Point(nonZeroLeft, y + sideTickLength));
                g.DrawLine(_pen, center, roll, new Point(centerRight, y), new Point(nonZeroRight, y));
                g.DrawLine(_pen, center, roll, new Point(nonZeroRight, y), new Point(nonZeroRight, y + sideTickLength));
                stringFormat.Alignment = StringAlignment.Near;
                g.DrawString(angle.ToString(), _font, _brush, center, roll, new Point(nonZeroLeft, y), stringFormat);
                stringFormat.Alignment = StringAlignment.Far;
                g.DrawString(angle.ToString(), _font, _brush, center, roll, new Point(nonZeroRight, y), stringFormat);
            }

            // draw negative
            stringFormat.LineAlignment = StringAlignment.Near;
            for (int angle = -10; angle > -180; angle -= 10)
            {
                y = (int)(center.Y - (angle * scale - pitch));
                g.DrawLine(_penDash, center, roll, new Point(nonZeroLeft, y), new Point(centerLeft, y));
                g.DrawLine(_penDash, center, roll, new Point(nonZeroLeft, y), new Point(nonZeroLeft, y - sideTickLength));
                g.DrawLine(_penDash, center, roll, new Point(centerRight, y), new Point(nonZeroRight, y));
                g.DrawLine(_penDash, center, roll, new Point(nonZeroRight, y), new Point(nonZeroRight, y - sideTickLength));
                stringFormat.Alignment = StringAlignment.Near;
                g.DrawString(angle.ToString(), _font, _brush, center, roll, new Point(nonZeroLeft, y), stringFormat);
                stringFormat.Alignment = StringAlignment.Far;
                g.DrawString(angle.ToString(), _font, _brush, center, roll, new Point(nonZeroRight, y), stringFormat);
            }
        }

        private void DrawDestination(Graphics g)
        {
            g.ResetClip();
            g.ResetTransform();

            int tickLength = _destinationRect.Width / 4;
            int midX = _destinationRect.Left + (_destinationRect.Width / 2);
            int midY = _destinationRect.Top + (_destinationRect.Height / 2);

            g.DrawEllipse(_pen, _destinationRect.Left + tickLength, _destinationRect.Top + tickLength, _destinationRect.Width - (2 * tickLength), _destinationRect.Height - (2 * tickLength));
            g.DrawLine(_pen, _destinationRect.Left, midY, _destinationRect.Left + tickLength, midY);
            g.DrawLine(_pen, _destinationRect.Right - tickLength, midY, _destinationRect.Right, midY);
            g.DrawLine(_pen, midX, _destinationRect.Top, midX, _destinationRect.Top + tickLength);
        }

        private void DrawCompass(Graphics g)
        {
            g.ResetClip();
            g.ResetTransform();
            g.SetClip(_compassRect);
            StringFormat stringFormatNSEW = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };
            StringFormat stringFormatNumeric = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            SizeF fontMetrics = g.MeasureString("W", _font, 1000, stringFormatNSEW);
            double tickAngle = MathHelper.Deg2Rad(10.0);
            int tickLength = (int)(_compassRect.Height / 5.0);
            Point center = new Point((int)(_compassRect.Width / 2.0), _compassRect.Height);

            // draw ticks
            double startAngle = Yaw % tickAngle;
            for (double angle = startAngle; angle < startAngle + _angle90 + tickAngle; angle += tickAngle)
            {
                g.DrawLine(_pen, center, angle, new Point(center.X, (int)fontMetrics.Height), new Point(center.X, (int)(tickLength + fontMetrics.Height)));
            }
            for (double angle = startAngle; angle > startAngle - _angle90 - tickAngle; angle -= tickAngle)
            {
                g.DrawLine(_pen, center, angle, new Point(center.X, (int)fontMetrics.Height), new Point(center.X, (int)(tickLength + fontMetrics.Height)));
            }

            // draw N,S,E,W
            g.DrawString("N", _font, _brush, center, Yaw + 0.0, new Point(center.X, 0), stringFormatNSEW);
            g.DrawString("E", _font, _brush, center, Yaw + _angle90, new Point(center.X, 0), stringFormatNSEW);
            g.DrawString("S", _font, _brush, center, Yaw + _angle180, new Point(center.X, 0), stringFormatNSEW);
            g.DrawString("W", _font, _brush, center, Yaw + _angle270, new Point(center.X, 0), stringFormatNSEW);

            // draw numeric value
            double displayYaw = Yaw < 0 ? Yaw + _angle360 : Yaw;
            int y = (int)(fontMetrics.Height + tickLength);
            g.DrawString(MathHelper.Rad2Deg(displayYaw).ToString("000"), _font, _brush, center, 0.0, new Point(center.X, y + ((_compassRect.Height - y) / 2)), stringFormatNumeric);
        }

        protected virtual void DrawRoll(Graphics g)
        {
            g.ResetClip();
            g.ResetTransform();
            g.SetClip(_rollRect);

            int majorTickLength = (int)(_rollRect.Height / 6.0);
            int minorTickLength = (int)(majorTickLength * 0.75);
            int indicatorHeight = minorTickLength;
            double angle = MathHelper.Deg2Rad(25);
            double radius = (_rollRect.Height - majorTickLength - indicatorHeight) / (1 - Math.Cos(angle));
            radius = Math.Min(_rollRect.Height * 2.0, radius);
            Point center = new Point(_rollRect.Width / 2, (int)(majorTickLength + indicatorHeight + radius));

            // major ticks
            g.DrawLine(_pen, center, 0.0, new Point(center.X, 0), new Point(center.X, majorTickLength));

            // minor ticks
            for (double a = angle; a > 0.0; a -= (angle / 4.0))
            {
                g.DrawLine(_pen, center, a, new Point(center.X, majorTickLength - minorTickLength), new Point(center.X, majorTickLength));
                g.DrawLine(_pen, center, -a, new Point(center.X, majorTickLength - minorTickLength), new Point(center.X, majorTickLength));
            }

            // indicator
            double indicatorRoll = MathHelper.Clamp(Roll, -angle, angle);
            if (Roll > angle)
            {
                g.DrawLine(_pen, center, indicatorRoll, new Point(center.X, majorTickLength + 4), new Point((int)(center.X + (indicatorHeight * 0.75)), majorTickLength + 4 + (indicatorHeight - 4) / 2));
                g.DrawLine(_pen, center, indicatorRoll, new Point(center.X, majorTickLength + indicatorHeight), new Point((int)(center.X + (indicatorHeight * 0.75)), majorTickLength + 4 + (indicatorHeight - 4) / 2));
            }
            else if (Roll < -angle)
            {
                g.DrawLine(_pen, center, indicatorRoll, new Point(center.X, majorTickLength + 4), new Point((int)(center.X - (indicatorHeight * 0.75)), majorTickLength + 4 + (indicatorHeight - 4) / 2));
                g.DrawLine(_pen, center, indicatorRoll, new Point(center.X, majorTickLength + indicatorHeight), new Point((int)(center.X - (indicatorHeight * 0.75)), majorTickLength + 4 + (indicatorHeight - 4) / 2));
            }
            else
            {
                g.DrawLine(_pen, center, indicatorRoll, new Point(center.X, majorTickLength + 4), new Point(center.X + 5, majorTickLength + indicatorHeight));
                g.DrawLine(_pen, center, indicatorRoll, new Point(center.X, majorTickLength + 4), new Point(center.X - 5, majorTickLength + indicatorHeight));
                g.DrawLine(_pen, center, indicatorRoll, new Point(center.X - 5, majorTickLength + indicatorHeight), new Point(center.X + 5, majorTickLength + indicatorHeight));
            }

            // roll string
            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };
            g.DrawString(MathHelper.Rad2Deg(Roll).ToString("000"), _font, _brush, center, 0.0, new Point(center.X, majorTickLength + indicatorHeight + 5), stringFormat);
        }
    }
}
