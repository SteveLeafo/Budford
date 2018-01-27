﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Budford.Control;
using Budford.Model;
using Budford.View;
using System.Diagnostics;
using Budford.Properties;
using Budford.Utilities;
using Budford.Tools;

namespace Budford
{
    public partial class FormMainWindow : Form
    {
        // All of our data...
        internal readonly Model.Model model;

        // For downloading and extracing.
        readonly Unpacker unpacker;

        // For launching the games.
        readonly Launcher launcher;

        // Managing files between revisions
        readonly FileManager fileManager;

        // Used for column sorting when clicking on a header
        private ListViewColumnSorter lvwColumnSorter;

        static InstalledVersion iv1;

        bool comments = false;

        /// <summary>
        /// 
        /// </summary>
        public FormMainWindow()
        {
            InitializeComponent();

            UsbNotification.RegisterUsbDeviceNotification(this.Handle);

            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Budford"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Budford");
            }
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Budford\\Model.xml"))
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Model.xml"))
                {
                    File.Copy(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Model.xml", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Budford\\Model.xml", false);
                }
            }
            model = Persistence.Load(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Budford\\Model.xml");
           

            unpacker = new Unpacker(this);
            launcher = new Launcher(this);
            fileManager = new FileManager(model);

            foreach (var folder in model.Settings.RomFolders)
            {
                using (FormScanRomFolder scanner = new FormScanRomFolder(folder, model.GameData))
                {
                    scanner.ShowDialog(this);
                }
            }

            model.OldVersions.Clear();

            if (!Directory.Exists("graphicsPacks"))
            {
                if (File.Exists("graphicsPacks.zip"))
                {
                    unpacker.Unpack("graphicsPacks.zip", "graphicsPacks");
                }
            }

            FolderScanner.FindGraphicsPacks(new DirectoryInfo("graphicsPacks\\graphicPacks_2-" + model.Settings.GraphicsPackRevision), model.GraphicsPacks);

            Persistence.LoadFromXml(model.OldVersions);

            fileManager.InitialiseFolderStructure(model);

            FolderScanner.AddGraphicsPacksToGames(model);

            NativeMethods.SurpressOsErrors();

            if (model.Users.Count == 0)
            {
                model.Users.Add(new User() { Name = "Default", Image = "default.png" });
                model.CurrentUser = "Default";
            }

            var firstOrDefault = model.Users.FirstOrDefault(u => u.Name == model.CurrentUser);
            if (firstOrDefault != null && File.Exists("Users\\" + firstOrDefault.Image))
            {
                var orDefault = model.Users.FirstOrDefault(u => u.Name == model.CurrentUser);
                if (orDefault != null)
                {
                    using (FileStream stream = new FileStream("Users\\" + orDefault.Image, FileMode.Open, FileAccess.Read))
                    {
                        pictureBox1.Image = Image.FromStream(stream);
                    }
                }
            }
            Text = Resources.fMainWindow_fMainWindow_CEMU_Game_DB______Current_User__ + model.CurrentUser;

            AddUserMenuItems();
            SetupShowRegionMenuItems();

            showStatusToolStripMenuItem1.Checked = model.Settings.ShowStausBar;
            statusStrip1.Visible = model.Settings.ShowStausBar;
            toolStrip1.Visible = model.Settings.ShowToolBar;
            pictureBox1.Visible = model.Settings.ShowToolBar;
            showToolbarToolStripMenuItem.Checked = model.Settings.ShowToolBar;
            listView1.KeyDown += listView1_KeyDown;
            //listView1.MouseDown += listView1_MouseDown;

            if (model.Settings.CurrentView == "Detailed")
            {
                detailsToolStripMenuItem_Click(null, null);
            }
            else
            {
                tToolStripMenuItem_Click(null, null);
            }

            // Create an instance of a ListView column sorter and assign it 
            // to the ListView control.
            lvwColumnSorter = new ListViewColumnSorter();
            lvwColumnSorter.ColumnToSort = -1;
            this.listView1.ListViewItemSorter = lvwColumnSorter;

            listView1.DoubleBuffered(true);
            pictureBox1.DoubleBuffered(true);
            this.DoubleBuffered = true;
            listView1.DrawColumnHeader += ListView1_DrawColumnHeader;
            listView1.DrawSubItem += ListView1_DrawSubItem;
            listView1.ColumnClick += ListView1_ColumnClick;

            ListView1_ColumnClick(this, new ColumnClickEventArgs(model.Settings.CurrentSortColumn));
            if (model.Settings.CurrentSortDirection == 1)
            {
                ListView1_ColumnClick(this, new ColumnClickEventArgs(model.Settings.CurrentSortColumn));
            }
        }

        void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip2.Items.Clear();
                var mi = new ToolStripMenuItem("C:\\temp");
                // handle menu item click event here [as required]
                mi.Click += mi_Click;
                contextMenuStrip2.Items.Add(mi);
                //var bounds = dataGridView1.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                if (sender != null)
                {
                    contextMenuStrip2.Show(sender as ListView, new Point(e.X, e.Y));
                }
            }
        }

        void mi_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hello");
        }

        void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            KeysConverter kc = new KeysConverter();
            string keyChar = kc.ConvertToString(e.KeyData);
            if (keyChar == "Enter")
            {
                if (listView1.SelectedItems.Count == 1)
                {
                    if (model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                    {
                        GameInformation game = model.GameData[listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')];
                        model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                        launcher.LaunchCemu(model, game, false, false, System.Windows.Forms.Control.ModifierKeys == Keys.Shift);
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

            model.Settings.CurrentSortColumn = e.Column;
            model.Settings.CurrentSortDirection = lvwColumnSorter.OrderOfSort == SortOrder.Ascending ? 0 : 1;

            // Perform the sort with these new sort options.
            this.listView1.Sort();

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
            usaToolStripMenuItem.Checked = model.Filters.ViewRegionUsa;
            usaToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            europeToolStripMenuItem.Checked = model.Filters.ViewRegionEur;
            europeToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            japanToolStripMenuItem .Checked = model.Filters.ViewRegionJap;
            japanToolStripMenuItem.Click += UsaToolStripMenuItem_Click;

            wiiUToolStripMenuItem.Checked = model.Filters.ViewTypeWiiU;
            wiiUToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            eShopToolStripMenuItem.Checked = model.Filters.ViewTypeEshop;
            eShopToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            channelToolStripMenuItem.Checked = model.Filters.ViewTypeChannel;
            channelToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            virtualConsoleToolStripMenuItem.Checked = model.Filters.ViewTypeVc;
            virtualConsoleToolStripMenuItem.Click += UsaToolStripMenuItem_Click;

            rating5ToolStripMenuItem.Checked = model.Filters.ViewRating5;
            rating5ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            rating4ToolStripMenuItem.Checked = model.Filters.ViewRating4;
            rating4ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            rating3ToolStripMenuItem.Checked = model.Filters.ViewRating3;
            rating3ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            rating2ToolStripMenuItem.Checked = model.Filters.ViewRating2;
            rating2ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            rating1ToolStripMenuItem.Checked = model.Filters.ViewRating1;
            rating1ToolStripMenuItem.Click += UsaToolStripMenuItem_Click;

            perfectToolStripMenuItem.Checked = model.Filters.ViewStatusPerfect;
            perfectToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            playableToolStripMenuItem.Checked = model.Filters.ViewStatusPlayable;
            playableToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            runsToolStripMenuItem.Checked = model.Filters.ViewStatusRuns;
            runsToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            loadsToolStripMenuItem.Checked = model.Filters.ViewStatusLoads;
            loadsToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            unplayableToolStripMenuItem.Checked = model.Filters.ViewStatusUnplayable;
            unplayableToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            notSetToolStripMenuItem.Checked = model.Filters.ViewStatusNotSet;
            notSetToolStripMenuItem.Click += UsaToolStripMenuItem_Click;

            officiallyPerfectToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusPerfect;
            officiallyPerfectToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyPlayableToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusPlayable;
            officiallyPlayableToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyRunsToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusRuns;
            officiallyRunsToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyLoadsToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusLoads;
            officiallyLoadsToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyUnplayableToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusUnplayable;
            officiallyUnplayableToolStripMenuItem.Click += UsaToolStripMenuItem_Click;
            officiallyNotSetToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusNotSet;
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

            model.Filters.ViewRegionUsa = usaToolStripMenuItem.Checked;
            model.Filters.ViewRegionEur = europeToolStripMenuItem.Checked;
            model.Filters.ViewRegionJap = japanToolStripMenuItem.Checked;

            model.Filters.ViewTypeWiiU = wiiUToolStripMenuItem.Checked;
            model.Filters.ViewTypeEshop = eShopToolStripMenuItem.Checked;
            model.Filters.ViewTypeChannel = channelToolStripMenuItem.Checked;
            model.Filters.ViewTypeVc = virtualConsoleToolStripMenuItem.Checked;

            model.Filters.ViewRating5 = rating5ToolStripMenuItem.Checked;
            model.Filters.ViewRating4 = rating4ToolStripMenuItem.Checked;
            model.Filters.ViewRating3 = rating3ToolStripMenuItem.Checked;
            model.Filters.ViewRating2 = rating2ToolStripMenuItem.Checked;
            model.Filters.ViewRating1 = rating1ToolStripMenuItem.Checked;

            model.Filters.ViewStatusPerfect = perfectToolStripMenuItem.Checked;
            model.Filters.ViewStatusPlayable = playableToolStripMenuItem.Checked;
            model.Filters.ViewStatusRuns = runsToolStripMenuItem.Checked;
            model.Filters.ViewStatusLoads = loadsToolStripMenuItem.Checked;
            model.Filters.ViewStatusUnplayable = unplayableToolStripMenuItem.Checked;
            model.Filters.ViewStatusNotSet = notSetToolStripMenuItem.Checked;

            model.Filters.ViewOfficialStatusPerfect = officiallyPerfectToolStripMenuItem.Checked;
            model.Filters.ViewOfficialStatusPlayable = officiallyPlayableToolStripMenuItem.Checked;
            model.Filters.ViewOfficialStatusRuns = officiallyRunsToolStripMenuItem.Checked;
            model.Filters.ViewOfficialStatusLoads = officiallyLoadsToolStripMenuItem.Checked;
            model.Filters.ViewOfficialStatusUnplayable = officiallyUnplayableToolStripMenuItem.Checked;
            model.Filters.ViewOfficialStatusNotSet = officiallyNotSetToolStripMenuItem.Checked;

            PopulateListView();
        }           

        /// <summary>
        /// 
        /// </summary>
        void AddUserMenuItems()
        {
            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
            foreach (var user in model.Users)
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
                if (user.Name == model.CurrentUser)
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
                    if (user.Name != model.CurrentUser)
                    {
                        if (File.Exists("Users\\" + user.Image))
                        {
                            using (FileStream stream = new FileStream("Users\\" + user.Image, FileMode.Open, FileAccess.Read))
                            {
                                pictureBox1.Image = Image.FromStream(stream);
                            }

                        }
                        Text = Resources.fMainWindow_fMainWindow_CEMU_Game_DB______Current_User__ + user.Name;
                        fileManager.SaveUserSaves(model.Users.FirstOrDefault(u => u.Name == model.CurrentUser));
                        fileManager.LoadUserSaves(user);
                        model.CurrentUser = user.Name;
                    }
                }
                UpdateMenuStrip(user);
                UpdateContextMenuStrip(user);
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
                if (menu.Text == model.CurrentUser)
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
                if (menu.Text == model.CurrentUser)
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
                FolderScanner.CheckGames(model);

                foreach (var game in model.GameData.OrderByDescending(gd => gd.Value.Name))
                {
                    if (FilterCheckedOut(game.Value))
                    {
                        ListViewItem lvi = new ListViewItem();
                        PopulateSubItems(game, lvi);

                        lvi.ImageIndex = GetRegionImageIndex(game);

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

                toolStripStatusLabel3.Text = "Currently showing " + listView1.Items.Count + (listView1.Items.Count == 1 ? " Game" : " Games");
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
            listView1.Columns[0].Width = 36;
            if (model.Settings.AutoSizeColumns)
            {
                for (int c = 4; c < listView1.Columns.Count; ++c)
                {
                    listView1.AutoResizeColumn(c, ColumnHeaderAutoResizeStyle.ColumnContent);
                    listView1.AutoResizeColumn(c, ColumnHeaderAutoResizeStyle.HeaderSize);
                }
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
            lvi.SubItems.Add(game.Value.GameSetting.OfficialEmulationState + "        ");
            lvi.SubItems.Add(game.Value.GameSetting.EmulationState + "        ");
            lvi.SubItems.Add(game.Value.SaveDir.Trim());
            lvi.SubItems.Add(game.Value.Type.Trim() + " ");
            lvi.SubItems.Add(game.Value.LastPlayed != DateTime.MinValue ? game.Value.LastPlayed.ToShortDateString() + " " : "                    ");
            lvi.SubItems.Add(game.Value.PlayCount != 0 ? game.Value.PlayCount + "                 " : "                 ");
            lvi.SubItems.Add(game.Value.GraphicsPacksCount != 0 ? game.Value.GraphicsPacksCount + "                    " : "                    ");
            lvi.SubItems.Add(game.Value.Rating + "                 ");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private bool FilterCheckedOut(GameInformation game)
        {
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
                    if (!model.Filters.ViewStatusNotSet) return false;
                    break;
                case GameSettings.EmulationStateType.Perfect:
                    if (!model.Filters.ViewStatusPerfect) return false;
                    break;
                case GameSettings.EmulationStateType.Playable:
                    if (!model.Filters.ViewStatusPlayable) return false;
                    break;
                case GameSettings.EmulationStateType.Runs:
                    if (!model.Filters.ViewStatusRuns) return false;
                    break;
                case GameSettings.EmulationStateType.Loads:
                    if (!model.Filters.ViewStatusLoads) return false;
                    break;
                case GameSettings.EmulationStateType.Unplayable:
                    if (!model.Filters.ViewStatusUnplayable) return false;
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
                    if (!model.Filters.ViewOfficialStatusNotSet) return false;
                    break;
                case GameSettings.EmulationStateType.Perfect:
                    if (!model.Filters.ViewOfficialStatusPerfect) return false;
                    break;
                case GameSettings.EmulationStateType.Playable:
                    if (!model.Filters.ViewOfficialStatusPlayable) return false;
                    break;
                case GameSettings.EmulationStateType.Runs:
                    if (!model.Filters.ViewOfficialStatusRuns) return false;
                    break;
                case GameSettings.EmulationStateType.Loads:
                    if (!model.Filters.ViewOfficialStatusLoads) return false;
                    break;
                case GameSettings.EmulationStateType.Unplayable:
                    if (!model.Filters.ViewOfficialStatusUnplayable) return false;
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
                    if (!model.Filters.ViewRegionUsa) return false;
                    break;
                case "EUR":
                    if (!model.Filters.ViewRegionEur) return false;
                    break;
                case "JAP":
                    if (!model.Filters.ViewRegionJap) return false;
                    break;
                case "ALL":
                    if (!model.Filters.ViewRegionAll) return false;
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
                    if (!model.Filters.ViewTypeWiiU) return false;
                    break;
                case "eShop":
                    if (!model.Filters.ViewTypeEshop) return false;
                    break;
                case "Channel":
                    if (!model.Filters.ViewTypeChannel) return false;
                    break;
                default:
                    if (!model.Filters.ViewTypeVc) return false;
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
                    if (!model.Filters.ViewRating5) return false;
                    break;
                case 4:
                    if (!model.Filters.ViewRating4) return false;
                    break;
                case 3:
                    if (!model.Filters.ViewRating3) return false;
                    break;
                case 2:
                    if (!model.Filters.ViewRating2) return false;
                    break;
                case 1:
                    if (!model.Filters.ViewRating1) return false;
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
            listView1.Columns.Add("", 50);
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
            if (model != null)
            {
                if (model.GameData.Count > 0)
                {
                    if (model.Settings.AutoSizeColumns)
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
            if (listView1.SelectedItems.Count == 1)
            {
                if (model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                {
                    GameInformation game = model.GameData[listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')];
                    model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                    launcher.LaunchCemu(model, game, false, false, System.Windows.Forms.Control.ModifierKeys == Keys.Shift);
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
                ListViewItem lvi =  listView1.FindItemWithText(model.CurrentId, true, 0);
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
                    listView1.SelectedItems[0].SubItems[15].Text = game.Rating + "                 ";
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
            launcher.Open(model);
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
            using (FormEditConfiguration configurationForm = new FormEditConfiguration(model, tabPageIndex))
            {
                List<string> oldRomFolder = new List<string>(model.Settings.RomFolders.ToArray());
                configurationForm.ShowDialog(this);
                if (oldRomFolder.Count == model.Settings.RomFolders.Count)
                {
                    for (int i = 0; i < oldRomFolder.Count; ++i)
                    {
                        if (oldRomFolder[i] != model.Settings.RomFolders[i])
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
            foreach (var folder in model.Settings.RomFolders)
            {
                using (FormScanRomFolder scanner = new FormScanRomFolder(folder, model.GameData))
                {
                    scanner.ShowDialog(this);
                }
            }

            Persistence.SetSaveDirs(model);
            Persistence.SetGameTypes(model);
            FolderScanner.AddGraphicsPacksToGames(model);

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
            User user = model.Users.FirstOrDefault(u => u.Name == model.CurrentUser);
            using (FormEditUser editUser = new FormEditUser(user))
            {
                editUser.ShowDialog(this);
            }
            if (user != null && File.Exists("Users\\" + user.Image))
            {
                using (FileStream stream = new FileStream("Users\\" + user.Image, FileMode.Open, FileAccess.Read))
                {
                    pictureBox1.Image = Image.FromStream(stream);
                }
            }
            if (user != null && model.CurrentUser != user.Name)
            {
                if (Directory.Exists("Users\\" + model.CurrentUser))
                {
                    try
                    {
                        Directory.Move("Users\\" + model.CurrentUser, "Users\\" + user.Name);
                    }
                    catch (Exception ex)
                    {
                        model.Errors.Add(ex.Message);
                    }
                }
                UpdateMenuStrip(user);
                UpdateContextMenuStrip(user);
              
                model.CurrentUser = user.Name;
                Text = Resources.fMainWindow_fMainWindow_CEMU_Game_DB______Current_User__ + model.CurrentUser;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            User newUser = new User() { Name = "User " + (model.Users.Count + 1), Image = "default.png" };
            using (FormEditUser editUser = new FormEditUser(newUser))
            {
                if (editUser.ShowDialog(this) == DialogResult.OK)
                {
                    model.Users.Add(newUser);
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
            fileManager.InitialiseFolderStructure(model);
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
                    model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                    var game = model.GameData[model.CurrentId];
                    try
                    {
                        using (FormEditGameSettings aboutBox = new FormEditGameSettings(game, model.Settings.InstalledVersions))
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
                model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');

                if (GetCurrentVersion().VersionNumber < 1110)
                {
                    if (!Directory.Exists(GetCurrentVersion().Folder + "\\mlc01\\emulatorSave\\" + model.GameData[model.CurrentId].SaveDir))
                    {
                        Directory.CreateDirectory(GetCurrentVersion().Folder + "\\mlc01\\emulatorSave\\" + model.GameData[model.CurrentId].SaveDir);
                    }
                    Process.Start(GetCurrentVersion().Folder + "\\mlc01\\emulatorSave\\" + model.GameData[model.CurrentId].SaveDir);
                }
                else
                {
                    string gameId = model.GameData[model.CurrentId].TitleId.Replace("00050000", "");

                    if (!Directory.Exists(GetCurrentVersion().Folder +"\\mlc01\\usr\\save\\00050000\\" + gameId + "\\user\\"))
                    {
                        Directory.CreateDirectory(GetCurrentVersion().Folder + "\\mlc01\\usr\\save\\00050000\\" + gameId + "\\user\\");
                    }
                    Process.Start(GetCurrentVersion().Folder + "\\mlc01\\usr\\save\\00050000\\" + gameId + "\\user\\");
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
                model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                if (Directory.Exists(GetCurrentVersion().Folder + "\\mlc01\\emulatorSave\\" + model.GameData[model.CurrentId].SaveDir))
                {
                    Process.Start(Path.GetDirectoryName(model.GameData[model.CurrentId].LaunchFile) + "\\..\\..");
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
                model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                if (Directory.Exists(GetCurrentVersion().Folder + "\\shaderCache\\transferable"))
                {
                    if (File.Exists(GetCurrentVersion().Folder + "\\shaderCache\\transferable\\" + model.GameData[model.CurrentId].SaveDir + ".bin"))
                    {
                        Process.Start("explorer.exe", "/select, " + GetCurrentVersion().Folder + "\\shaderCache\\transferable\\" + model.GameData[model.CurrentId].SaveDir + ".bin");
                    }
                    else
                    {
                        Process.Start("explorer.exe", "/select, " + GetCurrentVersion().Folder + "\\shaderCache\\transferable\\");
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
            Persistence.Save(model, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Budford\\Model.xml");
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
            model.Settings.ShowStausBar = showStatusToolStripMenuItem1.Checked;
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
            model.Settings.ShowToolBar = showToolbarToolStripMenuItem.Checked;             
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void installVS2015RedistributablesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            WindowsOS.IsVC2013RedistInstalled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void installVS2015RedistributablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowsOS.IsVC2015RedistInstalled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void installVS2012RedistributablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowsOS.IsVC2012RedistInstalled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void installVS2012RedistributablesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            WindowsOS.IsVC2010RedistInstalled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void installVS2012RedistributablesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            WindowsOS.IsVC2008RedistInstalled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void installVS2012RedistributablesToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            WindowsOS.IsVC2005RedistInstalled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dumpSaveDirCodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Persistence.Save(model);
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
                    FileManager.ImportShaderCache(model, dlg.FileName);
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
        private void installedVersionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormEditInstalledVersions installedVersions = new FormEditInstalledVersions(model, unpacker, launcher, fileManager))
            {
                installedVersions.ShowDialog(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Play
            if (listView1.SelectedItems.Count == 1)
            {
                if (model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                {
                    model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                    GameInformation game = model.GameData[model.CurrentId];
                    launcher.LaunchCemu(model, game);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            // Play
            if (listView1.SelectedItems.Count == 1)
            {
                if (model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                {
                    model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                    GameInformation game = model.GameData[model.CurrentId];
                    launcher.LaunchCemu(model, game);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            launcher.KillCurrentProcess();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tToolStripMenuItem_Click(object sender, EventArgs e)
        {
            model.Settings.CurrentView = "Tiled";
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
            model.Settings.CurrentView = "Detailed";
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
                model.CurrentId = listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ');
                var setting = model.GameData[model.CurrentId].GameSetting;
                InstalledVersion version;
                if (setting.PreferedVersion != "Latest")
                {
                    version = model.Settings.InstalledVersions.FirstOrDefault(v => v.Name == setting.PreferedVersion);
                }
                else
                {
                    version = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
                }

                return version;
            }
            return model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lanchCemuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            launcher.LaunchCemu(model, null, false, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CemuFeatures.DownloadCompatibilityStatus(this, model);
            PopulateListView();
            ResizeColumnHeaders();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void launchCemuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            launcher.LaunchCemu(model, null, false, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadCompatabilityStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CemuFeatures.DownloadCompatibilityStatus(this, model);  
            PopulateListView();
            ResizeColumnHeaders();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateSaveDirDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(ThreadProc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateInfo"></param>
        void ThreadProc(Object stateInfo)
        {
            foreach (var game in model.GameData)
            {
                if (game.Value.SaveDir.StartsWith("??"))
                {
                    model.CurrentId = game.Key;
                    launcher.LaunchCemu(model, game.Value, true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fixUnityGameSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var v in model.GameData)
            {
                GameInformation gi = v.Value;

                //if (gi.GameSetting.GpuBufferCacheAccuracy == GameSettings.GpuBufferCacheAccuracyType.Medium)
                //{
                //    gi.GameSetting.GpuBufferCacheAccuracy = GameSettings.GpuBufferCacheAccuracyType.High;
                //}

                //if (gi.LaunchFileName.ToLower().StartsWith("unity"))
                //{
                //    gi.GameSetting.GpuBufferCacheAccuracy = GameSettings.GpuBufferCacheAccuracyType.High;
                //    gi.GameSetting.PreferedVersion = "Latest";
                //}

                //if (gi.LaunchFileName == "WiiULauncher.rpx")
                //{
                //    gi.GameSetting.EmulationState = GameSettings.EmulationStateType.Unplayable;
                //}


                //if (gi.GameSetting.EmulationState == GameSettings.EmulationStateType.Perfect)
                //{
                    //if (gi.GameSetting.EmulationState == GameSettings.EmulationStateType.NotSet)
                    //{
                    //    gi.GameSetting.EmulationState = GameSettings.EmulationStateType.Perfect;
                    //}
                   // gi.Rating = 3;
                //}

                //if (gi.GameSetting.EmulationState == GameSettings.EmulationStateType.Playable)
                //{
                    //if (gi.GameSetting.EmulationState == GameSettings.EmulationStateType.NotSet)
                    //{
                    //    gi.GameSetting.EmulationState = GameSettings.EmulationStateType.Playable;
                    //}
                    //gi.Rating = 3;
                //}

                //if (gi.LaunchFileName == "WiiULauncher.rpx")
                //{
                //    gi.Comments = "Game crashes instantly on start up";
                //}
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
            using (FormEditInstalledVersions installedVersions = new FormEditInstalledVersions(model, unpacker, launcher, fileManager))
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
            if (CemuFeatures.DownloadLatestVersion(this, model.Settings))
            {
                FileManager.DownloadCemu(this, unpacker, model, FormEditInstalledVersions.uris, FormEditInstalledVersions.filenames);
                manageInstalledVersionsToolStripMenuItem_Click(sender, e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            launcher.Open(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copySavesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopySaves cs = new CopySaves(model);
            cs.Execute();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyShadersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyShaderCache cs = new CopyShaderCache(model);
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
                using (StreamWriter sw = new StreamWriter("C:\\Development\\" + cv.Version + ".txt"))
                {
                    foreach (var gd in model.GameData)
                    {
                        GameInformation gi = gd.Value;
                        gi.GameSetting.UseRtdsc = 1;
                        //sw.WriteLine(gi.SaveDir.Trim() + ", " + gi.Name + ", " + gi.GameSetting.EmulationState.ToString());
                    }
                }
            }
            //using (StreamWriter sw = new StreamWriter("C:\\Development\\broken.csv"))
            //{
            //    sw.WriteLine("Region, Title, Wiki Status, 1.11.1 Status, Launch file, Comment");
            //    foreach (var gd in model.GameData)
            //    {
            //        GameInformation gi = gd.Value;
            //        if (gi.GameSetting.EmulationState == GameSettings.EmulationStateType.Unplayable || gi.GameSetting.EmulationState == GameSettings.EmulationStateType.Loads)
            //        {
            //            if (gi.GameSetting.EmulationState != gi.GameSetting.OfficialEmulationState)
            //            {
            //                if (gi.Comments.Length > 0)
            //                {
            //                    sw.Write("\"");
            //                    sw.Write(gi.Region); sw.Write("\",\"");
            //                    sw.Write(gi.Name); sw.Write("\",\"");
            //                    sw.Write(gi.GameSetting.OfficialEmulationState.ToString()); sw.Write("\",\"");
            //                    sw.Write(gi.GameSetting.EmulationState.ToString()); sw.Write("\",\"");
            //                    sw.Write(gi.LaunchFileName); sw.Write("\",\"");
            //                    sw.Write(gi.Comments.Trim().Replace("\n", "").Replace("\r", "").Replace("\"", "'")); sw.Write("\"");
            //                    sw.WriteLine();
            //                }
            //            }
            //        }
            //    }
            //    sw.WriteLine();
            //    foreach (var gd in model.GameData)
            //    {
            //        GameInformation gi = gd.Value;
            //        if (gi.GameSetting.EmulationState == GameSettings.EmulationStateType.Unplayable || gi.GameSetting.EmulationState == GameSettings.EmulationStateType.Loads)
            //        {

            //            if (gi.GameSetting.EmulationState == gi.GameSetting.OfficialEmulationState)
            //            {
            //                if (gi.Comments.Length > 0)
            //                {
            //                    sw.Write("\"");
            //                    sw.Write(gi.Region); sw.Write("\",\"");
            //                    sw.Write(gi.Name); sw.Write("\",\"");
            //                    sw.Write(gi.GameSetting.OfficialEmulationState.ToString()); sw.Write("\",\"");
            //                    sw.Write(gi.GameSetting.EmulationState.ToString()); sw.Write("\",\"");
            //                    sw.Write(gi.LaunchFileName); sw.Write("\",\"");
            //                    sw.Write(gi.Comments.Trim().Replace("\n", "").Replace("\r", "").Replace("\"", "'")); sw.Write("\"");
            //                    sw.WriteLine();
            //                }
            //            }
            //        }
            //    }
            //}
            //using (StreamWriter sw = new StreamWriter("C:\\Development\\running.csv"))
            //{
            //    sw.WriteLine("Region, Title, Wiki Status, 1.11.1 Status, Launch file, Comment");
            //    foreach (var gd in model.GameData)
            //    {
            //        GameInformation gi = gd.Value;
            //        if (gi.GameSetting.EmulationState != GameSettings.EmulationStateType.Unplayable && gi.GameSetting.EmulationState != GameSettings.EmulationStateType.Loads)
            //        {
            //            if (gi.GameSetting.EmulationState != gi.GameSetting.OfficialEmulationState)
            //            {
            //                //if (gi.Comments.Length > 0)
            //                {
            //                    sw.Write("\"");
            //                    sw.Write(gi.Region); sw.Write("\",\"");
            //                    sw.Write(gi.Name); sw.Write("\",\"");
            //                    sw.Write(gi.GameSetting.OfficialEmulationState.ToString()); sw.Write("\",\"");
            //                    sw.Write(gi.GameSetting.EmulationState.ToString()); sw.Write("\",\"");
            //                    sw.Write(gi.LaunchFileName); sw.Write("\",\"");
            //                    sw.Write(gi.Comments.Trim().Replace("\n", "").Replace("\r", "").Replace("\"", "'")); sw.Write("\"");
            //                    sw.WriteLine();
            //                }
            //            }
            //        }
            //    }
            //    sw.WriteLine();
            //    foreach (var gd in model.GameData)
            //    {
            //        GameInformation gi = gd.Value;
            //        if (gi.GameSetting.EmulationState != GameSettings.EmulationStateType.Unplayable && gi.GameSetting.EmulationState != GameSettings.EmulationStateType.Loads)
            //        {
            //            if (gi.GameSetting.EmulationState == gi.GameSetting.OfficialEmulationState)
            //            {
            //                //if (gi.Comments.Length > 0)
            //                {
            //                    sw.Write("\"");
            //                    sw.Write(gi.Region); sw.Write("\",\"");
            //                    sw.Write(gi.Name); sw.Write("\",\"");
            //                    sw.Write(gi.GameSetting.OfficialEmulationState.ToString()); sw.Write("\",\"");
            //                    sw.Write(gi.GameSetting.EmulationState.ToString()); sw.Write("\",\"");
            //                    sw.Write(gi.LaunchFileName); sw.Write("\",\"");
            //                    sw.Write(gi.Comments.Trim().Replace("\n", "").Replace("\r", "").Replace("\"", "'")); sw.Write("\"");
            //                    sw.WriteLine();
            //                }
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void allToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // All
            perfectToolStripMenuItem.Checked = model.Filters.ViewStatusPerfect = true;
            playableToolStripMenuItem.Checked = model.Filters.ViewStatusPlayable = true;
            runsToolStripMenuItem.Checked = model.Filters.ViewStatusRuns = true;
            loadsToolStripMenuItem.Checked = model.Filters.ViewStatusLoads = true;
            unplayableToolStripMenuItem.Checked = model.Filters.ViewStatusUnplayable = true;
            notSetToolStripMenuItem.Checked = model.Filters.ViewStatusNotSet = true;
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
            perfectToolStripMenuItem.Checked = model.Filters.ViewStatusPerfect = false;
            playableToolStripMenuItem.Checked = model.Filters.ViewStatusPlayable = false;
            runsToolStripMenuItem.Checked = model.Filters.ViewStatusRuns = false;
            loadsToolStripMenuItem.Checked = model.Filters.ViewStatusLoads = false;
            unplayableToolStripMenuItem.Checked = model.Filters.ViewStatusUnplayable = false;
            notSetToolStripMenuItem.Checked = model.Filters.ViewStatusNotSet = false;
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
            officiallyPerfectToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusPerfect = true;
            officiallyPlayableToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusPlayable = true;
            officiallyRunsToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusRuns = true;
            officiallyLoadsToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusLoads = true;
            officiallyUnplayableToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusUnplayable = true;
            officiallyNotSetToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusNotSet = true;
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
            officiallyPerfectToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusPerfect = false;
            officiallyPlayableToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusPlayable = false;
            officiallyRunsToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusRuns = false;
            officiallyLoadsToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusLoads = false;
            officiallyUnplayableToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusUnplayable = false;
            officiallyNotSetToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusNotSet = false;
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
            usaToolStripMenuItem.Checked = model.Filters.ViewRegionUsa = true;
            europeToolStripMenuItem.Checked = model.Filters.ViewRegionEur = true;
            japanToolStripMenuItem.Checked = model.Filters.ViewRegionJap = true;
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
            usaToolStripMenuItem.Checked = model.Filters.ViewRegionUsa = false;
            europeToolStripMenuItem.Checked = model.Filters.ViewRegionEur = false;
            japanToolStripMenuItem.Checked = model.Filters.ViewRegionJap = false;
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
            wiiUToolStripMenuItem.Checked = model.Filters.ViewTypeWiiU = true;
            eShopToolStripMenuItem.Checked = model.Filters.ViewTypeEshop = true;
            channelToolStripMenuItem.Checked = model.Filters.ViewTypeChannel = true;
            virtualConsoleToolStripMenuItem.Checked = model.Filters.ViewTypeVc = true;
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
            wiiUToolStripMenuItem.Checked = model.Filters.ViewTypeWiiU = false;
            eShopToolStripMenuItem.Checked = model.Filters.ViewTypeEshop = false;
            channelToolStripMenuItem.Checked = model.Filters.ViewTypeChannel = false;
            virtualConsoleToolStripMenuItem.Checked = model.Filters.ViewTypeVc = false;
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
            rating5ToolStripMenuItem.Checked = model.Filters.ViewRating5 = true;
            rating4ToolStripMenuItem.Checked = model.Filters.ViewRating4 = true;
            rating3ToolStripMenuItem.Checked = model.Filters.ViewRating3 = true;
            rating2ToolStripMenuItem.Checked = model.Filters.ViewRating2 = true;
            rating1ToolStripMenuItem.Checked = model.Filters.ViewRating1 = true;
            PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void noneToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            rating5ToolStripMenuItem.Checked = model.Filters.ViewRating5 = false;
            rating4ToolStripMenuItem.Checked = model.Filters.ViewRating4 = false;
            rating3ToolStripMenuItem.Checked = model.Filters.ViewRating3 = false;
            rating2ToolStripMenuItem.Checked = model.Filters.ViewRating2 = false;
            rating1ToolStripMenuItem.Checked = model.Filters.ViewRating1 = false;
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
                if (model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                {
                    GameInformation game = model.GameData[listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')];
                    toolStripStatusLabel1.Text = game.Comments;
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

            launcher.model = model;

            foreach (var game in model.GameData)
            {
                if (!game.Value.SaveDir.StartsWith("??"))
                {
                    launcher.CopyLargestShaderCacheToCemu(game.Value);
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
            if (iv1 != null)
            {
                foreach (var game in model.GameData)
                {
                    if (!game.Value.SaveDir.StartsWith("??"))
                    {
                        FileInfo fi = new FileInfo(iv1.Folder + "\\shaderCache\\transferable\\" + game.Value.SaveDir + ".bin");
                        
                        if (fi.Exists)
                        {
                            if (fi.Length > 1000000)
                            {
                                model.CurrentId = game.Key;
                                launcher.LaunchCemu(model, game.Value, true);
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
                if (model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                {
                    GameInformation game = model.GameData[listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')];
                    launcher.CreateSaveSnapshot(model, game);
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
                if (model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                {
                    GameInformation game = model.GameData[listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')];
                    using (FormShapShots ss = new FormShapShots(model, game))
                    {
                        ss.ShowDialog(this);

                        if (ss.LaunchSnapShot != "")
                        {
                            MessageBox.Show("Launching " + ss.LaunchSnapShot);
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
                if (model.GameData.ContainsKey(listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')))
                {
                    GameInformation game = model.GameData[listView1.SelectedItems[0].SubItems[4].Text.TrimEnd(' ')];
                    DirectoryInfo src = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirBudford(model.CurrentUser, game, ""));
                    Process.Start(src.FullName);
                }
            }
        }
    }   
}
