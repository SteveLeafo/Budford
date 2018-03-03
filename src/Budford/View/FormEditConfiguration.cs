using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Budford.Model;

namespace Budford.View
{
    public partial class FormEditConfiguration : Form
    {
        // Our config settings
        readonly Model.Model model;

        readonly Settings settings;

        readonly List<string> removedFolders = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        public FormEditConfiguration(Model.Model modelIn, int modeIn)
        {
            InitializeComponent();
            model = modelIn;
            settings = model.Settings;

            foreach (var folder in settings.RomFolders)
            {
                listView1.Items.Add(folder);
            }
            checkBox1.Checked = settings.DisableShaderCache;
            checkBox4.Checked = settings.LegacyIntelGpuMode;
            checkBox5.Checked = settings.UseGlobalVolumeSettings;
            checkBox6.Checked = settings.ScanGameFoldersOnStart;
            checkBox7.Checked = settings.IncludeWiiULauncherRpx;
            checkBox8.Checked = settings.AutomaticallyDownloadGraphicsPackOnStart;
            checkBox9.Checked = settings.AutomaticallyDownloadLatestEverythingOnStart;

            textBox4.Text = settings.MlcFolder;
            textBox2.Text = settings.SavesFolder;
            textBox3.Text = settings.DownloadsFolder;

            for (int i = 0; i < Screen.AllScreens.Length; ++i)
            {
                comboBox8.Items.Add("Monitor " + (i + 1));
            }

            comboBox8.SelectedIndex = 0;
            if (model.Settings.Monitor - 1 < comboBox8.Items.Count)
            {
                comboBox8.SelectedIndex = model.Settings.Monitor - 1;
            }

            textBox1.Text = model.Settings.WineExe;
            comboBox7.SelectedIndex = 0;
            for (int i = 0; i < comboBox7.Items.Count; ++i)
            {
                if (comboBox7.Items[i].ToString() == model.Settings.StopHotkey)
                {
                    comboBox7.SelectedIndex = i;
                    break;
                }
            }

            trackBar1.Minimum = 1;
            trackBar1.Maximum = 100;
            trackBar1.Value = settings.GlobalVolume;

            radioButton1.Checked = settings.DefaultResolution == "2160p";
            radioButton2.Checked = settings.DefaultResolution == "1800p";
            radioButton3.Checked = settings.DefaultResolution == "1440p";
            radioButton4.Checked = settings.DefaultResolution == "1080p";
            radioButton12.Checked = settings.DefaultResolution == "900p";

            radioButton5.Checked = settings.DefaultResolution == "540p";
            radioButton11.Checked = settings.DefaultResolution == "480p";
            radioButton6.Checked = settings.DefaultResolution == "360p";

            radioButton8.Checked = settings.DefaultResolution == "5760p";
            radioButton10.Checked = settings.DefaultResolution == "4320p";
            radioButton9.Checked = settings.DefaultResolution == "2880p";


            radioButton7.Checked = settings.DefaultResolution == "default";

            if (modeIn == 1)
            {
                tabControl1.SelectTab(1);
            }
            else if (modeIn == 2)
            {
                tabControl1.SelectTab(2);
            }

            comboBox2.SelectedIndex = (int)settings.ConsoleLanguage;
            switch (settings.ConsoleRegion)
            {
                case Settings.ConsoleRegionType.Auto:
                    comboBox1.SelectedIndex = 0;
                    break;
                case Settings.ConsoleRegionType.Jap:
                    comboBox1.SelectedIndex = 1;
                    break;
                case Settings.ConsoleRegionType.Usa:
                    comboBox1.SelectedIndex = 2;
                    break;
                case Settings.ConsoleRegionType.Eur:
                    comboBox1.SelectedIndex = 3;
                    break;
                case Settings.ConsoleRegionType.China:
                    comboBox1.SelectedIndex = 4;
                    break;
                case Settings.ConsoleRegionType.Korea:
                    comboBox1.SelectedIndex = 5;
                    break;
                case Settings.ConsoleRegionType.Taiwan:
                    comboBox1.SelectedIndex = 6;
                    break;                
            }

            comboBox3.SelectedIndex = settings.SingleCorePriority;
            comboBox4.SelectedIndex = settings.DualCorePriority;
            comboBox5.SelectedIndex = settings.TripleCorePriority;
            comboBox6.SelectedIndex = settings.ShaderPriority;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    listView1.Items.Add(fbd.SelectedPath);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listView1.SelectedItems)
            {
                removedFolders.Add(lvi.Text);
                listView1.Items.Remove(lvi);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {           
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            settings.RomFolders.Clear();
            foreach (ListViewItem item in listView1.Items)
            {
                settings.RomFolders.Add(item.Text);
            }

            HandleRemovedFolder();

            if (radioButton8.Checked) settings.DefaultResolution = "5760p";
            if (radioButton10.Checked) settings.DefaultResolution = "4320p";
            if (radioButton9.Checked) settings.DefaultResolution = "2880p";

            if (radioButton1.Checked) settings.DefaultResolution = "2160p";
            if (radioButton2.Checked) settings.DefaultResolution = "1800p";
            if (radioButton3.Checked) settings.DefaultResolution = "1440p";
            if (radioButton4.Checked) settings.DefaultResolution = "1080p";
            if (radioButton12.Checked) settings.DefaultResolution = "900p";

            if (radioButton5.Checked) settings.DefaultResolution = "540p";
            if (radioButton11.Checked) settings.DefaultResolution = "480p";
            if (radioButton6.Checked) settings.DefaultResolution = "360p";

            if (radioButton7.Checked) settings.DefaultResolution = "default";

            settings.UseGlobalVolumeSettings = checkBox5.Checked;
            settings.ScanGameFoldersOnStart = checkBox6.Checked;
            settings.IncludeWiiULauncherRpx = checkBox7.Checked;
            settings.AutomaticallyDownloadGraphicsPackOnStart = checkBox8.Checked;
            settings.AutomaticallyDownloadLatestEverythingOnStart = checkBox9.Checked;

            model.Settings.MlcFolder = textBox4.Text;
            model.Settings.SavesFolder = textBox2.Text;
            model.Settings.DownloadsFolder = textBox3.Text;

            model.Settings.Monitor = comboBox8.SelectedIndex + 1;

            model.Settings.WineExe = textBox1.Text;
            model.Settings.StopHotkey = comboBox7.Text;

            settings.GlobalVolume = trackBar1.Value;

            settings.ConsoleLanguage = (Settings.ConsoleLanguageType)comboBox2.SelectedIndex;

            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    settings.ConsoleRegion = Settings.ConsoleRegionType.Auto;
                    break;
                case 1:
                    settings.ConsoleRegion = Settings.ConsoleRegionType.Jap;
                    break;
                case 2:
                    settings.ConsoleRegion = Settings.ConsoleRegionType.Usa;
                    break;
                case 3:
                    settings.ConsoleRegion = Settings.ConsoleRegionType.Eur;
                    break;
                case 4:
                    settings.ConsoleRegion = Settings.ConsoleRegionType.China;
                    break;
                case 5:
                    settings.ConsoleRegion = Settings.ConsoleRegionType.Korea;
                    break;
                case 6:
                    settings.ConsoleRegion = Settings.ConsoleRegionType.Taiwan;
                    break;
            }

            base.OnClosing(e);
        }

        /// <summary>
        /// 
        /// </summary>
        private void HandleRemovedFolder()
        {
            if (checkBox3.Checked)
            {
                List<string> keysToRemove = new List<string>();
                foreach (var game in model.GameData)
                {
                    foreach (var removedFolder in removedFolders)
                    {
                        if (game.Value.LaunchFile.Contains(removedFolder))
                        {
                            keysToRemove.Add(game.Key);
                        }
                    }
                }

                foreach (var key in keysToRemove)
                {
                    model.GameData.Remove(key);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            settings.DisableShaderCache = checkBox1.Checked;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            settings.LegacyIntelGpuMode = checkBox4.Checked;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (var game in model.GameData)
            {
                game.Value.GameSetting.Volume = (byte)trackBar1.Value;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Wine Executeable| *.*;";

                // Show open file dialog box 
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = dlg.FileName;
                    model.Settings.WineExe = textBox1.Text;
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox4.Text = fbd.SelectedPath;
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox2.Text = fbd.SelectedPath;
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox3.Text = fbd.SelectedPath;
                }
            }
        }
    }
}
