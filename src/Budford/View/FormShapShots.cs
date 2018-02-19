using Budford.Model;
using System;
using System.Windows.Forms;

namespace Budford.View
{
    public partial class FormShapShots : Form
    {
        internal string LaunchSnapShot = "";

        public FormShapShots(Model.Model modelIn, GameInformation gameInformationIn)
        {
            var model = modelIn;

            InitializeComponent();

            foreach (var snapShot in model.ShapShots)
            {
                if (snapShot.User == model.CurrentUser)
                {
                    if (snapShot.GameId == gameInformationIn.SaveDir)
                    {
                        ListViewItem lvi = new ListViewItem(snapShot.Folder);
                        lvi.SubItems.Add(snapShot.Comment);

                        listView1.Items.Add(lvi);
                    }
                }
            }

            for (int c = 0; c < listView1.Columns.Count; ++c)
            {
                listView1.AutoResizeColumn(c, ColumnHeaderAutoResizeStyle.ColumnContent);
                listView1.AutoResizeColumn(c, ColumnHeaderAutoResizeStyle.HeaderSize);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                LaunchSnapShot = listView1.SelectedItems[0].Text;
                Close();
            }
        }
    }
}
