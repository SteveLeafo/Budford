using System;
using System.Windows.Forms;

namespace Budford.View
{
    public partial class FormFirstTimeWindow : Form
    {
        public FormFirstTimeWindow()
        {
            InitializeComponent();

            DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

    }
}
