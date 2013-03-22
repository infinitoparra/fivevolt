using System;
using System.Drawing;
using System.Windows.Forms;

namespace SevenSegmentChartMaker
{
    public partial class SevenSegment : UserControl
    {
        public event EventHandler Change;

        public SevenSegment()
        {
            InitializeComponent();
        }

        public byte Value
        {
            get
            {
                return
                    (byte)((isOn(_segmentA) ? 0x40 : 0x00)
                            | (isOn(_segmentB) ? 0x20 : 0x00)
                            | (isOn(_segmentC) ? 0x10 : 0x00)
                            | (isOn(_segmentD) ? 0x08 : 0x00)
                            | (isOn(_segmentE) ? 0x04 : 0x00)
                            | (isOn(_segmentF) ? 0x02 : 0x00)
                            | (isOn(_segmentG) ? 0x01 : 0x00)
                    );
            }
            set
            {
                set(_segmentA, (value & 0x40) == 0x40);
                set(_segmentB, (value & 0x20) == 0x20);
                set(_segmentC, (value & 0x10) == 0x10);
                set(_segmentD, (value & 0x08) == 0x08);
                set(_segmentE, (value & 0x04) == 0x04);
                set(_segmentF, (value & 0x02) == 0x02);
                set(_segmentG, (value & 0x01) == 0x01);
            }
        }

        private void _segmentA_Click(object sender, EventArgs e)
        {
            toggle(_segmentA);
        }

        private void _segmentB_Click(object sender, EventArgs e)
        {
            toggle(_segmentB);
        }

        private void _segmentC_Click(object sender, EventArgs e)
        {
            toggle(_segmentC);
        }

        private void _segmentD_Click(object sender, EventArgs e)
        {
            toggle(_segmentD);
        }

        private void _segmentE_Click(object sender, EventArgs e)
        {
            toggle(_segmentE);
        }

        private void _segmentF_Click(object sender, EventArgs e)
        {
            toggle(_segmentF);
        }

        private void _segmentG_Click(object sender, EventArgs e)
        {
            toggle(_segmentG);
        }

        private void set(PictureBox segment, bool val)
        {
            if (val)
            {
                turnOn(segment);
            }
            else
            {
                turnOff(segment);
            }
        }

        private void toggle(PictureBox segment)
        {
            if (isOn(segment))
            {
                turnOff(segment);
            }
            else
            {
                turnOn(segment);
            }
        }

        private void turnOn(PictureBox segment)
        {
            segment.BackColor = Color.Red;
            OnChange(this, new EventArgs());
        }

        private void turnOff(PictureBox segment)
        {
            segment.BackColor = BackColor;
            OnChange(this,new EventArgs());
        }

        private bool isOn(PictureBox segment)
        {
            return segment.BackColor == Color.Red;
        }

        private void OnChange(object sender, EventArgs args)
        {
            if (Change != null)
            {
                Change(sender, args);
            }
        }
    }
}
