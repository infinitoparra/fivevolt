using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SevenSegmentChartMaker
{
    public partial class Main : Form
    {
        private Dictionary<char, byte> _values = new Dictionary<char, byte>();
        private string _lastFileName;

        public Main()
        {
            InitializeComponent();
            for (char c = '0'; c <= '9'; c++)
            {
                _character.Items.Add(c.ToString());
                _values.Add(c, 0);
            }
            for (char c = 'A'; c <= 'Z'; c++)
            {
                _character.Items.Add(c.ToString().ToLowerInvariant());
                _values.Add(c.ToString().ToLowerInvariant()[0], 0);
                _character.Items.Add(c.ToString());
                _values.Add(c, 0);
            }
        }

        private void _save_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = _lastFileName
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                SaveFile(dlg.FileName);
                _lastFileName = dlg.FileName;
            }
        }

        private void SaveFile(string fileName)
        {
            using (FileStream outStream = File.Open(fileName, FileMode.Create))
            using (StreamWriter writer = new StreamWriter(outStream))
            {
                writer.WriteLine("character,character hex,value,value hex,value binary");
                foreach (string item in _character.Items)
                {
                    char ch = item[0];
                    byte val = _values[ch];
                    string binary = Convert.ToString(val, 2).PadLeft(8, '0');
                    writer.WriteLine(
                        string.Format(
                            "{0},{1},{2},{3},{4}",
                            ch,
                            Convert.ToString(ch, 16),
                            val,
                            Convert.ToString(val, 16),
                            binary));
                }
            }
        }

        private void _load_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                LoadFile(dlg.FileName);
            }
        }

        private void LoadFile(string fileName)
        {
            using (FileStream inStream = File.Open(fileName, FileMode.Open))
            using (StreamReader reader = new StreamReader(inStream))
            {
                reader.ReadLine(); // header
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    char ch = parts[0][0];
                    byte value = byte.Parse(parts[2]);
                    _values[ch] = value;
                }
            }
        }

        private void _character_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte val = _values[((string)_character.SelectedItem)[0]];
            _sevenSegment.Value = val;
        }

        private void _sevenSegment_Change(object sender, EventArgs e)
        {
            _values[((string)_character.SelectedItem)[0]] = _sevenSegment.Value;
        }
    }
}
