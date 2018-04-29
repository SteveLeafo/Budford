using Budford.Control;
using Budford.Properties;
using Budford.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Budford.Model
{
    internal class ViewPlugin
    {
        // All the plug-ins we found
        private readonly List<PlugIns.PlugIn> plugIns = new List<PlugIns.PlugIn>();

        // The data model
        private readonly Model model;

        // A menu to add plug-ins to
        private readonly ToolStripMenuItem plugInsToolStripMenuItem;

        // The parent form
        private readonly Form parent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="modelIn"></param>
        /// <param name="parentIn"></param>
        /// <param name="plugInsToolStripMenuItemIn"></param>
        internal ViewPlugin(Model modelIn, Form parentIn, ToolStripMenuItem plugInsToolStripMenuItemIn)
        {
            model = modelIn;
            parent = parentIn;
            plugInsToolStripMenuItem = plugInsToolStripMenuItemIn;
        }

        /// <summary>
        /// Load all the imported plug-ins into the menu
        /// </summary>
        internal void LoadPlugIns()
        {
            if (Directory.Exists(SpecialFolders.PlugInFolder(model)))
            {
                List<ToolStripItem> items = new List<ToolStripItem>();

                string currentType = "";

                SearchForPlugins(items);

                AddPlugInsToMenu(items, currentType, plugInsToolStripMenuItem);
            }
        }

        /// <summary>
        /// Import a plug-in into the model
        /// </summary>
        internal void ImportPlugIn()
        {
            // Configure open file dialog box 
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = Resources.FormMainWindow_importBudfordPluginToolStripMenuItem_Click_Budford_Plug_in_Files_____xml_;

                // Show open file dialog box 
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    FileManager.SafeCreateDirectory(SpecialFolders.PlugInFolder(model));
                    FileManager.SafeCopy(dlg.FileName, Path.Combine(SpecialFolders.PlugInFolder(model), Path.GetFileName(dlg.FileName)), true);
                    LoadPlugIns();
                }
            }
        }

        /// <summary>
        /// Add plug-ins to menu
        /// </summary>
        /// <param name="items"></param>
        /// <param name="currentType"></param>
        internal static void AddPlugInsToMenu(List<ToolStripItem> items, string currentType, ToolStripMenuItem toolStripMenuItem)
        {
            // Painful, but we want these added to the top of the list...
            toolStripMenuItem.DropDownItems.Clear();
            var v = (from i in items orderby ((PlugIns.PlugIn)i.Tag).Type select i).ToList();
            foreach (var item in v)
            {
                PlugIns.PlugIn p = (PlugIns.PlugIn)item.Tag;
                if (p.Type != currentType)
                {
                    if (currentType != "")
                    {
                        toolStripMenuItem.DropDownItems.Insert(0, new ToolStripSeparator());
                    }
                    currentType = p.Type;
                }
                toolStripMenuItem.DropDownItems.Insert(0, item);
            }
        }

        /// <summary>
        /// Search for installed plug-ins
        /// </summary>
        /// <param name="items"></param>
        private void SearchForPlugins(List<ToolStripItem> items)
        {
            foreach (var file in Directory.EnumerateFiles(SpecialFolders.PlugInFolder(model)))
            {
                var extension = Path.GetExtension(file);
                if (extension != null && extension.ToLower().Contains("xml"))
                {
                    plugInsToolStripMenuItem.Visible = true;

                    PlugIns.PlugIn p = Persistence.LoadPlugin(file);
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
        }

        /// <summary>
        /// Execute a plug-in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlugIn_Click(object sender, EventArgs e)
        {
            var toolStripMenuItem = sender as ToolStripMenuItem;
            if (toolStripMenuItem != null)
            {
                PlugIns.PlugIn plugIn = toolStripMenuItem.Tag as PlugIns.PlugIn;
                if (plugIn != null)
                {
                    if (plugIn.Type == "ExternalTool")
                    {
                        ProcessStartInfo start = new ProcessStartInfo { FileName = plugIn.FileName };
                        Process.Start(start);
                    }
                    else
                    {
                        using (FormExecutePlugIn executor = new FormExecutePlugIn(model, plugIn))
                        {
                            if (executor.ShowDialog(parent) == DialogResult.OK)
                            {
                                MessageBox.Show(plugIn.Name + Resources.FormMainWindow_PlugIn_Click__executed_successfully, Resources.FormMainWindow_PlugIn_Click_Success);
                            }
                        }
                    }
                }
            }
        }
    }
}
