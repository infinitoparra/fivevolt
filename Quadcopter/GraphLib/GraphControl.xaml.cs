using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace GraphLib
{
    public partial class GraphControl : UserControl
    {
        private WriteableBitmap _bitmap;
        private readonly GraphRenderer _graphRenderer = new GraphRenderer();

        public Graph Graph { get; set; }

        public GraphRenderer GraphRenderer
        {
            get { return _graphRenderer; }
        }

        public GraphControl()
        {
            InitializeComponent();
            Graph = new Graph();
            RecreateBitmap();
        }

        private void _graph_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RecreateBitmap();
        }

        // ReSharper disable AssignNullToNotNullAttribute
        private void RecreateBitmap()
        {
            int width = (int)Math.Max(Math.Max(_graphContainer.ActualWidth, double.IsNaN(_graphContainer.Width) ? 0.0 : _graphContainer.Width), 20);
            int height = (int)Math.Max(Math.Max(_graphContainer.ActualHeight, double.IsNaN(_graphContainer.Height) ? 0.0 : _graphContainer.Height), 20);
            _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            _graphImage.Source = _bitmap;
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
                        lock (Graph.Lock)
                        {
                            _graphRenderer.Render(Graph, g, bm.Width, bm.Height);
                        }
                    }
                    _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
                }
                finally
                {
                    _bitmap.Unlock();
                }
            }));
        }
    }
}
