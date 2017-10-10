using System.Reflection;
using System.Windows.Forms;

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
            label2.Text = "V" + Assembly.GetExecutingAssembly().GetName().Version.ToString(); 
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
