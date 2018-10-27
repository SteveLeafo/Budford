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

namespace Budford.View
{
    public partial class FormMainWindow : Form
    {
        internal ToolStripMenuItem OfficiallyPerfectToolStripMenuItem { get { return officiallyPerfectToolStripMenuItem; } }
        internal ToolStripMenuItem OfficiallyPlayableToolStripMenuItem { get { return officiallyPlayableToolStripMenuItem; } }
        internal ToolStripMenuItem OfficiallyRunsToolStripMenuItem { get { return officiallyRunsToolStripMenuItem; } }
        internal ToolStripMenuItem OfficiallyLoadsToolStripMenuItem { get { return officiallyLoadsToolStripMenuItem; } }
        internal ToolStripMenuItem OfficiallyUnplayableToolStripMenuItem { get { return officiallyUnplayableToolStripMenuItem; } }
        internal ToolStripMenuItem OfficiallyNotSetToolStripMenuItem { get { return officiallyNotSetToolStripMenuItem; } }

        internal ToolStripMenuItem Rating1ToolStripMenuItem { get { return rating1ToolStripMenuItem; } }
        internal ToolStripMenuItem Rating2ToolStripMenuItem { get { return rating2ToolStripMenuItem; } }
        internal ToolStripMenuItem Rating3ToolStripMenuItem { get { return rating3ToolStripMenuItem; } }
        internal ToolStripMenuItem Rating4ToolStripMenuItem { get { return rating4ToolStripMenuItem; } }
        internal ToolStripMenuItem Rating5ToolStripMenuItem { get { return rating5ToolStripMenuItem; } }

        internal ToolStripMenuItem WiiUToolStripMenuItem { get { return wiiUToolStripMenuItem; } }
        internal ToolStripMenuItem EShopToolStripMenuItem { get { return eShopToolStripMenuItem; } }
        internal ToolStripMenuItem ChannelToolStripMenuItem { get { return channelToolStripMenuItem; } }
        internal ToolStripMenuItem VirtualConsoleToolStripMenuItem { get { return virtualConsoleToolStripMenuItem; } }

        internal ToolStripMenuItem PlatWiiUToolStripMenuItem { get { return platWiiUToolStripMenuItem; } }
        internal ToolStripMenuItem PlatHtml5ToolStripMenuItem { get { return platHtml5ToolStripMenuItem; } }
        internal ToolStripMenuItem PlatNintendo64ToolStripMenuItem { get { return nintendo64ToolStripMenuItem; } }
        internal ToolStripMenuItem PlatNintendoDsToolStripMenuItem { get { return nintendoDSToolStripMenuItem; } }
        internal ToolStripMenuItem PlatNESToolStripMenuItem { get { return nESToolStripMenuItem; } }
        internal ToolStripMenuItem PlatSNESToolStripMenuItem { get { return sNESToolStripMenuItem; } }
        
        internal ToolStripMenuItem UsaToolStripMenuItem { get { return usaToolStripMenuItem; } }
        internal ToolStripMenuItem EuropeToolStripMenuItem { get { return europeToolStripMenuItem; } }
        internal ToolStripMenuItem JapanToolStripMenuItem { get { return japanToolStripMenuItem; } }

        internal ToolStripMenuItem PerfectToolStripMenuItem { get { return perfectToolStripMenuItem; } }
        internal ToolStripMenuItem PlayableToolStripMenuItem { get { return playableToolStripMenuItem; } }
        internal ToolStripMenuItem RunsToolStripMenuItem { get { return runsToolStripMenuItem; } }
        internal ToolStripMenuItem LoadsToolStripMenuItem { get { return loadsToolStripMenuItem; } }
        internal ToolStripMenuItem UnplayableToolStripMenuItem { get { return unplayableToolStripMenuItem; } }
        internal ToolStripMenuItem NotSetToolStripMenuItem { get { return notSetToolStripMenuItem; } }

        internal ToolStripMenuItem DecafPerfectToolStripMenuItem { get { return perfectToolStripMenuItem1; } }
        internal ToolStripMenuItem DecafPlayableToolStripMenuItem { get { return playableToolStripMenuItem1; } }
        internal ToolStripMenuItem DecafRunsToolStripMenuItem { get { return runsToolStripMenuItem1; } }
        internal ToolStripMenuItem DecafLoadsToolStripMenuItem { get { return loadsToolStripMenuItem1; } }
        internal ToolStripMenuItem DecafUnplayableToolStripMenuItem { get { return unplayableToolStripMenuItem1; } }
        internal ToolStripMenuItem DecafNotSetToolStripMenuItem { get { return notSetToolStripMenuItem1; } }

