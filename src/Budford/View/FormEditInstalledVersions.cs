using System;
using System.Linq;
using System.Windows.Forms;
using Budford.Model;
using Budford.Control;
using System.IO;
using Budford.Properties;

namespace Budford.View
{
    public partial class FormEditInstalledVersions : Form
    {
        // All of our data...
        readonly Model.Model model;

        // For downloading and extracing.
        readonly Unpacker unpacker;

        // For launching the games.
        readonly Launcher launcher;

        // Set to true while populating the list to stop events changing the data
        bool updating;

        // Managing save files and folders.
        internal static string[] Uris = new[]
            {
                "http://cemu.info/releases/cemu_1.9.1.zip",
                "https://files.sshnuke.net/cemuhook_190c_0532.zip",
                "https://github.com/slashiee/cemu_graphic_packs/archive/master.zip",
                "https://files.sshnuke.net/sharedFonts.7z"
            };

        internal static string[] Filenames= new[]
            {
                "cemu_1.9.1.zip",
                "cemu_hook.zip",
                "graphicsPacks.zip",
                "sharedFonts.7z"
            };

        bool autoSize = true;

        /// <summary>
        /// 
        /// </summary>
        public FormEditInstalledVersions()
        {
            InitializeComponent();

            ShowIcon = true;

            model = Persistence.Load(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Model.xml");

            Initialise();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelIn"></param>
        /// <param name="unpackerIn"></param>
        /// <param name="launcherIn"></param>
        internal FormEditInstalledVersions(Model.Model modelIn, Unpacker unpackerIn, Launcher launcherIn)
        {
            InitializeComponent();

            model = modelIn;
            unpacker = unpackerIn;
            launcher = launcherIn;

            Initialise(); 
        }

        /// <summary>
        /// 
        /// </summary>
        private void Initialise()
        {
            listView1.DoubleBuffered(true);

            PopulateList();

            textBox1.Text = model.Settings.DefaultInstallFolder;

            autoSize = true;

            Uris = new[]
            {
                "http://cemu.info/releases/cemu_" + model.Settings.CurrentCemuVersion + ".zip",
                "https://files.sshnuke.net/cemuhook_190c_0532.zip",
                "https://github.com/slashiee/cemu_graphic_packs/archive/master.zip",
                "https://files.sshnuke.net/sharedFonts.7z"
            };

            Filenames = new[]
            {
                "cemu_" + model.Settings.CurrentCemuVersion + ".zip",
                "cemu_hook.zip",
                "graphicsPacks.zip",
                "sharedFonts.7z"
            };

            AddOldGameMenuItems();

            UpdateGraphicsPackCombo();
        }     
            
        /// <summary>
        /// 
        /// </summary>
        private void UpdateGraphicsPackCombo(bool useLatest = false)
        {
            comboBox1.Items.Clear();
            string pack = "";
            if (Directory.Exists("graphicsPacks"))
            {
                foreach (var dir in Directory.EnumerateDirectories("graphicsPacks"))
                {
                    string folder = dir.Replace("graphicsPacks\\", "");
                    if (folder.StartsWith("graphicPacks_2-"))
                    {
                        pack = folder.Replace("graphicPacks_2-", "");
                        comboBox1.Items.Add(pack);
                        if (pack == model.Settings.GraphicsPackRevision)
                        {
                            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
                        }
                    }
                }
            }

            if (useLatest && pack != "")
            {
                comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
                model.Settings.GraphicsPackRevision = pack;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void PopulateList()
        {
            listView1.Items.Clear();
            updating = true;
            listView1.BeginUpdate();
            try
            {
                foreach (var v in model.Settings.InstalledVersions.OrderByDescending(version => version.Name))
                {
                    ListViewItem lvi = new ListViewItem("")
                    {
                        Tag = v,
                        Checked = v.IsLatest
                    };
                    lvi.SubItems.Add(v.Name);
                    lvi.SubItems.Add(v.Folder);
                    lvi.SubItems.Add(v.Version);
                    lvi.SubItems.Add(v.HasFonts ? "Yes" : "No");
                    lvi.SubItems.Add(v.HasCemuHook ? "Yes" : "No");
                    lvi.SubItems.Add(GetLinkType(v));
                    lvi.SubItems.Add(v.HasPatch ? "Yes" : "No");
                    lvi.SubItems.Add(v.DlcSource == null ? "" : v.DlcSource);
                    listView1.Items.Add(lvi);
                }

                if (autoSize)
                {
                    listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                }
                else
                {
                    listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
                    listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
                }
                listView1.Sort();
            }
            finally
            {
                updating = false;
                listView1.EndUpdate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private static string GetLinkType(InstalledVersion v)
        {
            switch (v.DlcType)
            {
                case 0: return "No";
                case 1: return "Yes";
                case 2: return "Link";
                case 3: return "Dead";
            }
            return "??";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstalledVersion iv = new InstalledVersion();
            using (FormEditInstalledVersion eiv = new FormEditInstalledVersion(model.Settings.InstalledVersions, iv))
            {
                if (eiv.ShowDialog(this) == DialogResult.OK)
                {
                    model.Settings.InstalledVersions.Add(iv);
                    PopulateList();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                InstalledVersion iv = (InstalledVersion)listView1.SelectedItems[0].Tag;
                using (FormEditInstalledVersion eiv = new FormEditInstalledVersion(model.Settings.InstalledVersions, iv))
                {
                    if (eiv.ShowDialog(this) == DialogResult.OK)
                    {
                        PopulateList();
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                var installedVersion = (InstalledVersion)listView1.SelectedItems[0].Tag;
                model.Settings.InstalledVersions.Remove(installedVersion);
                if (installedVersion.IsLatest)
                {
                    if (model.Settings.InstalledVersions.Any())
                    {
                        model.Settings.InstalledVersions[0].IsLatest = true;
                    }
                }

                PopulateList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            FileManager.SearchForInstalledVersions(model);
            FolderScanner.GetGameInformation(null, "", "");
            CemuFeatures.UpdateFeaturesForInstalledVersions(model);
            autoSize = false;
            PopulateList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!updating)
            {
                if (e.Item.Checked)
                {
                    ((InstalledVersion)e.Item.Tag).IsLatest = true;
                }

                foreach (var v in model.Settings.InstalledVersions)
                {
                    if (v != e.Item.Tag)
                    {
                        v.IsLatest = false;
                    }
                }

                UpdateListView();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateListView()
        {
            updating = true;
            try
            {
                listView1.BeginUpdate();
                foreach (ListViewItem lvi in listView1.Items)
                {
                    lvi.Checked = ((InstalledVersion)lvi.Tag).IsLatest;
                }
                listView1.EndUpdate();
            }
            finally
            {
                updating = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox1.Text = fbd.SelectedPath;
                    model.Settings.DefaultInstallFolder = textBox1.Text;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            CemuFeatures.RepairInstalledVersions(this, model);
            PopulateList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addNewInstanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string version;
                    if (CemuFeatures.IsCemuFolder(fbd.SelectedPath, out version))
                    {
                        FileManager.AddInstalledVersion(model, fbd.SelectedPath, version);
                        CemuFeatures.UpdateFeaturesForInstalledVersions(model);
                        PopulateList();
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            model.Settings.InstalledVersions.Clear();
            PopulateList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button3_Click(null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void launchCEMUToolStripMenuItem_Click(object sender, EventArgs e)
        {
            launcher.LaunchCemu(null, model, null, false, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadLatestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CemuFeatures.DownloadLatestVersion(this, model.Settings))
            {
                FileManager.DownloadCemu(this, unpacker, model, Uris, Filenames);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void oldVersionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var toolStripMenuItem = sender as ToolStripMenuItem;
            if (toolStripMenuItem != null)
            {
                OldVersion oldVersion = toolStripMenuItem.Tag as OldVersion;
                if (oldVersion != null)
                {
                    unpacker.DownloadAndUnpack(oldVersion.Name + ".zip", oldVersion.Uri, model.Settings.DefaultInstallFolder, oldVersion.Name);
                    FileManager.SearchForInstalledVersions(model);
                    FolderScanner.GetGameInformation(null, "", "");
                    CemuFeatures.UpdateFeaturesForInstalledVersions(model);
                }
                AddOldGameMenuItems();
                PopulateList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadCemuHookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //unpacker.DownloadAndUnpack("cemu_hook.zip", "https://files.sshnuke.net/cemuhook_190c_0532.zip", "Downloads", "CEMU Hook");
            using (FormWebpageDownload dlc = new FormWebpageDownload("https://sshnuke.net/cemuhook", "Latest Version"))
            {
                dlc.ShowDialog(this);
                foreach (var line in dlc.Result.Split('\n'))
                {
                    string s = line.Trim();
                    if (s.Contains(".zip"))
                    {
                        if (s.Length > 20)
                        {
                            s = s.Substring(39);
                            int p = s.IndexOf("\"", StringComparison.Ordinal);
                            if (p > -1)
                            {
                                string cemuHook = s.Substring(0, p);
                                if (File.Exists("cemu_hook.zip"))
                                {
                                    File.Delete("cemu_hook.zip");
                                }
                                unpacker.DownloadAndUnpack("cemu_hook.zip", "https://files.sshnuke.net/" + cemuHook, "Downloads", "CEMU Hook");
                                return ;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadOldCemuHookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormFileDownload dlc = new FormFileDownload("https://files.sshnuke.net/cemuhook_174d_0410.zip", "OldCemuHook.zip"))
            {
                dlc.ShowDialog(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadGraphicsPacksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormWebpageDownload dlc = new FormWebpageDownload("https://api.github.com/repos/slashiee/cemu_graphic_packs/releases/latest", "Latest Graphic Pack"))
            {
                dlc.ShowDialog(this);
                CemuFeatures.DownloadLatestGraphicPack(this, dlc.Result);
                UpdateGraphicsPackCombo(true);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        void AddOldGameMenuItems()
        {
            oldVersionsToolStripMenuItem.DropDownItems.Clear();

            foreach (var oldVersion in model.OldVersions)
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem { Text = oldVersion.Name };

                if (CheckVersionExists(oldVersion))
                {
                    menuItem.Checked = true;
                }
                else
                {
                    menuItem.Click += oldVersionsToolStripMenuItem_Click;
                }
                menuItem.Tag = oldVersion;
                oldVersionsToolStripMenuItem.DropDownItems.Add(menuItem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldVersion"></param>
        /// <returns></returns>
        private bool CheckVersionExists(OldVersion oldVersion)
        {
            foreach (var v in model.Settings.InstalledVersions)
            {
                if (v.Version == oldVersion.Folder.Substring(5))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importGraphicsPackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = Resources.FormEditInstalledVersions_importGraphicsPackToolStripMenuItem_Click_Graphic_Packs____zip_;

                // Show open file dialog box 
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    unpacker.Unpack(dlg.FileName, "graphicsPacks\\" + Path.GetFileNameWithoutExtension(dlg.FileName));
                    UpdateGraphicsPackCombo();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            model.Settings.GraphicsPackRevision = comboBox1.Text;
        }
    }
}
