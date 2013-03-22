using System;
using System.IO;
using System.Xml.Linq;

namespace Quadcopter
{
    public class Settings
    {
        private static readonly Settings _instance = new Settings();
        private bool _saveSuspended;
        public static Settings Instance { get { return _instance; } }

        public Settings()
        {
            Load();
        }

        public double SecondsToGraph { get; set; }
        public bool[] SeriesToGraph { get; set; }

        public string FileName
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Quadcopter/Settings.xml"); }
        }

        public bool IsSaveSuspended
        {
            get { return _saveSuspended; }
        }

        public void Load()
        {
            EnsureDirectoryExists();
            if (!File.Exists(FileName))
            {
                return;
            }

            XDocument doc = XDocument.Load(FileName);
            foreach (XElement setting in doc.Element("Settings").Elements("Setting"))
            {
                string name = setting.Attribute("name").Value;
                int idx = int.Parse(setting.Attribute("index").Value);
                if (setting.Attribute("isNull") != null)
                {
                    continue;
                }
                string val = setting.Value;

                switch (name)
                {
                    case "SecondsToGraph":
                        SecondsToGraph = double.Parse(val);
                        break;

                    case "SeriesToGraph":
                        EnsureSeriesToGraphSize(idx + 1);
                        SeriesToGraph[idx] = bool.Parse(val);
                        break;
                }
            }
        }

        public void EnsureSeriesToGraphSize(int size)
        {
            if (SeriesToGraph == null)
            {
                SeriesToGraph = new bool[size];
            }
            else if (SeriesToGraph.Length < size)
            {
                bool[] s = SeriesToGraph;
                Array.Resize(ref s, size);
                SeriesToGraph = s;
            }
        }

        public void Save()
        {
            if (_saveSuspended)
            {
                return;
            }

            EnsureDirectoryExists();

            XDocument doc = new XDocument();
            XElement root = new XElement("Settings");

            root.Add(CreateValue("SecondsToGraph", 0, SecondsToGraph));
            if (SeriesToGraph != null)
            {
                for (int i = 0; i < SeriesToGraph.Length; i++)
                {
                    root.Add(CreateValue("SeriesToGraph", i, SeriesToGraph[i]));
                }
            }

            doc.Add(root);

            doc.Save(FileName);
        }

        private XElement CreateValue(string name, int idx, object value)
        {
            XElement result = new XElement("Setting",
                new XAttribute("name", name),
                new XAttribute("index", idx));
            if (value == null)
            {
                result.Add(new XAttribute("isNull", "true"));
            }
            else
            {
                result.Add(new XCData(value.ToString()));
            }
            return result;
        }

        private void EnsureDirectoryExists()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FileName));
        }

        public void SuspendSave()
        {
            _saveSuspended = true;
        }

        public void ResumeSave()
        {
            _saveSuspended = false;
        }
    }
}