        // ReSharper disable once InconsistentNaming
        public string launchGame = "";
        public bool LaunchFull = true;

        const int MyactionHotkeyId = 1;


        // All of our data...
        internal readonly Model.Model Model;

        readonly ViewPlugin viewPlugIn;
        readonly ViewUsers viewUsers;
        readonly ViewShaderCache viewShaderCache;
 
        // For launching the games.
        internal Launcher Launcher;

        // Used for column sorting when clicking on a header
        private readonly ListViewColumnSorter lvwColumnSorter;

        internal bool MinimizeToTray = false;

        /// <summary>
        /// 
        /// </summary>
        public FormMainWindow(Model.Model modelIn)
        {
            InitializeComponent();

            Model = modelIn;
            UsbNotification.RegisterUsbDeviceNotification(Handle);

            DiscordRichPresence.Initialize();

            Launcher = new Launcher(this);

            PerformWelcomeActions();

            CemuFeatures.PerformAutoOptionsOnStart(Model, this);
           
            Model.OldVersions.Clear();

            FolderScanner.SearchForInstalledGraphicPacks(Model);

            Persistence.LoadFromXml(Model.OldVersions);

            FolderScanner.AddGraphicsPacksToGames(Model);
            
            Text = Resources.fMainWindow_fMainWindow_CEMU_Game_DB______Current_User__ + Model.CurrentUser;

            viewUsers = new ViewUsers(Model, this, contextMenuStrip1, userToolStripMenuItem, pictureBox1);
            addNewToolStripMenuItem.Click += viewUsers.addNewToolStripMenuItem_Click;
            notifyIcon1.DoubleClick += notifyIcon1_DoubleClick;

            viewShaderCache = new ViewShaderCache(this, Model);

            SetupShowRegionMenuItems();

            SetVisibility();

            // Create an instance of a ListView column sorter and assign it 
            // to the ListView control.
            lvwColumnSorter = new ListViewColumnSorter {ColumnToSort = -1};
            listView1.ListViewItemSorter = lvwColumnSorter;

            EnableDoubleBuffering();

            RegisterEvents();

            viewPlugIn = new ViewPlugin(Model, this, plugInsToolStripMenuItem);
            viewPlugIn.LoadPlugIns();

            var verrsion = GetCurrentVersion();
            if (verrsion != null)
            {
                string path = Path.Combine(new string[]{verrsion.Folder, "mlc01", "sys", "title", "00050030"});
                string[] paths = new string[]{path};
                using (FormScanRomFolder scanner = new FormScanRomFolder(Model, Model.WiiUApps, new List<string>(paths)))
                {
                    scanner.ShowDialog(this);
                    foreach (var game in Model.WiiUApps)
                    {
                        wiiUAppsToolStripMenuItem.Visible = true;

                        ToolStripMenuItem menuItem = new ToolStripMenuItem
                        {
                            Text = game.Value.Name,
                            Tag = game.Value
                        };
                        menuItem.Click += menuItem_Click;
                        wiiUAppsToolStripMenuItem.DropDownItems.Insert(0, menuItem);
                    }                 
                }
            }
        }

        void menuItem_Click(object sender, EventArgs e)
        {
             var toolStripMenuItem = sender as ToolStripMenuItem;
             if (toolStripMenuItem != null)
             {
                 EnableControlsForGameRunning();
                 GameInformation game = toolStripMenuItem.Tag as GameInformation;
                 game.Exists = true;
                 game.GameSetting.Online = 1;
                 Text = Resources.fMainWindow_fMainWindow_CEMU_Game_DB______Current_User__ + Model.CurrentUser + "\t        Running: " + game.Name;

                 RegisterStopHotKey(Model);

                 Launcher.LaunchCemu(this, Model, game, false, false, ModifierKeys == Keys.Shift);

                 if (Model.Settings.CloseCemuOnExit)
                 {
                     if (File.Exists("BudfordsAssassin.exe"))
                     {
                         Process.Start("BudfordsAssassin.exe");
                     }
                 }
             }
        }

