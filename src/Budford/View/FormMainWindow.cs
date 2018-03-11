using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Budford.Control;
using Budford.Model;
using Budford.Properties;
using Budford.Tools;
using Budford.Utilities;
using System.Runtime.InteropServices;

namespace Budford.View
{
    public partial class FormMainWindow : Form
    {
        // ReSharper disable once InconsistentNaming
        public string launchGame = "";
        public bool LaunchFull = true;

        [DllImport("user32.dll")]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly List<Model.PlugIns.PlugIn> plugIns = new List<Model.PlugIns.PlugIn>();

        const int MyactionHotkeyId = 1;

        enum KeyModifier
        {
            None = 0,
            // ReSharper disable once UnusedMember.Local
            Alt = 1,
            // ReSharper disable once UnusedMember.Local
            Control = 2,
            // ReSharper disable once UnusedMember.Local
            Shift = 4,
            // ReSharper disable once UnusedMember.Local
            WinKey = 8
        }


        // All of our data...
        internal readonly Model.Model Model;

        // For downloading and extracing.
        readonly Unpacker unpacker;

        // For launching the games.
        readonly Launcher launcher;

        // Used for column sorting when clicking on a header
        private readonly ListViewColumnSorter lvwColumnSorter;

        InstalledVersion iv1;

        readonly bool comments = false;

        /// <summary>
        /// 
        /// </summary>
        private void SetCemuFolder()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Model.Settings.DefaultInstallFolder = fbd.SelectedPath;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public FormMainWindow()
        {
            InitializeComponent();

            Model = TransferLegacyModel();

            UsbNotification.RegisterUsbDeviceNotification(Handle);

            unpacker = new Unpacker(this);
            launcher = new Launcher(this);

            if (!Directory.Exists(Model.Settings.DefaultInstallFolder))
            {
                using (FormFirstTimeWindow fftw = new FormFirstTimeWindow())
                {
                    switch (fftw.ShowDialog(this))
                    {
                        case DialogResult.OK:
                            FileManager.DownloadCemu(this, unpacker, Model);
                            break;
                        case DialogResult.Yes:
                            SetCemuFolder();
                            break;
                    }
                }
            }
            else
            {
                FileManager.SearchForInstalledVersions(Model);
                FolderScanner.GetGameInformation(null, "", "");
            }

            if (Model.Settings.ScanGameFoldersOnStart)
            {
                foreach (var folder in Model.Settings.RomFolders)
                {
                    using (FormScanRomFolder scanner = new FormScanRomFolder(Model, folder, Model.GameData))
                    {
                        scanner.ShowDialog(this);
                    }
                }
            }

            Model.OldVersions.Clear();

            if (Model.Settings.AutomaticallyDownloadLatestEverythingOnStart)
            {
                try
                {
                    FileManager.DownloadCemu(this, unpacker, Model);
                }
                catch (Exception)
                {
                    // No code
                }
            }
            else if (Model.Settings.AutomaticallyDownloadGraphicsPackOnStart)
            {
                try
                {
                    CemuFeatures.DownloadLatestGraphicsPack(this, Model, false);
                }
                catch (Exception)
                {
                    // No code
                }
            }

            FolderScanner.FindGraphicsPacks(new DirectoryInfo(Path.Combine("graphicsPacks", "graphicPacks_2-") + Model.Settings.GraphicsPackRevision), Model.GraphicsPacks);

            Persistence.LoadFromXml(Model.OldVersions);

            FolderScanner.AddGraphicsPacksToGames(Model);

            if (Model.Users.Count == 0)
            {
                Model.Users.Add(new User() { Name = "Default", Image = "default.png" });
                Model.CurrentUser = "Default";
            }

            var firstOrDefault = Model.Users.FirstOrDefault(u => u.Name == Model.CurrentUser);
            if (firstOrDefault != null && File.Exists(Path.Combine("Users", firstOrDefault.Image)))
            {
                var orDefault = Model.Users.FirstOrDefault(u => u.Name == Model.CurrentUser);
                if (orDefault != null)
                {
                    using (FileStream stream = new FileStream(Path.Combine("Users", orDefault.Image), FileMode.Open, FileAccess.Read))
                    {
                        pictureBox1.Image = Image.FromStream(stream);
                    }
                }
            }
            Text = Resources.fMainWindow_fMainWindow_CEMU_Game_DB______Current_User__ + Model.CurrentUser;

            AddUserMenuItems();
            SetupShowRegionMenuItems();

            showStatusToolStripMenuItem1.Checked = Model.Settings.ShowStausBar;
            statusStrip1.Visible = Model.Settings.ShowStausBar;
            toolStrip1.Visible = Model.Settings.ShowToolBar;
            pictureBox1.Visible = Model.Settings.ShowToolBar;
            showToolbarToolStripMenuItem.Checked = Model.Settings.ShowToolBar;
            listView1.KeyDown += listView1_KeyDown;
            this.Resize += FormMainWindow_Resize;

            Model.Settings.CurrentView = "Detailed";

            if (Model.Settings.CurrentView == "Detailed")
            {
                detailsToolStripMenuItem_Click(null, null);
            }
            else
            {
                tToolStripMenuItem_Click(null, null);
            }

            // Create an instance of a ListView column sorter and assign it 
            // to the ListView control.
            lvwColumnSorter = new ListViewColumnSorter {ColumnToSort = -1};
            listView1.ListViewItemSorter = lvwColumnSorter;

            listView1.DoubleBuffered(true);
            pictureBox1.DoubleBuffered(true);
            DoubleBuffered = true;
            listView1.DrawColumnHeader += ListView1_DrawColumnHeader;
            listView1.DrawSubItem += ListView1_DrawSubItem;
            listView1.ColumnClick += ListView1_ColumnClick;

            ListView1_ColumnClick(this, new ColumnClickEventArgs(Model.Settings.CurrentSortColumn));
            if (Model.Settings.CurrentSortDirection == 1)
            {
                ListView1_ColumnClick(this, new ColumnClickEventArgs(Model.Settings.CurrentSortColumn));
            }

            LoadPlugIns();
        }

        void FormMainWindow_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                //notifyIcon1.ShowBalloonTip(500);
                this.Hide();
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (launchGame != "")
            {
                WindowState = FormWindowState.Minimized;
                Visible = false;
                Hide();
                bool launched = false;
                foreach (var game in Model.GameData)
                {
                    if (game.Value.LaunchFile.ToLower() == launchGame.ToLower())
                    {
                        RegisterStopHotKey(Model);
                        game.Value.Exists = true;
                        launcher.LaunchCemu(this, Model, game.Value, false, false, false, LaunchFull);
                        launched = true;
                        break;
                    }
                }
                if (!launched)
                {
                    // Game wasn't in library, so just launch with current settings.
                    new Launcher(null).LaunchRpx(Model, launchGame, LaunchFull);
                }
            }
            
            base.OnLoad(e);
        }

        internal static string GetModelFileName()
        {
            return Program.IsInstalled() ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Budford", "Model.xml") : "Model.xml";
        }

