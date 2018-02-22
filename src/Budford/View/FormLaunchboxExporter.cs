using Budford.Model;
using Budford.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Budford.View
{
    public partial class FormLaunchboxExporter : Form
    {
        Model.Model model;
        HashSet<string> games;

        Dictionary<string, string> emulators = new Dictionary<string, string>();

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
                }
            }

            if (id != "")
            {
                string[] toks = comboBox1.Text.Split(':');
                if (toks.Length == 2)
                {
                    string platformFileName = Path.Combine(Path.GetDirectoryName(textBox1.Text), "Data", "Platforms", toks[0] + ".xml");
                    if (File.Exists(platformFileName))
                    {
                        if (File.Exists(platformFileName + ".bak"))
                        {
                            File.Delete(platformFileName + ".bak");
                        }
                        File.Move(platformFileName, platformFileName + ".bak");
                    }

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
                        MessageBox.Show("Successfully exported " + count + " games to LaunchBox");
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "LaunchBox Executeable| *.exe;";

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
                    foreach (var g in xElement.Elements("EmulatorPlatform"))
                    {
                        string Emulator = g.Element("Emulator").Value;
                        string Platform = g.Element("Platform").Value;
                        if (Emulator.Length > 0)
                        {
                            emulators.Add(Emulator, Platform);
                        }
                    }

                    foreach (var g in xElement.Elements("Emulator"))
                    {
                        string Title = g.Element("Title").Value;
                        string ID = g.Element("ID").Value;
                        if (Title.Length > 0)
                        {
                            if (emulators.ContainsKey(ID))
                            {
                                emulators[ID] += ":" + Title;
                            }
                        }
                    }

                    foreach (var v in emulators)
                    {
                        comboBox1.Items.Add(v.Value);
                    }

                    if (emulators.Count > 0)
                    {
                        // TODO - Look for Wii U or Cemu?
                        comboBox1.SelectedIndex = 0;
                        comboBox1.Enabled = true;
                        button1.Enabled = true;
                    }
                }
            }
        }
    }
}
