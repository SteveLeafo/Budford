using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Budford.Utilities;

namespace Budford.View
{
    public partial class FormShaderMerger : Form
    {
        FileCache cache;
        HashSet<Tuple<ulong, ulong>> items = new HashSet<Tuple<ulong, ulong>>();
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
                dlg.Filter = "Shader Cache Files| *.bin;";

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
            int originalCount = id;
            listView1.BeginUpdate();
            foreach (var item in cache.fileTableEntries)
            {
                //if (!items.Contains(new Tuple<ulong, ulong>(item.name1, item.name2)))
                {
                    ListViewItem lvi = new ListViewItem(id.ToString());
                    lvi.SubItems.Add(item.name1.ToString("X"));
                    lvi.SubItems.Add(item.name2.ToString("X"));
                    lvi.SubItems.Add(item.fileOffset.ToString());
                    lvi.SubItems.Add(item.fileSize.ToString());
                    lvi.SubItems.Add(item.extraReserved.ToString("X"));

                    listView1.Items.Add(lvi);
                    id++;
                    items.Add(new Tuple<ulong, ulong>(item.name1, item.name2));
                }
            }
            listView1.EndUpdate();
            //if (id > originalCount)
            //{
            //    MessageBox.Show("Added " + (id - originalCount).ToString() + " new shaders");
            //    MessageBox.Show("Added " + FileCache.fileCache_countFileEntries(cache) + " new shaders");
            MessageBox.Show("Added " + FileCache.fileCache_getFileEntryCount(cache) + " new shaders");
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
                dlg.Filter = "Shader Cache Files| *.bin;";

                // Show open file dialog box 
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    FileCache newCache = FileCache.fileCache_create(dlg.FileName, 1);
                    int c = 5;
                    c = cache.fileTableEntries.Length;
                    for (int i = 1; i < c; ++i)
                    {
                        int sz = 0;
                        byte[] file = FileCache.fileCache_getFile(cache, cache.fileTableEntries[i].name1, cache.fileTableEntries[i].name2, ref sz);
                        FileCache.fileCache_addFile(newCache, cache.fileTableEntries[i].name1, cache.fileTableEntries[i].name2, file, sz, cache.fileTableEntries[i].extraReserved);
                    }
                }
            }
        }
    }
}