        internal static Model.Model TransferLegacyModel()
        {

            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Budford")))
            {
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Budford"));
            }

            if (!File.Exists(GetModelFileName()))
            {
                if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Model.xml")))
                {
                    File.Copy(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Model.xml"), GetModelFileName(), false);
                    File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Model.xml"));
                }
            }

            Model.Model m = Persistence.Load(GetModelFileName());

            if (m.Settings.SavesFolder == "")
            {
                m.Settings.SavesFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
          
            return m;

        }


        internal void LoadPlugIns()
        {
            if (Directory.Exists(SpecialFolders.PlugInFolder(Model)))
            {
                List<ToolStripItem> items = new List<ToolStripItem>();

                string currentType = "";

                foreach (var file in Directory.EnumerateFiles(SpecialFolders.PlugInFolder(Model)))
                {
                    var extension = Path.GetExtension(file);
                    if (extension != null && extension.ToLower().Contains("xml"))
                    {
                        plugInsToolStripMenuItem.Visible = true;

                        Model.PlugIns.PlugIn p = Persistence.LoadPlugin(file);
                        plugIns.Add(p);

                     
                        ToolStripMenuItem menuItem = new ToolStripMenuItem
                        {
                            Text = p.Name,
                            Tag = p
                        };
                        menuItem.Click += PlugIn_Click;
                        items.Add(menuItem);
                    }
                }

                // Painful, but we want these added to the top of the list...
                plugInsToolStripMenuItem.DropDownItems.Clear();
                var v = (from i in items orderby ((Model.PlugIns.PlugIn)i.Tag).Type select i ).ToList();
                foreach (var item in v)
                {
                    Model.PlugIns.PlugIn p = (Model.PlugIns.PlugIn)item.Tag;
                    if (p.Type != currentType)
                    {
                        if (currentType != "")
                        {
                            plugInsToolStripMenuItem.DropDownItems.Insert(0, new ToolStripSeparator());
                        }
                        currentType = p.Type;
                    }
                    plugInsToolStripMenuItem.DropDownItems.Insert(0, item);
                }
            }
        }

        void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            KeysConverter kc = new KeysConverter();
            string keyChar = kc.ConvertToString(e.KeyData);
            if (keyChar == "Enter")
            {
                if (listView1.SelectedItems.Count == 1)
                {
                    if (Model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                    {
                        GameInformation game = Model.GameData[listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')];
                        Model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                        RegisterStopHotKey(Model);
    
                        launcher.LaunchCemu(this, Model, game, false, false, ModifierKeys == Keys.Shift);
                        e.Handled = true;
                    }
                }
            }
            else
            {
                e.Handled = FindMyString(keyChar);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.ColumnToSort)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.OrderOfSort == SortOrder.Ascending)
                {
                    lvwColumnSorter.OrderOfSort = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.OrderOfSort = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.ColumnToSort = e.Column;
                lvwColumnSorter.OrderOfSort = SortOrder.Ascending;
                if (e.Column == 13 || e.Column == 14 || e.Column == 15)
                {
                    lvwColumnSorter.SortType = 1;
                }
                else if (e.Column == 5)
                {
                    lvwColumnSorter.SortType = 2;
                }
                else if (e.Column == 12)
                {
                    lvwColumnSorter.SortType = 3;
                }
                else
                {
                    lvwColumnSorter.SortType = 0;
                }
            }

            Model.Settings.CurrentSortColumn = e.Column;
            Model.Settings.CurrentSortDirection = lvwColumnSorter.OrderOfSort == SortOrder.Ascending ? 0 : 1;

            // Perform the sort with these new sort options.
            listView1.Sort();

            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (i % 2 == 0)
                {
                    listView1.Items[i].BackColor = Color.FromArgb(240, 240, 240);
                }
                else
                {
                    listView1.Items[i].BackColor = Color.White;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == UsbNotification.WmDevicechange)
            {
                switch ((int)m.WParam)
                {
                    case UsbNotification.DbtDeviceremovecomplete:
                    case UsbNotification.DbtDevicearrival:
                        PopulateListView();
                        break;

                }
            }
            if (m.Msg == 0x0312)
            {
                launcher.KillCurrentProcess();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if ((e.Item.SubItems[9] == e.SubItem))
            {
                e.DrawDefault = false;
                e.DrawBackground();
                e.DrawFocusRectangle(e.SubItem.Bounds);
                if ((e.ItemState & ListViewItemStates.Selected) == ListViewItemStates.Selected)
                {
                    Rectangle r = new Rectangle(e.SubItem.Bounds.Left, e.SubItem.Bounds.Top, e.SubItem.Bounds.Width, e.SubItem.Bounds.Height);
                    e.Graphics.FillRectangle(SystemBrushes.Highlight, r);
                    e.SubItem.ForeColor = SystemColors.HighlightText;
                }
                else
                {
                    e.SubItem.ForeColor = SystemColors.WindowText;
                }

                int x = e.SubItem.Bounds.Location.X + (e.SubItem.Bounds.Width / 2) - (imageList1.Images[0].Width / 2);
                int y = e.SubItem.Bounds.Location.Y + (e.SubItem.Bounds.Height / 2) - (imageList1.Images[0].Height / 2);

                int imageIndex = 5;
                imageIndex = GetStatusImageIndex(e, imageIndex);
                e.Graphics.DrawImage(imageList2.Images[imageIndex], x, y);
                DrawText(e);
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="imageIndex"></param>
        /// <returns></returns>
        private static int GetStatusImageIndex(DrawListViewSubItemEventArgs e, int imageIndex)
        {
            switch (e.SubItem.Text.Trim())
            {
                case "Perfect":
                    imageIndex = 0;
                    break;
                case "Playable":
                    imageIndex = 1;
                    break;
                case "Runs":
                    imageIndex = 2;
                    break;
                case "Loads":
                    imageIndex = 3;
                    break;
                case "Unplayable":
                    imageIndex = 4;
                    break;
            }

            return imageIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        void DrawText(DrawListViewSubItemEventArgs e)
        {
            RectangleF rect = new RectangleF(e.SubItem.Bounds.X + imageList1.Images[0].Width, e.SubItem.Bounds.Y, e.SubItem.Bounds.Width - imageList1.Images[0].Width, e.SubItem.Bounds.Height);
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(e.SubItem.Text, e.SubItem.Font, new SolidBrush(e.SubItem.ForeColor), rect, sf);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetupShowRegionMenuItems()
        {
            usaToolStripMenuItem.Checked = Model.Filters.ViewRegionUsa;
            usaToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            europeToolStripMenuItem.Checked = Model.Filters.ViewRegionEur;
            europeToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            japanToolStripMenuItem .Checked = Model.Filters.ViewRegionJap;
            japanToolStripMenuItem.Click += UsaToolStripMenuItem_Click;

            wiiUToolStripMenuItem.Checked = Model.Filters.ViewTypeWiiU;
            wiiUToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            eShopToolStripMenuItem.Checked = Model.Filters.ViewTypeEshop;
            eShopToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            channelToolStripMenuItem.Checked = Model.Filters.ViewTypeChannel;
            channelToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            virtualConsoleToolStripMenuItem.Checked = Model.Filters.ViewTypeVc;
            virtualConsoleToolStripMenuItem.Click += UsaToolStripMenuItem_Click;

            rating5ToolStripMenuItem.Checked = Model.Filters.ViewRating5;
            rating5ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            rating4ToolStripMenuItem.Checked = Model.Filters.ViewRating4;
            rating4ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            rating3ToolStripMenuItem.Checked = Model.Filters.ViewRating3;
            rating3ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            rating2ToolStripMenuItem.Checked = Model.Filters.ViewRating2;
            rating2ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            rating1ToolStripMenuItem.Checked = Model.Filters.ViewRating1;
            rating1ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;

            perfectToolStripMenuItem.Checked = Model.Filters.ViewStatusPerfect;
            perfectToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            playableToolStripMenuItem.Checked = Model.Filters.ViewStatusPlayable;
            playableToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            runsToolStripMenuItem.Checked = Model.Filters.ViewStatusRuns;
            runsToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            loadsToolStripMenuItem.Checked = Model.Filters.ViewStatusLoads;
            loadsToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            unplayableToolStripMenuItem.Checked = Model.Filters.ViewStatusUnplayable;
            unplayableToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            notSetToolStripMenuItem.Checked = Model.Filters.ViewStatusNotSet;
            notSetToolStripMenuItem.Click += UsaToolStripMenuItem_Click;

            officiallyPerfectToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusPerfect;
            officiallyPerfectToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyPlayableToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusPlayable;
            officiallyPlayableToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyRunsToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusRuns;
            officiallyRunsToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyLoadsToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusLoads;
            officiallyLoadsToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyUnplayableToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusUnplayable;
            officiallyUnplayableToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyNotSetToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusNotSet;
            officiallyNotSetToolStripMenuItem.Click += UsaToolStripMenuItem_Click;           

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UsaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;

            Model.Filters.ViewRegionUsa = usaToolStripMenuItem.Checked;
            Model.Filters.ViewRegionEur = europeToolStripMenuItem.Checked;
            Model.Filters.ViewRegionJap = japanToolStripMenuItem.Checked;

            Model.Filters.ViewTypeWiiU = wiiUToolStripMenuItem.Checked;
            Model.Filters.ViewTypeEshop = eShopToolStripMenuItem.Checked;
            Model.Filters.ViewTypeChannel = channelToolStripMenuItem.Checked;
            Model.Filters.ViewTypeVc = virtualConsoleToolStripMenuItem.Checked;

            Model.Filters.ViewRating5 = rating5ToolStripMenuItem.Checked;
            Model.Filters.ViewRating4 = rating4ToolStripMenuItem.Checked;
            Model.Filters.ViewRating3 = rating3ToolStripMenuItem.Checked;
            Model.Filters.ViewRating2 = rating2ToolStripMenuItem.Checked;
            Model.Filters.ViewRating1 = rating1ToolStripMenuItem.Checked;

            Model.Filters.ViewStatusPerfect = perfectToolStripMenuItem.Checked;
            Model.Filters.ViewStatusPlayable = playableToolStripMenuItem.Checked;
            Model.Filters.ViewStatusRuns = runsToolStripMenuItem.Checked;
            Model.Filters.ViewStatusLoads = loadsToolStripMenuItem.Checked;
            Model.Filters.ViewStatusUnplayable = unplayableToolStripMenuItem.Checked;
            Model.Filters.ViewStatusNotSet = notSetToolStripMenuItem.Checked;

            Model.Filters.ViewOfficialStatusPerfect = officiallyPerfectToolStripMenuItem.Checked;
            Model.Filters.ViewOfficialStatusPlayable = officiallyPlayableToolStripMenuItem.Checked;
            Model.Filters.ViewOfficialStatusRuns = officiallyRunsToolStripMenuItem.Checked;
            Model.Filters.ViewOfficialStatusLoads = officiallyLoadsToolStripMenuItem.Checked;
            Model.Filters.ViewOfficialStatusUnplayable = officiallyUnplayableToolStripMenuItem.Checked;
            Model.Filters.ViewOfficialStatusNotSet = officiallyNotSetToolStripMenuItem.Checked;

            PopulateListView();
        }           

        /// <summary>
        /// 
        /// </summary>
        void AddUserMenuItems()
        {
            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
            foreach (var user in Model.Users)
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem
                {
                    Text = user.Name,
                    Tag = user
                };
                menuItem.Click += User_Click;
                items.Insert(0, menuItem);

                ToolStripMenuItem menuItem2 = new ToolStripMenuItem
                {
                    Text = user.Name,
                    Tag = user
                };
                menuItem2.Click += User_Click;
                contextMenuStrip1.Items.Add(menuItem2);
                if (user.Name == Model.CurrentUser)
                {
                    menuItem2.Checked = true;
                    menuItem.Checked = true;
                }
            }

            // Painful, but we want these added to the top of the list...
            foreach (var item in items)
            {
                userToolStripMenuItem.DropDownItems.Insert(0, item);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void User_Click(object sender, EventArgs e)
        {
            var toolStripMenuItem = sender as ToolStripMenuItem;
            if (toolStripMenuItem != null)
            {
                User user = toolStripMenuItem.Tag as User;
                if (user != null)
                {
                    if (user.Name != Model.CurrentUser)
                    {
                        DeleteAllLockFiles();
                        if (File.Exists(Path.Combine("Users", user.Image)))
                        {
                            using (FileStream stream = new FileStream(Path.Combine("Users", user.Image), FileMode.Open, FileAccess.Read))
                            {
                                pictureBox1.Image = Image.FromStream(stream);
                            }

                        }
                        Text = Resources.fMainWindow_fMainWindow_CEMU_Game_DB______Current_User__ + user.Name;
                        Model.CurrentUser = user.Name;
                    }
                }
                UpdateMenuStrip(user);
                UpdateContextMenuStrip(user);
            }
        }

        private void DeleteAllLockFiles()
        {
            CopySaves cs = new CopySaves(Model);
            cs.Execute();
            foreach (var game in Model.GameData.OrderByDescending(gd => gd.Value.Name))
            {
                foreach (var version in Model.Settings.InstalledVersions)
                {
                    DirectoryInfo dest = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirCemu(version, game.Value));

                    string lockFileName = Path.Combine(dest.FullName, "Budford.lck");
                    if (File.Exists(lockFileName))
                    {
                        try
                        {
                            File.Delete(lockFileName);
                        }
                        catch (Exception)
                        {
                            // ignored
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
        private void PlugIn_Click(object sender, EventArgs e)
        {
            var toolStripMenuItem = sender as ToolStripMenuItem;
            if (toolStripMenuItem != null)
            {
                Model.PlugIns.PlugIn plugIn = toolStripMenuItem.Tag as Model.PlugIns.PlugIn;
                if (plugIn != null)
                {
                    if (plugIn.Type == "ExternalTool")
                    {
                        ProcessStartInfo start = new ProcessStartInfo {FileName = plugIn.FileName};
                        Process.Start(start);
                    }
                    else
                    {
                        using (FormExecutePlugIn executor = new FormExecutePlugIn(Model, plugIn))
                        {
                            if (executor.ShowDialog(this) == DialogResult.OK)
                            {
                                MessageBox.Show(plugIn.Name + Resources.FormMainWindow_PlugIn_Click__executed_successfully, Resources.FormMainWindow_PlugIn_Click_Success);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        private void UpdateContextMenuStrip(User user)
        {
            foreach (ToolStripItem menu in contextMenuStrip1.Items)
            {
                var item = menu as ToolStripMenuItem;
                if (item != null)
                {
                    item.Checked = false;
                }
                if (menu.Text == Model.CurrentUser)
                {
                    ((ToolStripMenuItem)menu).Checked = true;
                    if (user != null)
                    {
                        menu.Text = user.Name;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        private void UpdateMenuStrip(User user)
        {
            foreach (ToolStripItem menu in userToolStripMenuItem.DropDownItems)
            {
                var item = menu as ToolStripMenuItem;
                if (item != null)
                {
                    item.Checked = false;
                }
                if (menu.Text == Model.CurrentUser)
                {
                    ((ToolStripMenuItem)menu).Checked = true;
                    if (user != null)
                    {
                        menu.Text = user.Name;
                    }
                }
            }
        }       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            InitialiseListView();

            PopulateListView();

            ResizeColumnHeaders();
        }

        /// <summary>
        /// 
        /// </summary>
        private void PopulateListView()
        {
            listView1.BeginUpdate();
            try
            {
                listView1.Items.Clear();
                FolderScanner.CheckGames(Model);

                foreach (var game in Model.GameData.OrderByDescending(gd => gd.Value.Name))
                {
                    if (FilterCheckedOut(game.Value))
                    {
                        ListViewItem lvi = new ListViewItem();
                        PopulateSubItems(game, lvi);

                        lvi.ImageIndex = GetRegionImageIndex(game);
                        lvi.Tag = game.Value;

                        listView1.Items.Add(lvi);

                        if (!game.Value.Exists)
                        {
                            lvi.ForeColor = Color.Gray;
                        }
                        if (game.Value.LaunchFile.Contains("Ⅱ"))
                        {
                            lvi.ForeColor = Color.Red;
                        }
                        if (game.Value.LaunchFile.Contains("™"))
                        {
                            lvi.ForeColor = Color.Red;
                        }

                    }
                }

                for (int i = 0; i < listView1.Items.Count; i += 2)
                {
                    listView1.Items[i].BackColor = Color.FromArgb(240, 240, 240);
                }

                ResizeColumns();

                toolStripStatusLabel3.Text = Resources.FormMainWindow_PopulateListView_Currently_showing_ + listView1.Items.Count + (listView1.Items.Count == 1 ? " Game" : " Games");
            }
            finally
            {
                listView1.EndUpdate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ResizeColumns()
        {
            try
            {
                if (listView1.Columns.Count > 0)
                {
                    listView1.Columns[0].Width = 36;
                    if (Model.Settings.AutoSizeColumns)
                    {
                        for (int c = 4; c < listView1.Columns.Count; ++c)
                        {
                            if (!listView1.IsDisposed)
                            {
                                listView1.AutoResizeColumn(c, ColumnHeaderAutoResizeStyle.ColumnContent);
                                listView1.AutoResizeColumn(c, ColumnHeaderAutoResizeStyle.HeaderSize);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static int GetRegionImageIndex(KeyValuePair<string, GameInformation> game)
        {
            switch (game.Value.Region)
            {
                case "EUR": return 0;
                case "JPN": return 1;
                case "USA": return 2;
                default: return 4;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="lvi"></param>
        /// <returns></returns>
        private void PopulateSubItems(KeyValuePair<string, GameInformation> game, ListViewItem lvi)
        {
            lvi.SubItems.Add(game.Value.Name);
            lvi.SubItems.Add(game.Value.Region + "     ");
            if (comments)
            {
                lvi.SubItems.Add(game.Value.Comments);
            }
            else
            {
                lvi.SubItems.Add(game.Value.Publisher);
            }
            lvi.SubItems.Add(game.Value.ProductCode.Replace("WUP-P-", "").Replace("WUP-U-", "").Replace("WUP-N-", "") + game.Value.CompanyCode + "       ");
            lvi.SubItems.Add(game.Value.Size);
            lvi.SubItems.Add(game.Value.LaunchFileName);
            lvi.SubItems.Add(game.Value.GameSetting.PreferedVersion + "               ");
            if (game.Value.GameSetting.OfficialEmulationState == game.Value.GameSetting.PreviousOfficialEmulationState)
            {
                lvi.SubItems.Add(game.Value.GameSetting.OfficialEmulationState + "        ");
            }
            else
            {
                lvi.SubItems.Add(game.Value.GameSetting.OfficialEmulationState + " <- " + game.Value.GameSetting.PreviousOfficialEmulationState);
            }
            lvi.SubItems.Add(game.Value.GameSetting.EmulationState + "        ");
            lvi.SubItems.Add(game.Value.SaveDir.Trim());
            lvi.SubItems.Add(game.Value.Type.Trim() + " ");
            lvi.SubItems.Add(game.Value.LastPlayed != DateTime.MinValue ? game.Value.LastPlayed.ToShortDateString() + " " : "                    ");
            lvi.SubItems.Add(game.Value.PlayCount != 0 ? game.Value.PlayCount + "                 " : "                 ");
            lvi.SubItems.Add(game.Value.GraphicsPacksCount != 0 ? game.Value.GraphicsPacksCount + "                    " : "                    ");
            lvi.SubItems.Add(game.Value.Rating + "                 "); 
            lvi.SubItems.Add(game.Value.Comments + "                 ");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private bool FilterCheckedOut(GameInformation game)
        {
            if (!Model.Settings.IncludeWiiULauncherRpx)
            {
                if (game.LaunchFile.Contains("WiiULauncher.rpx"))
                {
                    return false;
                }
            }

            if (CheckRegionFilter(game))
            {
                if (CheckStatusFilter(game))
                {
                    if (CheckOfficialStatusFilter(game))
                    {
                        if (CheckRatingFilter(game))
                        {
                            return CheckTypeFilter(game);
                        }
                    }
                }
            }
           
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private bool CheckStatusFilter(GameInformation game)
        {
            switch (game.GameSetting.EmulationState)
            {
                case GameSettings.EmulationStateType.NotSet:
                    if (!Model.Filters.ViewStatusNotSet) return false;
                    break;
                case GameSettings.EmulationStateType.Perfect:
                    if (!Model.Filters.ViewStatusPerfect) return false;
                    break;
                case GameSettings.EmulationStateType.Playable:
                    if (!Model.Filters.ViewStatusPlayable) return false;
                    break;
                case GameSettings.EmulationStateType.Runs:
                    if (!Model.Filters.ViewStatusRuns) return false;
                    break;
                case GameSettings.EmulationStateType.Loads:
                    if (!Model.Filters.ViewStatusLoads) return false;
                    break;
                case GameSettings.EmulationStateType.Unplayable:
                    if (!Model.Filters.ViewStatusUnplayable) return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private bool CheckOfficialStatusFilter(GameInformation game)
        {
            switch (game.GameSetting.OfficialEmulationState)
            {
                case GameSettings.EmulationStateType.NotSet:
                    if (!Model.Filters.ViewOfficialStatusNotSet) return false;
                    break;
                case GameSettings.EmulationStateType.Perfect:
                    if (!Model.Filters.ViewOfficialStatusPerfect) return false;
                    break;
                case GameSettings.EmulationStateType.Playable:
                    if (!Model.Filters.ViewOfficialStatusPlayable) return false;
                    break;
                case GameSettings.EmulationStateType.Runs:
                    if (!Model.Filters.ViewOfficialStatusRuns) return false;
                    break;
                case GameSettings.EmulationStateType.Loads:
                    if (!Model.Filters.ViewOfficialStatusLoads) return false;
                    break;
                case GameSettings.EmulationStateType.Unplayable:
                    if (!Model.Filters.ViewOfficialStatusUnplayable) return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private bool CheckRegionFilter(GameInformation game)
        {
            switch (game.Region)
            {
                case "USA":
                    if (!Model.Filters.ViewRegionUsa) return false;
                    break;
                case "EUR":
                    if (!Model.Filters.ViewRegionEur) return false;
                    break;
                case "JAP":
                    if (!Model.Filters.ViewRegionJap) return false;
                    break;
                case "ALL":
                    if (!Model.Filters.ViewRegionAll) return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private bool CheckTypeFilter(GameInformation game)
        {
            switch (game.Type)
            {
                case "WiiU":
                    if (!Model.Filters.ViewTypeWiiU) return false;
                    break;
                case "eShop":
                    if (!Model.Filters.ViewTypeEshop) return false;
                    break;
                case "Channel":
                    if (!Model.Filters.ViewTypeChannel) return false;
                    break;
                default:
                    if (!Model.Filters.ViewTypeVc) return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private bool CheckRatingFilter(GameInformation game)
        {
            switch (game.Rating)
            {
                case 5:
                    if (!Model.Filters.ViewRating5) return false;
                    break;
                case 4:
                    if (!Model.Filters.ViewRating4) return false;
                    break;
                case 3:
                    if (!Model.Filters.ViewRating3) return false;
                    break;
                case 2:
                    if (!Model.Filters.ViewRating2) return false;
                    break;
                case 1:
                    if (!Model.Filters.ViewRating1) return false;
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitialiseListView()
        {
            listView1.View = System.Windows.Forms.View.Details;
            listView1.SmallImageList = imageList1;

            listView1.Columns.Add(" ", 40);
            listView1.Columns.Add("Title", 200);
            listView1.Columns.Add("Region", 50);
            listView1.Columns.Add("Publisher", 100);
            listView1.Columns.Add("Product Code", 100);
            listView1.Columns.Add("Size", 75, HorizontalAlignment.Right);
            listView1.Columns.Add("Launcher", 100);
            listView1.Columns.Add("Cemu Version", 100);
            listView1.Columns.Add("Official Status", 100);
            listView1.Columns.Add("Status", 100);
            listView1.Columns.Add("SaveDir", 75);
            listView1.Columns.Add("Type", 50);
            listView1.Columns.Add("Last Played", 50);
            listView1.Columns.Add("Play Count", 50);
            listView1.Columns.Add("Graphics Packs", 50);
            listView1.Columns.Add("Rating", 50);
            listView1.Columns.Add("Comment", 50);

            listView1.HeaderStyle = ColumnHeaderStyle.Clickable;

            listView1.FullRowSelect = true;
            listView1.MultiSelect = false;
            listView1.Sorting = SortOrder.Ascending;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ResizeColumnHeaders()
        {
            if (Model != null)
            {
                if (Model.GameData.Count > 0)
                {
                    if (Model.Settings.AutoSizeColumns)
                    {
                        for (int i = 4; i < listView1.Columns.Count; i++)
                        {
                            listView1.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                        }

                        if (listView1.Columns.Count > 1)
                        {
                            listView1.Columns[listView1.Columns.Count - 1].Width = -2;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ResizeColumnHeaders();
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
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            LaunchGame();
        }

        private void LaunchGame()
        {
            if (toolStripButton3.Enabled)
            {
                if (listView1.SelectedItems.Count == 1)
                {
                    if (Model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                    {
                        EnableControlsForGameRunning();


                        GameInformation game = Model.GameData[listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')];
                        Model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                        RegisterStopHotKey(Model);

                        launcher.LaunchCemu(this, Model, game, false, false, ModifierKeys == Keys.Shift);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void UpdateSaveDir(string savedDir)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => { UpdateSaveDir(savedDir); }));
            }
            else
            {
                ListViewItem lvi =  listView1.FindItemWithText(Model.CurrentId, true, 0);
                if (lvi != null)
                { 
                    lvi.SubItems[10].Text = savedDir;
                    ResizeColumnHeaders();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void RefreshList(GameInformation game)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => { RefreshList(game); }));
            }
            else
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    if (comments)
                    {
                        listView1.SelectedItems[0].SubItems[3].Text = game.Comments;
                    }
                    listView1.SelectedItems[0].SubItems[7].Text = game.GameSetting.PreferedVersion;
                    listView1.SelectedItems[0].SubItems[8].Text = game.GameSetting.OfficialEmulationState.ToString();
                    listView1.SelectedItems[0].SubItems[9].Text = game.GameSetting.EmulationState.ToString();
                    listView1.SelectedItems[0].SubItems[12].Text = game.LastPlayed != DateTime.MinValue ? game.LastPlayed.ToShortDateString() + " " : "                    ";
                    listView1.SelectedItems[0].SubItems[13].Text = game.PlayCount != 0 ? game.PlayCount + "                 " : "                 ";
                    listView1.SelectedItems[0].SubItems[15].Text = game.Rating + Resources.FormMainWindow_RefreshList__________________;
                    listView1.SelectedItems[0].SubItems[16].Text = game.Comments + Resources.FormMainWindow_RefreshList__________________;
                }
            }
        }      
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (launcher.Open(Model))
            {
                RegisterStopHotKey(Model);
                EnableControlsForGameRunning();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            LaunchConfigurationForm(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabPageIndex"></param>
        private void LaunchConfigurationForm(int tabPageIndex)
        {
            using (FormEditConfiguration configurationForm = new FormEditConfiguration(Model, tabPageIndex))
            {
                List<string> oldRomFolder = new List<string>(Model.Settings.RomFolders.ToArray());
                configurationForm.ShowDialog(this);
                if (oldRomFolder.Count == Model.Settings.RomFolders.Count)
                {
                    for (int i = 0; i < oldRomFolder.Count; ++i)
                    {
                        if (oldRomFolder[i] != Model.Settings.RomFolders[i])
                        {
                            RefreshGameList();
                            break;
                        }
                    }
                }
                else
                {
                    RefreshGameList();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void RefreshGameList()
        {
            foreach (var folder in Model.Settings.RomFolders)
            {
                using (FormScanRomFolder scanner = new FormScanRomFolder(Model, folder, Model.GameData))
                {
                    scanner.ShowDialog(this);
                }
            }

            Persistence.SetSaveDirs(Model);
            Persistence.SetGameTypes(Model);
            FolderScanner.AddGraphicsPacksToGames(Model);

            PopulateListView();
            ResizeColumnHeaders();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            launcher.KillCurrentProcess();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            User user = Model.Users.FirstOrDefault(u => u.Name == Model.CurrentUser);
            using (FormEditUser editUser = new FormEditUser(user))
            {
                editUser.ShowDialog(this);
            }

            if (user != null && File.Exists(Path.Combine("Users", user.Image)))
            {
                using (FileStream stream = new FileStream(Path.Combine("Users", user.Image), FileMode.Open, FileAccess.Read))
                {
                    pictureBox1.Image = Image.FromStream(stream);
                }
            }

            if (user != null && Model.CurrentUser != user.Name)
            {
                if (Directory.Exists(Path.Combine("Users", Model.CurrentUser)))
                {
                    try
                    {
                        Directory.Move(Path.Combine("Users", Model.CurrentUser), Path.Combine("Users", user.Name));
                    }
                    catch (Exception ex)
                    {
                        Model.Errors.Add(ex.Message);
                    }
                }
                UpdateMenuStrip(user);
                UpdateContextMenuStrip(user);
              
                Model.CurrentUser = user.Name;
                Text = Resources.fMainWindow_fMainWindow_CEMU_Game_DB______Current_User__ + Model.CurrentUser;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            User newUser = new User() { Name = "User " + (Model.Users.Count + 1), Image = "default.png" };
            using (FormEditUser editUser = new FormEditUser(newUser))
            {
                if (editUser.ShowDialog(this) == DialogResult.OK)
                {
                    Model.Users.Add(newUser);
                    ToolStripMenuItem menuItem = new ToolStripMenuItem
                    {
                        Text = newUser.Name,
                        Tag = newUser
                    };
                    menuItem.Click += User_Click;
                    userToolStripMenuItem.DropDownItems.Insert(0, menuItem);

                    ToolStripMenuItem menuItem2 = new ToolStripMenuItem
                    {
                        Text = newUser.Name,
                        Tag = newUser
                    };
                    menuItem2.Click += User_Click;
                    contextMenuStrip1.Items.Add(menuItem2);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormAbout aboutBox = new FormAbout())
            {
                aboutBox.ShowDialog(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Game Properties
            if (listView1.SelectedItems.Count == 1)
            {
                if (listView1.SelectedItems.Count == 1)
                {
                    Model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                    var game = Model.GameData[Model.CurrentId];
                    try
                    {
                        using (FormEditGameSettings aboutBox = new FormEditGameSettings(game, Model.Settings.InstalledVersions))
                        {
                            aboutBox.ShowDialog(this);
                            if (listView1.SelectedItems.Count == 1)
                            {
                                RefreshList(game);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openSaveFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Save location
            if (listView1.SelectedItems.Count == 1)
            {
                Model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');

                if (GetCurrentVersion().VersionNumber < 1110)
                {
                    if (!Directory.Exists(Path.Combine(GetCurrentVersion().Folder,  "mlc01", "emulatorSave",  Model.GameData[Model.CurrentId].SaveDir)))
                    {
                        Directory.CreateDirectory(Path.Combine(GetCurrentVersion().Folder, "mlc01", "emulatorSave", Model.GameData[Model.CurrentId].SaveDir));
                    }
                    Process.Start(Path.Combine(GetCurrentVersion().Folder, "mlc01", "emulatorSave", Model.GameData[Model.CurrentId].SaveDir));
                }
                else
                {
                    string gameId = Model.GameData[Model.CurrentId].TitleId.Replace("00050000", "");

                    if (!Directory.Exists(Path.Combine(GetCurrentVersion().Folder, "mlc01", "usr", "save", "00050000", gameId, "user")))
                    {
                        Directory.CreateDirectory(Path.Combine(GetCurrentVersion().Folder, "mlc01", "usr", "save", "00050000", gameId, "user"));
                    }
                    Process.Start(Path.Combine(GetCurrentVersion().Folder, "mlc01", "usr", "save", "00050000", gameId, "user"));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Containing Folder
            if (listView1.SelectedItems.Count == 1)
            {
                Model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                if (Directory.Exists(Path.Combine(Path.GetDirectoryName(Model.GameData[Model.CurrentId].LaunchFile), "..", "..")))
                {
                    Process.Start((Path.Combine(Path.GetDirectoryName(Model.GameData[Model.CurrentId].LaunchFile), "..", "..")));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openCEMUShaderCacheFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                Model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                if (Directory.Exists(Path.Combine(GetCurrentVersion().Folder, "shaderCache", "transferable")))
                {
                    if (File.Exists(Path.Combine(GetCurrentVersion().Folder, "shaderCache", "transferable", Model.GameData[Model.CurrentId].SaveDir + ".bin")))
                    {
                        Process.Start("explorer.exe", "/select, " + Path.Combine(GetCurrentVersion().Folder, "shaderCache", "transferable", Model.GameData[Model.CurrentId].SaveDir + ".bin"));
                    }
                    else
                    {
                        Process.Start("explorer.exe", "/select, " + Path.Combine(GetCurrentVersion().Folder, "shaderCache", "transferable"));
                    }
                }
                else
                {
                    MessageBox.Show(Resources.fMainWindow_openSaveFileLocationToolStripMenuItem_Click_, Resources.fMainWindow_openSaveFileLocationToolStripMenuItem_Click_Folder_not_found, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fMainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Persistence.Save(Model, GetModelFileName());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showStatusToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            showStatusToolStripMenuItem1.Checked = !showStatusToolStripMenuItem1.Checked;
            statusStrip1.Visible = showStatusToolStripMenuItem1.Checked;
            Model.Settings.ShowStausBar = showStatusToolStripMenuItem1.Checked;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showToolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showToolbarToolStripMenuItem.Checked = !showToolbarToolStripMenuItem.Checked;
            toolStrip1.Visible = showToolbarToolStripMenuItem.Checked;
            toolStrip1.Visible = showToolbarToolStripMenuItem.Checked;
            Model.Settings.ShowToolBar = showToolbarToolStripMenuItem.Checked;             
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void installVS2015RedistributablesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            WindowsOs.IsVc2013RedistInstalled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void installVS2015RedistributablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowsOs.IsVc2015RedistInstalled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void installVS2012RedistributablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowsOs.IsVc2012RedistInstalled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void installVS2012RedistributablesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            WindowsOs.IsVc2010RedistInstalled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void installVS2012RedistributablesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            WindowsOs.IsVc2008RedistInstalled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void installVS2012RedistributablesToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            WindowsOs.IsVc2005RedistInstalled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dumpSaveDirCodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Persistence.Save(Model, GetModelFileName());
            MessageBox.Show(Resources.fMainWindow_dumpSaveDirCodesToolStripMenuItem_Click_SaveDirs_saved_successfully, Resources.fMainWindow_dumpSaveDirCodesToolStripMenuItem_Click_Success, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importShaderCacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Configure open file dialog box 
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = Resources.fMainWindow_importShaderCacheToolStripMenuItem_Click_Shader_Cache_Files_____bin_;

                // Show open file dialog box 
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    FileManager.ImportShaderCache(this, Model, dlg.FileName);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            RefreshGameList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            // Play
            LaunchGame();
        }

        Keys GetHotKey(string key)
        {
            var values = Enum.GetValues(typeof(Keys));
            foreach (Keys value in values)
            {
                if (value.ToString() == key)
                {
                    return value;
                }
            }
            return Keys.None;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Model.Settings.CurrentView = "Tiled";
            tToolStripMenuItem.Checked = true;
            detailsToolStripMenuItem.Checked = false;
            tableLayoutPanel1.RowStyles[0].Height = 0;
            tableLayoutPanel1.RowStyles[1].Height = tableLayoutPanel1.Parent.Height;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Model.Settings.CurrentView = "Detailed";
            tToolStripMenuItem.Checked = false;
            detailsToolStripMenuItem.Checked = true;
            tableLayoutPanel1.RowStyles[1].Height = 0;
            tableLayoutPanel1.RowStyles[0].Height = tableLayoutPanel1.Parent.Height;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkDLCFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            JunctionPoint.Create(@"C:\Games\Emulators\cemu_1.8.2b\mlc01\usr\title", @"C:\Games\Emulators\cemu_1.9.1\mlc01\usr\title", true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            LaunchConfigurationForm(1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            LaunchConfigurationForm(2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        InstalledVersion GetCurrentVersion()
        {
            if (listView1.SelectedItems.Count == 1)
            {
                Model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                var setting = Model.GameData[Model.CurrentId].GameSetting;
                InstalledVersion version;
                if (setting.PreferedVersion != "Latest")
                {
                    version = Model.Settings.InstalledVersions.FirstOrDefault(v => v.Name == setting.PreferedVersion);
                }
                else
                {
                    version = Model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
                }

                return version;
            }
            return Model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void launchCemuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RegisterStopHotKey(Model);
            launcher.LaunchCemu(this, Model, null, false, true);
        }

        public void RegisterStopHotKey(Model.Model model)
        {
            if (model.Settings.StopHotkey != "None")
            {
                Keys key = GetHotKey(model.Settings.StopHotkey);
                RegisterHotKey(Handle, MyactionHotkeyId, (int)KeyModifier.None, key.GetHashCode());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadCompatabilityStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var game in Model.GameData)
            {
                game.Value.GameSetting.PreviousOfficialEmulationState = game.Value.GameSetting.OfficialEmulationState;
            }
            CemuFeatures.DownloadCompatibilityStatus(this, Model);
            foreach (var game in Model.GameData)
            {
                if (game.Value.GameSetting.PreviousOfficialEmulationState != game.Value.GameSetting.OfficialEmulationState)
                {
                    MessageBox.Show(Resources.FormMainWindow_downloadCompatabilityStatusToolStripMenuItem_Click_, Resources.FormMainWindow_downloadCompatabilityStatusToolStripMenuItem_Click_Exciting_news);
                    break;
                }
            }
            PopulateListView();
            ResizeColumnHeaders();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fixUnityGameSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var v in Model.GameData)
            {
                GameInformation gi = v.Value;

                if (gi.GameSetting.EmulationState == GameSettings.EmulationStateType.Unplayable)
                {
                    gi.GameSetting.ControllerOverride2 = 5;
                    gi.GameSetting.ControllerOverride3 = 5;
                    gi.GameSetting.ControllerOverride4 = 5;
                    gi.GameSetting.ControllerOverride5 = 5;
                    gi.GameSetting.ControllerOverride6 = 5;
                    gi.GameSetting.ControllerOverride7 = 5;
                    gi.GameSetting.ControllerOverride8 = 5;
                }
          
            }
            PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchString"></param>
        private bool FindMyString(string searchString)
        {
            // Ensure we have a proper string to search for.
            if (searchString != string.Empty)
            {
                int s = 0;
                for (int i = 0; i < listView1.Items.Count; ++i)
                {
                    if (listView1.Items[i].SubItems[1].Text.ToLower().StartsWith(searchString.ToLower()))
                    {
                        if (listView1.Items[i].Selected)
                        {
                            s = i + 1;
                            break;
                        }
                    }
                }

                for (int i = s; i < listView1.Items.Count; ++i)
                {
                    if (listView1.Items[i].SubItems[1].Text.ToLower().StartsWith(searchString.ToLower()))
                    {
                        listView1.Items[i].Selected = true;
                        listView1.Items[i].Focused = true;
                        listView1.Items[i].EnsureVisible();
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void manageInstalledVersionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormEditInstalledVersions installedVersions = new FormEditInstalledVersions(Model, unpacker, launcher))
            {
                installedVersions.ShowDialog(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadLatestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileManager.DownloadCemu(this, unpacker, Model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copySavesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopySaves cs = new CopySaves(Model);
            cs.Execute();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyShadersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyShaderCache cs = new CopyShaderCache(Model);
            cs.Execute();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dumpTestingResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var cv = GetCurrentVersion();
            if (cv != null)
            {
                foreach (var gd in Model.GameData)
                {
                    GameInformation gi = gd.Value;
                    gi.GameSetting.UseRtdsc = 1;
                }
            }     
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void allToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // All
            Model.Filters.ViewStatusPerfect = true;
            Model.Filters.ViewStatusPlayable = true;
            Model.Filters.ViewStatusRuns = true;
            Model.Filters.ViewStatusLoads = true;
            Model.Filters.ViewStatusUnplayable = true;
            Model.Filters.ViewStatusNotSet = true;

            perfectToolStripMenuItem.Checked = Model.Filters.ViewStatusPerfect;
            playableToolStripMenuItem.Checked = Model.Filters.ViewStatusPlayable;
            runsToolStripMenuItem.Checked = Model.Filters.ViewStatusRuns;
            loadsToolStripMenuItem.Checked = Model.Filters.ViewStatusLoads;
            unplayableToolStripMenuItem.Checked = Model.Filters.ViewStatusUnplayable;
            notSetToolStripMenuItem.Checked = Model.Filters.ViewStatusNotSet;

            PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // None
            Model.Filters.ViewStatusPerfect = false;
            Model.Filters.ViewStatusPlayable = false;
            Model.Filters.ViewStatusRuns = false;
            Model.Filters.ViewStatusLoads = false;
            Model.Filters.ViewStatusUnplayable = false;
            Model.Filters.ViewStatusNotSet = false;

            perfectToolStripMenuItem.Checked = Model.Filters.ViewStatusPerfect;
            playableToolStripMenuItem.Checked = Model.Filters.ViewStatusPlayable;
            runsToolStripMenuItem.Checked = Model.Filters.ViewStatusRuns;
            loadsToolStripMenuItem.Checked = Model.Filters.ViewStatusLoads;
            unplayableToolStripMenuItem.Checked = Model.Filters.ViewStatusUnplayable;
            notSetToolStripMenuItem.Checked = Model.Filters.ViewStatusNotSet;

            PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void allToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            // Officially all
            Model.Filters.ViewOfficialStatusPerfect = true;
            Model.Filters.ViewOfficialStatusPlayable = true;
            Model.Filters.ViewOfficialStatusRuns = true;
            Model.Filters.ViewOfficialStatusLoads = true;
            Model.Filters.ViewOfficialStatusUnplayable = true;
            Model.Filters.ViewOfficialStatusNotSet = true;

            officiallyPerfectToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusPerfect;
            officiallyPlayableToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusPlayable;
            officiallyRunsToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusRuns;
            officiallyLoadsToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusLoads;
            officiallyUnplayableToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusUnplayable;
            officiallyNotSetToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusNotSet;

            PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void noneToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            // Officially none
            Model.Filters.ViewOfficialStatusPerfect = false;
            Model.Filters.ViewOfficialStatusPlayable = false;
            Model.Filters.ViewOfficialStatusRuns = false;
            Model.Filters.ViewOfficialStatusLoads = false;
            Model.Filters.ViewOfficialStatusUnplayable = false;
            Model.Filters.ViewOfficialStatusNotSet = false;

            officiallyPerfectToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusPerfect;
            officiallyPlayableToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusPlayable;
            officiallyRunsToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusRuns;
            officiallyLoadsToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusLoads;
            officiallyUnplayableToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusUnplayable;
            officiallyNotSetToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusNotSet;

            PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void allToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // All regions
            Model.Filters.ViewRegionUsa = true;
            Model.Filters.ViewRegionEur = true;
            Model.Filters.ViewRegionJap = true;

            usaToolStripMenuItem.Checked = Model.Filters.ViewRegionUsa;
            europeToolStripMenuItem.Checked = Model.Filters.ViewRegionEur;
            japanToolStripMenuItem.Checked = Model.Filters.ViewRegionJap;

            PopulateListView();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void noneToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // No regions
            Model.Filters.ViewRegionUsa = false;
            Model.Filters.ViewRegionEur = false;
            Model.Filters.ViewRegionJap = false;

            usaToolStripMenuItem.Checked = Model.Filters.ViewRegionUsa;
            europeToolStripMenuItem.Checked = Model.Filters.ViewRegionEur;
            japanToolStripMenuItem.Checked = Model.Filters.ViewRegionJap;

            PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void allToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            // All types
            Model.Filters.ViewTypeWiiU = true;
            Model.Filters.ViewTypeEshop = true;
            Model.Filters.ViewTypeChannel = true;
            Model.Filters.ViewTypeVc = true;

            wiiUToolStripMenuItem.Checked = Model.Filters.ViewTypeWiiU;
            eShopToolStripMenuItem.Checked = Model.Filters.ViewTypeEshop;
            channelToolStripMenuItem.Checked = Model.Filters.ViewTypeChannel;
            virtualConsoleToolStripMenuItem.Checked = Model.Filters.ViewTypeVc;

            PopulateListView();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void noneToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            // No types
            Model.Filters.ViewTypeWiiU = false;
            Model.Filters.ViewTypeEshop = false;
            Model.Filters.ViewTypeChannel = false;
            Model.Filters.ViewTypeVc = false;

            wiiUToolStripMenuItem.Checked = Model.Filters.ViewTypeWiiU;
            eShopToolStripMenuItem.Checked = Model.Filters.ViewTypeEshop;
            channelToolStripMenuItem.Checked = Model.Filters.ViewTypeChannel;
            virtualConsoleToolStripMenuItem.Checked = Model.Filters.ViewTypeVc;

            PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void allToolStripMenuItem4_Click(object sender, EventArgs e)
        {
             // No types
            Model.Filters.ViewRating5 = true;
            Model.Filters.ViewRating4 = true;
            Model.Filters.ViewRating3 = true;
            Model.Filters.ViewRating2 = true;
            Model.Filters.ViewRating1 = true;

            rating5ToolStripMenuItem.Checked = Model.Filters.ViewRating5;
            rating4ToolStripMenuItem.Checked = Model.Filters.ViewRating4;
            rating3ToolStripMenuItem.Checked = Model.Filters.ViewRating3;
            rating2ToolStripMenuItem.Checked = Model.Filters.ViewRating2;
            rating1ToolStripMenuItem.Checked = Model.Filters.ViewRating1;

            PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void noneToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            Model.Filters.ViewRating5 = false;
            Model.Filters.ViewRating4 = false;
            Model.Filters.ViewRating3 = false;
            Model.Filters.ViewRating2 = false;
            Model.Filters.ViewRating1 = false;

            rating5ToolStripMenuItem.Checked = Model.Filters.ViewRating5;
            rating4ToolStripMenuItem.Checked = Model.Filters.ViewRating4;
            rating3ToolStripMenuItem.Checked = Model.Filters.ViewRating3;
            rating2ToolStripMenuItem.Checked = Model.Filters.ViewRating2;
            rating1ToolStripMenuItem.Checked = Model.Filters.ViewRating1;

            PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (Model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                {
                    GameInformation game = Model.GameData[listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')];
                    toolStripStatusLabel1.Text = game.Comments.Replace("\r\n", " ");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateShaderCachesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            iv1 = GetCurrentVersion();

            launcher.Model = Model;

            foreach (var game in Model.GameData)
            {
                if (!game.Value.SaveDir.StartsWith("??"))
                {
                    if (game.Value.GameSetting.EmulationState != GameSettings.EmulationStateType.NotSet)
                    {
                        if (game.Value.GameSetting.EmulationState != GameSettings.EmulationStateType.Loads)
                        {
                            if (game.Value.GameSetting.EmulationState != GameSettings.EmulationStateType.Unplayable)
                            {
                                if (game.Value.GameSetting.PreferedVersion == "Latest")
                                {
                                    launcher.CopyLargestShaderCacheToCemu(game.Value);
                                }
                            }
                        }
                    }
                }
            }

            System.Threading.ThreadPool.QueueUserWorkItem(ThreadProc2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateInfo"></param>
        void ThreadProc2(Object stateInfo)
        {
            foreach (var game in Model.GameData)
            {
                if (!game.Value.SaveDir.StartsWith("??"))
                {
                    if (IsPlayable(game.Value.GameSetting.EmulationState))
                    {
                        if (game.Value.GameSetting.PreferedVersion == "Latest")
                        {
                            try
                            {
                                UpdateShaderCache(game);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                    }
                }
            }
        }

        bool IsPlayable(GameSettings.EmulationStateType emulationState)
        {
            if (emulationState != GameSettings.EmulationStateType.NotSet)
            {
                if (emulationState != GameSettings.EmulationStateType.Loads)
                {
                    if (emulationState != GameSettings.EmulationStateType.Unplayable)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void UpdateShaderCache(KeyValuePair<string, GameInformation> game)
        {
            FileInfo transferableShader = new FileInfo(Path.Combine(iv1.Folder, "shaderCache", "transferable", game.Value.SaveDir + ".bin"));
            FileInfo precompiledShader = new FileInfo(Path.Combine(iv1.Folder, "shaderCache", "precompiled", game.Value.SaveDir + ".bin"));

            if (!File.Exists(precompiledShader.FullName))
            {
                if (File.Exists(transferableShader.FullName))
                {
                    if (transferableShader.Length > 1000000)
                    {
                        Model.CurrentId = game.Key;
                        launcher.LaunchCemu(this, Model, game.Value, true);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mergeShaderCachesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormShaderMerger merger = new FormShaderMerger())
            {
                merger.ShowDialog(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createSaveFileSnapshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (Model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                {
                    GameInformation game = Model.GameData[listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')];
                    launcher.CreateSaveSnapshot(Model, game);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void snapshotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (Model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                {
                    GameInformation game = Model.GameData[listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')];
                    using (FormShapShots ss = new FormShapShots(Model, game))
                    {
                        ss.ShowDialog(this);

                        if (ss.LaunchSnapShot != "")
                        {
                            MessageBox.Show(Resources.FormMainWindow_snapshotsToolStripMenuItem_Click_Launching_ + ss.LaunchSnapShot);
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
        private void openBudfordSaveFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Save location
            if (listView1.SelectedItems.Count == 1)
            {
                if (Model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                {
                    GameInformation game = Model.GameData[listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')];
                    DirectoryInfo src = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirBudford(Model, Model.CurrentUser, game, ""));
                    if (!Directory.Exists(src.FullName))
                    {
                        src.Create();
                    }
                    Process.Start(src.FullName);
                }
            }
        }

        private void downloadLatestGraphicPacksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Graphic Packs
            CemuFeatures.DownloadLatestGraphicsPack(this, Model);
        }



        internal void ProcessRunning()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(ProcessRunning));
            }
            else
            {
                if (launchGame != "")
                {
                    if (Model.Settings.StopHotkey == "None")
                    {
                        UnregisterHotKey(Handle, MyactionHotkeyId);
                        EnableControlsForGameExitted();
                        Close();
                    }
                }
            }
        }
        internal void ProcessExited()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(ProcessExited));
            }
            else
            {
                UnregisterHotKey(Handle, MyactionHotkeyId);
                EnableControlsForGameExitted();
                if (launchGame != "")
                {
                    Close();
                }
            }
        }

        private void EnableControlsForGameExitted()
        {
            toolStripButton1.Enabled = true;
            toolStripButton3.Enabled = true;
            playToolStripMenuItem.Enabled = true;
            loadToolStripMenuItem.Enabled = true;

            toolStripButton4.Enabled = false;
            toolStripButton8.Enabled = false;
            toolStripButton9.Enabled = false;

            stopToolStripMenuItem.Enabled = false;
            fullScreenToolStripMenuItem.Enabled = false;
            takeScreenshotToolStripMenuItem.Enabled = false;
        }

        private void EnableControlsForGameRunning()
        {
            toolStripButton1.Enabled = false;
            toolStripButton3.Enabled = false;
            playToolStripMenuItem.Enabled = false;
            loadToolStripMenuItem.Enabled = false;

            toolStripButton4.Enabled = true;
            toolStripButton8.Enabled = true;
            toolStripButton9.Enabled = true;


            stopToolStripMenuItem.Enabled = true;
            fullScreenToolStripMenuItem.Enabled = true;
            takeScreenshotToolStripMenuItem.Enabled = true;
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            if (launcher != null)
            {
                launcher.FullScreen();
            }
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            if (launcher != null)
            {
                launcher.ScreenShot();
            }
        }

        private void exportToLaunchboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HashSet<string> games = new HashSet<string>();
            foreach (ListViewItem item in listView1.Items)
            {
                GameInformation g = (GameInformation)item.Tag;
                games.Add(g.Name);
            }
            FormLaunchboxExporter flbe = new FormLaunchboxExporter(Model, games);
            flbe.ShowDialog(this);
        }

        internal void SetParent(Process runningProcess)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => { SetParent(runningProcess); }));
            }
            else
            {
                NativeMethods.SetParent(runningProcess.Handle, Handle);
            }
        }

        private void openCompatibilityWikiEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (Model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                {
                    GameInformation game = Model.GameData[listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')];
                    if (game.GameSetting.CompatibilityUrl != "")
                    {
                        Process.Start(game.GameSetting.CompatibilityUrl);
                    }
                }
            }
        }

        private void refreshGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshGameList();
        }

        private void importBudfordPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Configure open file dialog box 
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = Resources.FormMainWindow_importBudfordPluginToolStripMenuItem_Click_Budford_Plug_in_Files_____xml_;

                // Show open file dialog box 
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (!Directory.Exists(SpecialFolders.PlugInFolder(Model)))
                    {
                        Directory.CreateDirectory(SpecialFolders.PlugInFolder(Model));
                    }
                    File.Copy(dlg.FileName, Path.Combine(SpecialFolders.PlugInFolder(Model), Path.GetFileName(dlg.FileName)), true);
                    LoadPlugIns();
                }
            }
        }
    }
}
