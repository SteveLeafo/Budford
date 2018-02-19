using System.Reflection;
using System.Windows.Forms;
using Budford.Properties;

namespace Budford.View
{
    public partial class FormAbout : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public FormAbout()
        {
            InitializeComponent();
            label2.Text = Resources.FormAbout_FormAbout_V + Assembly.GetExecutingAssembly().GetName().Version; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