        void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            notifyIcon1.Visible = false;
            Show();
        }

        /// <summary>
        /// 
        /// </summary>
        private void PerformWelcomeActions()
        {
            if (!Directory.Exists(Model.Settings.DefaultInstallFolder))
            {
                using (FormFirstTimeWindow fftw = new FormFirstTimeWindow())
                {
                    switch (fftw.ShowDialog(this))
                    {
                        case DialogResult.OK:
                            FileManager.DownloadCemu(this, Model);
                            break;
                        case DialogResult.Yes:
                            CemuFeatures.SetCemuFolder(Model);
                            break;
                    }
                }
            }
            else
            {
                FileManager.SearchForInstalledVersions(Model);
                FolderScanner.GetGameInformation(null, "", "");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void RegisterEvents()
        {
            listView1.KeyDown += listView1_KeyDown;
            Resize += FormMainWindow_Resize;

            listView1.OwnerDraw = false;
            listView1.DrawColumnHeader += ListView1_DrawColumnHeader;
            listView1.DrawSubItem += ListView1_DrawSubItem;
            listView1.ColumnClick += ListView1_ColumnClick;

            ListView1_ColumnClick(this, new ColumnClickEventArgs(Model.Settings.CurrentSortColumn));
            if (Model.Settings.CurrentSortDirection == 1)
            {
                ListView1_ColumnClick(this, new ColumnClickEventArgs(Model.Settings.CurrentSortColumn));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void EnableDoubleBuffering()
        {
            listView1.DoubleBuffered(true);
            pictureBox1.DoubleBuffered(true);
            DoubleBuffered = true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetVisibility()
        {
            showStatusToolStripMenuItem1.Checked = Model.Settings.ShowStausBar;
            statusStrip1.Visible = Model.Settings.ShowStausBar;
            toolStrip1.Visible = Model.Settings.ShowToolBar;
            pictureBox1.Visible = Model.Settings.ShowToolBar;
            showToolbarToolStripMenuItem.Checked = Model.Settings.ShowToolBar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FormMainWindow_Resize(object sender, EventArgs e)
        {
            if (MinimizeToTray)
            {
                if (FormWindowState.Minimized == WindowState)
                {
                    notifyIcon1.Visible = true;
                    Hide();
                }

                else if (FormWindowState.Normal == WindowState)
                {
                    notifyIcon1.Visible = false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            if (launchGame != "")
            {
                WindowState = FormWindowState.Minimized;
                Visible = false;
                //Hide();
                bool launched = false;
                foreach (var game in Model.GameData)
                {
                    if (game.Value.LaunchFile.ToLower() == launchGame.ToLower())
                    {
                        RegisterStopHotKey(Model);
                        game.Value.Exists = true;
                        Launcher.LaunchCemu(this, Model, game.Value, false, false, false, LaunchFull);
                        if (Model.Settings.CloseCemuOnExit)
                        {
                            if (File.Exists("BudfordsAssassin.exe"))
                            {
                                Process.Start("BudfordsAssassin.exe");
                            }
                        }
                        launched = true;
                        break;
                    }
                }
                if (!launched)
                {
                    // Game wasn't in library, so just launch with current settings.
                    Launcher.LaunchRpx(Model, launchGame, LaunchFull);
                    if (Model.Settings.CloseCemuOnExit)
                    {
                        if (File.Exists("BudfordsAssassin.exe"))
                        {
                            Process.Start("BudfordsAssassin.exe");
                        }
                    }
                }
            }

            base.OnLoad(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            KeysConverter kc = new KeysConverter();
            string keyChar = kc.ConvertToString(e.KeyData);
            if (keyChar == "Enter")
            {
                LaunchGame();
                e.Handled = true;
            }
            else
            {
                //if (keyChar == "U" && listView1.SelectedItems.Count == 1)
                //{
                //    GameInformation game = Model.GameData[listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')];
                //    game.GameSetting.DecafEmulationState = GameSettings.EmulationStateType.Unplayable;
                //    RefreshList(game);
                //}
                //else
                {
                    e.Handled = FindMyString(keyChar);
                }
            }
        }

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        /// <summary>
        /// 
        /// </summary>
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
                        Text = Resources.fMainWindow_fMainWindow_CEMU_Game_DB______Current_User__ + Model.CurrentUser + "\t        Running: " + game.Name;
                        Model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                        RegisterStopHotKey(Model);

                        Launcher.LaunchCemu(this, Model, game, false, false, ModifierKeys == Keys.Shift);
                        if (game.GameSetting.EnableLogging)
                        {
                            // We have enabled logging as a work around, this timer will turn in off once the game starts
                            timer = new System.Windows.Forms.Timer();
                            timer.Tick += timer_Tick;
                            timer.Interval = 20000;
                            timer.Start();
                        }
                        if (Model.Settings.CloseCemuOnExit)
                        {
                            if (File.Exists("BudfordsAssassin.exe"))
                            {
                                Process.Start("BudfordsAssassin.exe");
                            }
                        }

                    }
                }
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Launcher.cemuController.ToggleLogging();
            timer.Stop();
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
                lvwColumnSorter.ReverseCurrentSort();
            }
            else
            {
                lvwColumnSorter.SetSortType(e);
            }

            Model.Settings.CurrentSortColumn = e.Column;
            Model.Settings.CurrentSortDirection = lvwColumnSorter.OrderOfSort == SortOrder.Ascending ? 0 : 1;

            // Perform the sort with these new sort options.
            listView1.Sort();

            MakeBackgroundStripy();
        }
           
        /// <summary>
        /// 
        /// </summary>
        private void MakeBackgroundStripy()
        {
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
                // Hot key was pressed
                Launcher.KillCurrentProcess();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if ((e.Item.SubItems[0] == e.SubItem))
            {
                e.DrawDefault = true;
            }
            else
            {
                e.DrawDefault = false;
                if ((e.ItemState & ListViewItemStates.Selected) == ListViewItemStates.Selected)
                {
                    Rectangle r = new Rectangle(e.SubItem.Bounds.Left, e.SubItem.Bounds.Top, e.SubItem.Bounds.Width, e.SubItem.Bounds.Height);
                    e.Graphics.FillRectangle(SystemBrushes.Highlight, r);
                    e.SubItem.ForeColor = SystemColors.HighlightText;
                }
                else
                {
                    if (e.Item.BackColor != Color.White)
                    {
                        Rectangle r = new Rectangle(e.SubItem.Bounds.Left, e.SubItem.Bounds.Top, e.SubItem.Bounds.Width, e.SubItem.Bounds.Height);
                        e.Graphics.FillRectangle(new SolidBrush(e.Item.BackColor), r);
                    }
                }

                if ((e.Item.SubItems[9] == e.SubItem) || (e.Item.SubItems[8] == e.SubItem))
                {
                    int x = e.SubItem.Bounds.Location.X + (e.SubItem.Bounds.Width / 2) - (imageList1.Images[0].Width / 2);
                    int y = e.SubItem.Bounds.Location.Y + (e.SubItem.Bounds.Height / 2) - (imageList1.Images[0].Height / 2);

                    int imageIndex = GameSettings.GetStatusImageIndex(e.SubItem.Text.Trim());
                    e.Graphics.DrawImage(imageList2.Images[imageIndex], x, y);
                }
                else
                {
                    e.DrawText(TextFormatFlags.VerticalCenter);
                }
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
            usaToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            europeToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            japanToolStripMenuItem.Click += UsaToolStripMenuItem_Click;

            wiiUToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            eShopToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            channelToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            virtualConsoleToolStripMenuItem.Click += UsaToolStripMenuItem_Click;

            platHtml5ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            platWiiUToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            nintendo64ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            nintendoDSToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            sNESToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            nESToolStripMenuItem.Click += UsaToolStripMenuItem_Click;

            rating5ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            rating4ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            rating3ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            rating2ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            rating1ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;

            perfectToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            playableToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            runsToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            loadsToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            unplayableToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            notSetToolStripMenuItem.Click += UsaToolStripMenuItem_Click;

            perfectToolStripMenuItem1.Click += UsaToolStripMenuItem_Click;
            playableToolStripMenuItem1.Click += UsaToolStripMenuItem_Click;
            runsToolStripMenuItem1.Click += UsaToolStripMenuItem_Click;
            loadsToolStripMenuItem1.Click += UsaToolStripMenuItem_Click;
            unplayableToolStripMenuItem1.Click += UsaToolStripMenuItem_Click;
            notSetToolStripMenuItem1.Click += UsaToolStripMenuItem_Click;

            officiallyPerfectToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyPlayableToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyRunsToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyLoadsToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyUnplayableToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyNotSetToolStripMenuItem.Click += UsaToolStripMenuItem_Click;

            ViewFilters.UpdateMenuItemChecks(Model, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UsaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
            ViewFilters.UpdateFiltersItems(Model, this);
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
        }

        /// <summary>
        /// 
        /// </summary>
        internal void PopulateListView()
        {
            listView1.BeginUpdate();
            try
            {
                listView1.Items.Clear();
                FolderScanner.CheckGames(Model);

                foreach (var game in Model.GameData.OrderByDescending(gd => gd.Value.Name))
                {
                    if (GameFilter.FilterCheckedOut(Model, game.Value))
                    {
                        ListViewItem lvi = new ListViewItem();
                        PopulateSubItems(game, lvi);

                        lvi.ImageIndex = GameSettings.GetRegionImageIndex(game);
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
                MakeBackgroundStripy();

                ResizeColumns();

                toolStripStatusLabel3.Text = Resources.FormMainWindow_PopulateListView_Currently_showing_ + listView1.Items.Count + (listView1.Items.Count == 1 ? " Game" : " Games");
            }
            finally
            {
                listView1.EndUpdate();
            }
            ResizeColumnHeaders();
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
                // No code
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
            lvi.SubItems.Add(game.Value.Publisher);
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
            lvi.SubItems.Add(game.Value.GameSetting.DecafEmulationState + "        ");
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
            listView1.Columns.Add("Decaf Status", 100);
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
            Logger.Log("Close 2");
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
                    lvi.SubItems[11].Text = savedDir;
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
                    listView1.SelectedItems[0].SubItems[7].Text = game.GameSetting.PreferedVersion;
                    listView1.SelectedItems[0].SubItems[8].Text = game.GameSetting.OfficialEmulationState.ToString();
                    listView1.SelectedItems[0].SubItems[9].Text = game.GameSetting.EmulationState.ToString();
                    listView1.SelectedItems[0].SubItems[10].Text = game.GameSetting.DecafEmulationState.ToString();
                    listView1.SelectedItems[0].SubItems[13].Text = game.LastPlayed != DateTime.MinValue ? game.LastPlayed.ToShortDateString() + " " : "                    ";
                    listView1.SelectedItems[0].SubItems[14].Text = game.PlayCount != 0 ? game.PlayCount + "                 " : "                 ";
                    listView1.SelectedItems[0].SubItems[16].Text = game.Rating + Resources.FormMainWindow_RefreshList__________________;
                    listView1.SelectedItems[0].SubItems[17].Text = game.Comments + Resources.FormMainWindow_RefreshList__________________;
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
            if (Launcher.Open(Model))
            {
                RegisterStopHotKey(Model);
                EnableControlsForGameRunning();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void RefreshGameList()
        {
            using (FormScanRomFolder scanner = new FormScanRomFolder(Model, Model.GameData, Model.Settings.RomFolders))
            {
                scanner.ShowDialog(this);
            }

            PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            Launcher.KillCurrentProcess();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewUsers.EditCurrentUser();
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
                Model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                var game = Model.GameData[Model.CurrentId];
                try
                {
                    using (FormEditGameSettings editGameSettings = new FormEditGameSettings(game, Model.Settings.InstalledVersions))
                    {
                        editGameSettings.ShowDialog(this);
                        RefreshList(game);
                    }
                }
                catch (Exception)
                {
                    // ignored
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
                FileManager.OpenSaveFileLocation(Model, GetCurrentVersion());
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
                    if (File.Exists(Model.GameData[Model.CurrentId].LaunchFile))
                    {
                        Process.Start((Path.Combine(Path.GetDirectoryName(Model.GameData[Model.CurrentId].LaunchFile), "..", "..")));
                    }
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
                FileManager.OpenShaderCacheFolder(Model, GetCurrentVersion());
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fMainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Persistence.Save(Model, Persistence.GetModelFileName());
            DiscordRichPresence.ShutDown();
            Logger.Log("Budford is closing");
            //if (Model.Settings.CloseCemuOnExit)
            //{
            //    Logger.Log("Budford is closing - trying to bring Cemu down with it"); 
            //    Launcher.KillCurrentProcess();
            //}
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
        private void importShaderCacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewShaderCache.importShaderCacheToolStripMenuItem_Click();
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            LaunchConfigurationForm(4);
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
        /// <returns></returns>
        internal InstalledVersion GetCurrentVersion()
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
            Launcher.LaunchCemu(this, Model, null, false, true);
            if (Model.Settings.CloseCemuOnExit)
            {
                if (File.Exists("BudfordsAssassin.exe"))
                {
                    Process.Start("BudfordsAssassin.exe");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        public void RegisterStopHotKey(Model.Model model)
        {
            if (model.Settings.StopHotkey != "None")
            {
                Keys key = NativeMethods.GetHotKey(model.Settings.StopHotkey);
                NativeMethods.RegisterHotKey(Handle, MyactionHotkeyId, (int)NativeMethods.KeyModifiers.None, key.GetHashCode());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadCompatabilityStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CemuFeatures.DownloadCompatibilityStatus(this, Model);           
            PopulateListView();
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
            using (FormEditInstalledVersions installedVersions = new FormEditInstalledVersions(this, Model, Launcher))
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
            FileManager.DownloadCemu(this, Model);
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
        private void allToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ViewFilters.AllStatus(Model, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ViewFilters.NoStatus(Model, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void allToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            ViewFilters.OfficiallyAll(Model, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void noneToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            ViewFilters.OfficiallyNone(Model, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void allToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ViewFilters.AllRegions(Model, this);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void noneToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ViewFilters.NoRegions(Model, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void allToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            ViewFilters.AllTypes(Model, this);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void noneToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            ViewFilters.NoTypes(Model, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void allToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            ViewFilters.AllRatings(Model, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void noneToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            ViewFilters.NoRatings(Model, this);
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
            viewShaderCache.updateShaderCachesToolStripMenuItem_Click();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mergeShaderCachesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewShaderCache.mergeShaderCachesToolStripMenuItem_Click();
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
                    Launcher.CreateSaveSnapshot(Model, game);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadLatestGraphicPacksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Graphic Packs
            CemuFeatures.DownloadLatestGraphicsPack(this, Model);
        }

        /// <summary>
        /// 
        /// </summary>
        internal void ProcessRunning()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(ProcessRunning));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void ProcessExited()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(ProcessExited));
            }
            else
            {
                NativeMethods.UnregisterHotKey(Handle, MyactionHotkeyId);
                EnableControlsForGameExitted();
                Logger.Log("Cemu process has exited");
                if (launchGame != "")
                {
                    Logger.Log("Close 1");
                    Close();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
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

            Text = Resources.fMainWindow_fMainWindow_CEMU_Game_DB______Current_User__ + Model.CurrentUser;
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            if (Launcher != null)
            {
                Launcher.FullScreen();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            if (Launcher != null)
            {
                Launcher.ScreenShot();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openCompatibilityWikiEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                string gameId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                CemuFeatures.OpenCompatibilityEntry(gameId, Model, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshGameList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importBudfordPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewPlugIn.ImportPlugIn();
        }

        private void allToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            ViewFilters.AllPlatforms(Model, this);
        }

        private void noneToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            ViewFilters.NoPlatforms(Model, this);
        }

        private void compareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Model.Model temp = Persistence.Load(@"C:\Users\steve\Documents\New folder (2)\Model-1.12.0.xml");
            for (int i = 0; i < temp.GameData2.Count; ++i)
            {
                if (temp.GameData2[i].GameSetting.EmulationState != Model.GameData2[i].GameSetting.EmulationState)
                {
                    string s = temp.GameData2[i].Name;
                }
            }
        }

        private void cemuWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://cemu.info");
        }

        private void budfordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://compat.cemu.info/wiki/Main_Page");
        }

        private void githubRepositoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/SteveLeafo/Budford");
        }

        private void FormMainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Logger.Log("Closed");
        }

        private void allToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            ViewFilters.AllDecafStatus(Model, this);
        }

        private void noneToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            ViewFilters.NoDecafStatus(Model, this);
        }
    }
}
