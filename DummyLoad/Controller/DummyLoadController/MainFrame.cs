using System.Drawing;
using DummyLoadController.Services;
using WX;
using WX.Aui;
using System;
using System.IO;
using System.Xml.Linq;
using System.Xml;

namespace DummyLoadController
{
    public class MainFrame : Frame
    {
        private const int ID_FileExit = 1;
        private readonly AuiManager _auiManager;
        private readonly ComService _comService = new ComService();
        private FrontPanelPanel _frontPanel;
        private ComPanel _comPanel;

        public MainFrame()
            : base(null, "DummyLoad Controller", WxDefaultPosition, new Size(500, 500))
        {
            _auiManager = new AuiManager(this, AuiManagerOption.Default | AuiManagerOption.VenetianBlindsHint);
            var auiNotebook = new AuiNotebook(this, -1, WxDefaultPosition, new Size(100, 100), AuiNotebookOption.DefaultStyle);
            AuiPaneInfo notebookPaneInfo = new AuiPaneInfo
            {
                CloseButtom = false,
                DockFixed = true,
                Gripper = false,
                CaptionVisible = false,
                Direction = Direction.Top,
                PaneBorder = false
            };
            notebookPaneInfo.CenterPane();
            _auiManager.AddPane(auiNotebook, notebookPaneInfo);

            _comPanel = new ComPanel(this, _comService);
            AuiPaneInfo comPanelPaneInfo = new AuiPaneInfo
            {
                Direction = Direction.Bottom,
                Caption = "COM"
            };
            comPanelPaneInfo.Bottom();
            _auiManager.AddPane(_comPanel, comPanelPaneInfo);

            _frontPanel = new FrontPanelPanel(this, _comService);
            auiNotebook.AddPage(_frontPanel, "Front Panel");

            _auiManager.Update();

            InitMenuBar();

            LoadSettings();

            EvtClose((s, e) => { OnClose(); e.Skip(true); });
        }

        private void OnClose()
        {
            SaveSettings();

            if (_comService != null)
            {
                _comService.Disconnect();
                _comService.Dispose();
            }
        }

        private void LoadSettings()
        {
            try
            {
                string fileName = GetSettingsFileName();
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                if (!File.Exists(fileName))
                {
                    return;
                }

                using (FileStream stream = File.Open(fileName, FileMode.Open))
                {
                    XDocument xdoc = XDocument.Load(stream);
                    XElement rootElem = xdoc.Root;

                    XElement frontPanelElem = rootElem.Element("FrontPanel");
                    if (frontPanelElem != null && _frontPanel != null)
                    {
                        _frontPanel.LoadSettings(frontPanelElem);
                    }

                    XElement comElem = rootElem.Element("ComPanel");
                    if (comElem != null && _comPanel != null)
                    {
                        _comPanel.LoadSettings(comElem);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageDialog.ShowModal(this, ex.Message, "Error Loading Settings", WindowStyles.IconError);
            }
        }

        private void SaveSettings()
        {
            string fileName = GetSettingsFileName();
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Indent = true;

            using (FileStream stream = File.Open(fileName, FileMode.Create))
            using (XmlWriter writer = XmlWriter.Create(stream, writerSettings))
            {
                XDocument xdoc = new XDocument();
                XElement rootElem = new XElement("DummyLoadSettings");
                xdoc.Add(rootElem);

                XElement frontPanel = new XElement("FrontPanel");
                if (_frontPanel != null)
                {
                    _frontPanel.SaveSettings(frontPanel);
                }
                rootElem.Add(frontPanel);

                XElement comPanel = new XElement("ComPanel");
                if (_comPanel != null)
                {
                    _comPanel.SaveSettings(comPanel);
                }
                rootElem.Add(comPanel);

                xdoc.WriteTo(writer);
            }
        }

        private string GetSettingsFileName()
        {
            return Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "DummyLoad\\Setting.xml");
        }

        private void InitMenuBar()
        {
            MenuBar menuBar = new MenuBar();
            Menu fileMenu = new Menu();

            fileMenu.Append(ID_FileExit, "E&xit\tAlt+F4", "Exit this app");
            EvtMenu(ID_FileExit, (s, e) => Close());

            menuBar.Append(fileMenu, "&File");
            MenuBar = menuBar;
        }
    }
}