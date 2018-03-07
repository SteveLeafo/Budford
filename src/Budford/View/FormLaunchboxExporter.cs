using Budford.Model;
using Budford.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Budford.Properties;

namespace Budford.View
{
    public partial class FormLaunchboxExporter : Form
    {
        readonly Model.Model model;
        readonly HashSet<string> games;

        readonly Dictionary<string, string> emulators = new Dictionary<string, string>();

        public FormLaunchboxExporter(Model.Model modelIn, HashSet<string>gamesIn)
        {
            InitializeComponent();

            textBox1.ReadOnly = true;

            model = modelIn;
            games = gamesIn;

            if (model.Settings.LaunchBoxExe != "")
            {
                 textBox1.Text = model.Settings.LaunchBoxExe;
                PopulateComboBox();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string id = "";

            foreach (var emulator in emulators)
            {
                if (emulator.Value == comboBox1.Text)
                {
                    id = emulator.Key;
                    string[] toks = comboBox1.Text.Split(':');
                    if (toks.Length == 2)
                    {
                        string platformFileName = Path.Combine(Path.GetDirectoryName(textBox1.Text), "Data", "Platforms", toks[0] + ".xml");

                        BackupPlatformFile(platformFileName);

                        UpdatePlatformFile(id, toks, platformFileName);
                    }
                }
            }
        }

        private static void BackupPlatformFile(string platformFileName)
        {
            if (File.Exists(platformFileName))
            {
                if (File.Exists(platformFileName + ".bak"))
                {
                    File.Delete(platformFileName + ".bak");
                }
                File.Move(platformFileName, platformFileName + ".bak");
            }
        }

        private void UpdatePlatformFile(string id, string[] toks, string platformFileName)
        {
            using (StreamWriter sw = new StreamWriter(platformFileName))
            {
                sw.WriteLine("<?xml version=\"1.0\" standalone=\"yes\"?>");
                sw.WriteLine("<LaunchBox>");
                int count = 0;
                foreach (var game in model.GameData)
                {
                    if (games == null || games.Contains(game.Value.Name))
                    {
                        if (LaunchPad.GetId(game.Value.Name) != "")
                        {
                            count++;
                            sw.WriteLine("<Game>");
                            sw.WriteLine("  <ApplicationPath>" + Xml.XmlEscape(game.Value.LaunchFile) + "</ApplicationPath>");
                            sw.WriteLine("  <Emulator>" + id + "</Emulator>");
                            sw.WriteLine("  <Platform>" + toks[0] + "</Platform>");
                            sw.WriteLine("  <Title>" + Xml.XmlEscape(game.Value.Name) + "</Title>");
                            sw.WriteLine("  <Id>" + LaunchPad.GetId(game.Value.Name) + "</Id>");
                            sw.WriteLine("  <PlayCount>" + game.Value.PlayCount + "</PlayCount>");
                            sw.WriteLine("</Game>");
                        }
                    }
                }
                sw.WriteLine("</LaunchBox>");
                MessageBox.Show(Resources.FormLaunchboxExporter_button1_Click_Successfully_exported_ + count + Resources.FormLaunchboxExporter_button1_Click__games_to_LaunchBox);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = Resources.FormLaunchboxExporter_button2_Click_LaunchBox_Executeable____exe_;

                // Show open file dialog box 
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = dlg.FileName;
                    model.Settings.LaunchBoxExe = textBox1.Text;
                    PopulateComboBox();
                }
            }
        }

        void PopulateComboBox()
        {
            button1.Enabled = false;
            emulators.Clear();
            if (textBox1.Text.Length > 0)
            {
                string emulatorFileName = Path.Combine(Path.GetDirectoryName(textBox1.Text), "Data", "Emulators.xml");
                
                if (File.Exists(emulatorFileName))
                {
                    comboBox1.Items.Clear();

                    XElement xElement = XElement.Parse(XDocument.Load(emulatorFileName).ToString());

                    ExtractPlatforms(xElement);
                    ExtractEmulators(xElement);

                    foreach (var v in emulators)
                    {
                        comboBox1.Items.Add(v.Value);
                    }

                    EnableControlsForExport();
                }
            }
        }

        private void EnableControlsForExport()
        {
            if (emulators.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
                comboBox1.Enabled = true;
                button1.Enabled = true;
            }
        }

        private void ExtractEmulators(XElement xElement)
        {

            foreach (var g in xElement.Elements("Emulator"))
            {
                var element = g.Element("Title");
                if (element != null)
                {
                    string title = element.Value;
                    var o = g.Element("ID");
                    if (o != null)
                    {
                        string id = o.Value;
                        if (title.Length > 0)
                        {
                            if (emulators.ContainsKey(id))
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append(emulators[id]);
                                sb.Append(":");
                                sb.Append(title);
                                emulators[id] = sb.ToString();
                            }
                        }
                    }
                }
            }
        }

        private void ExtractPlatforms(XElement xElement)
        {
            foreach (var g in xElement.Elements("EmulatorPlatform"))
            {
                var element = g.Element("Emulator");
                if (element != null)
                {
                    string emulator = element.Value;
                    var o = g.Element("Platform");
                    if (o != null)
                    {
                        string platform = o.Value;
                        if (emulator.Length > 0)
                        {
                            emulators.Add(emulator, platform);
                        }
                    }
                }
            }
        }
    }
}
