using Budford.Control;
using Budford.Properties;
using Budford.Tools;
using Budford.View;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Budford.Model
{
    internal class ViewUsers
    {
        // The model
        readonly Model model;

        // Our parent form
        readonly FormMainWindow mainForm;

        // Context menu from the parent form
        readonly ContextMenuStrip contextMenuStrip1;

        // Tool strip menu from the parent form
        readonly ToolStripMenuItem userToolStripMenuItem;

        // The users picture on the main form
        readonly PictureBox pictureBox1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="modelIn"></param>
        /// <param name="mainFormIn"></param>
        /// <param name="contextMenuStrip1In"></param>
        /// <param name="userToolStripMenuItemIn"></param>
        /// <param name="pictureBox1In"></param>
        internal ViewUsers(Model modelIn, FormMainWindow mainFormIn, ContextMenuStrip contextMenuStrip1In, ToolStripMenuItem userToolStripMenuItemIn, PictureBox pictureBox1In)
        {
            model = modelIn;
            mainForm = mainFormIn;
            contextMenuStrip1 = contextMenuStrip1In;
            userToolStripMenuItem = userToolStripMenuItemIn;
            pictureBox1 = pictureBox1In;

            SetupCurrentUser();
            AddUserMenuItems();

        }

        /// <summary>
        /// Edit the current user
        /// </summary>
        internal void EditCurrentUser()
        {
            User user = model.Users.FirstOrDefault(u => u.Name == model.CurrentUser);
            using (FormEditUser editUser = new FormEditUser(user))
            {
                editUser.ShowDialog(mainForm);
            }

            if (user != null && File.Exists(Path.Combine("Users", user.Image)))
            {
                using (FileStream stream = new FileStream(Path.Combine("Users", user.Image), FileMode.Open, FileAccess.Read))
                {
                    pictureBox1.Image = Image.FromStream(stream);
                }
            }

            if (user != null && model.CurrentUser != user.Name)
            {
                if (Directory.Exists(Path.Combine("Users", model.CurrentUser)))
                {
                    try
                    {
                        Directory.Move(Path.Combine("Users", model.CurrentUser), Path.Combine("Users", user.Name));
                    }
                    catch (Exception ex)
                    {
                        model.Errors.Add(ex.Message);
                    }
                }
                UpdateMenuStrip(user);
                UpdateContextMenuStrip(user);

                model.CurrentUser = user.Name;
                mainForm.Text = Resources.fMainWindow_fMainWindow_CEMU_Game_DB______Current_User__ + model.CurrentUser;
            }
        }

        /// <summary>
        /// Add a new user item for each user
        /// </summary>
        private void AddUserMenuItems()
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
        /// We are changing user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void User_Click(object sender, EventArgs e)
        {
            var toolStripMenuItem = sender as ToolStripMenuItem;
            if (toolStripMenuItem != null)
            {
                User user = toolStripMenuItem.Tag as User;
                if (user != null)
                {
                    if (user.Name != model.CurrentUser)
                    {
                        DeleteAllLockFiles();
                        if (File.Exists(Path.Combine("Users", user.Image)))
                        {
                            using (FileStream stream = new FileStream(Path.Combine("Users", user.Image), FileMode.Open, FileAccess.Read))
                            {
                                pictureBox1.Image = Image.FromStream(stream);
                            }

                        }
                        mainForm.Text = Resources.fMainWindow_fMainWindow_CEMU_Game_DB______Current_User__ + user.Name;
                        model.CurrentUser = user.Name;
                    }
                }
                UpdateMenuStrip(user);
                UpdateContextMenuStrip(user);
            }
        }

        /// <summary>
        /// The user context menu
        /// </summary>
        /// <param name="user"></param>
        internal void UpdateContextMenuStrip(User user)
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
        /// Update the menu strip
        /// </summary>
        /// <param name="user"></param>
        internal void UpdateMenuStrip(User user)
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
        /// Adding a new user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void addNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            User newUser = new User() { Name = "User " + (model.Users.Count + 1), Image = "default.png" };
            using (FormEditUser editUser = new FormEditUser(newUser))
            {
                if (editUser.ShowDialog(mainForm) == DialogResult.OK)
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
        }

        /// <summary>
        /// Delete all the lock files
        /// </summary>
        private void DeleteAllLockFiles()
        {
            CopySaves cs = new CopySaves(model);
            cs.Execute();
            foreach (var game in model.GameData.OrderByDescending(gd => gd.Value.Name))
            {
                foreach (var version in model.Settings.InstalledVersions)
                {
                    DirectoryInfo dest = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirCemu(version, game.Value));

                    string lockFileName = Path.Combine(dest.FullName, "Budford.lck");
                    FileManager.SafeDelete(lockFileName);
                }
            }
        }

        /// <summary>
        /// Set up a user
        /// </summary>
        private void SetupCurrentUser()
        {
            if (model.Users.Count == 0)
            {
                model.Users.Add(new User() { Name = "Default", Image = "default.png" });
                model.CurrentUser = "Default";
            }

            var firstOrDefault = model.Users.FirstOrDefault(u => u.Name == model.CurrentUser);
            if (firstOrDefault != null && File.Exists(Path.Combine("Users", firstOrDefault.Image)))
            {
                var orDefault = model.Users.FirstOrDefault(u => u.Name == model.CurrentUser);
                if (orDefault != null)
                {
                    using (FileStream stream = new FileStream(Path.Combine("Users", orDefault.Image), FileMode.Open, FileAccess.Read))
                    {
                        pictureBox1.Image = Image.FromStream(stream);
                    }
                }
            }
        }
    }
}
