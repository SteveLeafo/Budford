using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Budford.Properties;
using Budford.Utilities;

namespace Budford.View
{
    public partial class FormShaderMerger : Form
    {
        FileCache cache;
        readonly HashSet<Tuple<ulong, ulong>> items = new HashSet<Tuple<ulong, ulong>>();
        int id = 1;

        public FormShaderMerger()
        {
            InitializeComponent();
            listView1.Columns[1].Width *= 2;
            listView1.Columns[2].Width *= 2;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = Resources.FormShaderMerger_openToolStripMenuItem_Click_Shader_Cache_Files____bin_;

                // Show open file dialog box 
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    cache = FileCache.fileCache_openExisting(dlg.FileName, 1);
                    UpdateListView();
                }
            }
        }

        void UpdateListView()
        {
            listView1.BeginUpdate();
            foreach (var item in cache.FileTableEntries)
            {
                //if (!items.Contains(new Tuple<ulong, ulong>(item.name1, item.name2)))
                {
                    ListViewItem lvi = new ListViewItem(id.ToString());
                    lvi.SubItems.Add(item.Name1.ToString("X"));
                    lvi.SubItems.Add(item.Name2.ToString("X"));
                    lvi.SubItems.Add(item.FileOffset.ToString());
                    lvi.SubItems.Add(item.FileSize.ToString());
                    lvi.SubItems.Add(item.ExtraReserved.ToString("X"));

                    listView1.Items.Add(lvi);
                    id++;
                    items.Add(new Tuple<ulong, ulong>(item.Name1, item.Name2));
                }
            }
            listView1.EndUpdate();
            //if (id > originalCount)
            //{
            //    MessageBox.Show("Added " + (id - originalCount).ToString() + " new shaders");
            //    MessageBox.Show("Added " + FileCache.fileCache_countFileEntries(cache) + " new shaders");
            MessageBox.Show(Resources.FormShaderMerger_UpdateListView_Added_ + FileCache.fileCache_getFileEntryCount(cache) + Resources.FormShaderMerger_UpdateListView__new_shaders);
            //}
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            items.Clear();
            id = 1;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = Resources.FormShaderMerger_saveAsToolStripMenuItem_Click_Shader_Cache_Files____bin_;

                // Show open file dialog box 
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    FileCache newCache = FileCache.fileCache_create(dlg.FileName, 1);
                    int c;
                    c = cache.FileTableEntries.Length;
                    for (int i = 1; i < c; ++i)
                    {
                        int sz = 0;
                        byte[] file = FileCache.fileCache_getFile(cache, cache.FileTableEntries[i].Name1, cache.FileTableEntries[i].Name2, ref sz);
                        FileCache.fileCache_addFile(newCache, cache.FileTableEntries[i].Name1, cache.FileTableEntries[i].Name2, file, sz, cache.FileTableEntries[i].ExtraReserved);
                    }
                }
            }
        }
    }
}
