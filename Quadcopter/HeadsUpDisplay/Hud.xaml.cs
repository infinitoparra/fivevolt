using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace HeadsUpDisplay
{
    public partial class Hud : UserControl
    {
        private WriteableBitmap _bitmap;
        private readonly HudRenderer _hudRenderer = new HudRenderer();
        private bool _redrawEnabled;

        public Hud()
        {
            InitializeComponent();
            RecreateBitmap();
        }

        public double Roll
        {
            get { return _hudRenderer.Roll; }
            set { _hudRenderer.Roll = value; Redraw(); }
        }

        public double Pitch
        {
            get { return _hudRenderer.Pitch; }
            set { _hudRenderer.Pitch = value; Redraw(); }
        }

        public double Yaw
        {
            get { return _hudRenderer.Yaw; }
            set { _hudRenderer.Yaw = value; Redraw(); }
        }

        public double BatteryLevel
        {
            get { return _hudRenderer.BatteryLevel; }
            set { _hudRenderer.BatteryLevel = value; Redraw(); }
        }

        public double QuadcopterFrontLevel
        {
            get { return _hudRenderer.QuadcopterFrontLevel; }
            set { _hudRenderer.QuadcopterFrontLevel = value; Redraw(); }
        }

        public double QuadcopterBackLevel
        {
            get { return _hudRenderer.QuadcopterBackLevel; }
            set { _hudRenderer.QuadcopterBackLevel = value; Redraw(); }
        }

        public double QuadcopterLeftLevel
        {
            get { return _hudRenderer.QuadcopterLeftLevel; }
            set { _hudRenderer.QuadcopterLeftLevel = value; Redraw(); }
        }

        public double QuadcopterRightLevel
        {
            get { return _hudRenderer.QuadcopterRightLevel; }
            set { _hudRenderer.QuadcopterRightLevel = value; Redraw(); }
        }

        public double UserInputRoll
        {
            get { return _hudRenderer.UserInputRoll; }
            set { _hudRenderer.UserInputRoll = value; Redraw(); }
        }

        public double UserInputPitch
        {
            get { return _hudRenderer.UserInputPitch; }
            set { _hudRenderer.UserInputPitch = value; Redraw(); }
        }

        public double UserInputYaw
        {
            get { return _hudRenderer.UserInputYaw; }
            set { _hudRenderer.UserInputYaw = value; Redraw(); }
        }

        public double UserInputElevation
        {
            get { return _hudRenderer.UserInputElevation; }
            set { _hudRenderer.UserInputElevation = value; Redraw(); }
        }

        private void _hud_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RecreateBitmap();
        }

        // ReSharper disable AssignNullToNotNullAttribute
        private void RecreateBitmap()
        {
            int width = (int)Math.Max(Math.Max(_hudContainer.ActualWidth, double.IsNaN(_hudContainer.Width) ? 0.0 : _hudContainer.Width), 20);
            int height = (int)Math.Max(Math.Max(_hudContainer.ActualHeight, double.IsNaN(_hudContainer.Height) ? 0.0 : _hudContainer.Height), 20);
            _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            _hud.Source = _bitmap;
            Redraw();
        }
        // ReSharper restore AssignNullToNotNullAttribute

        public void Redraw()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                _bitmap.Lock();
                try
                {
                    using (Bitmap bm = new Bitmap(_bitmap.PixelWidth, _bitmap.PixelHeight, _bitmap.BackBufferStride, PixelFormat.Format32bppArgb, _bitmap.BackBuffer))
                    using (Graphics g = Graphics.FromImage(bm))
                    {
                        _hudRenderer.Render(g, bm.Width, bm.Height);
                    }
                    _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
                }
                finally
                {
                    _bitmap.Unlock();
                }
            }));
        }

        public void SuspendRedraw()
        {
            _redrawEnabled = false;
        }

        public void ResumeRedraw()
        {
            _redrawEnabled = true;
            Redraw();
        }
    }
}
