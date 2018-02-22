using Budford.Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Budford.Properties;
using System.IO;

namespace Budford.View
{
    public partial class FormEditInstalledVersion : Form
    {
        readonly List<InstalledVersion> installedVersions;

        readonly InstalledVersion installedVersion;

        readonly string originalName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="installedVersionsIn"></param>
        /// <param name="installedVersionIn"></param>
        public FormEditInstalledVersion(List<InstalledVersion> installedVersionsIn, InstalledVersion installedVersionIn)
        {
            InitializeComponent();

            installedVersions = installedVersionsIn;
            installedVersion = installedVersionIn;

            originalName = installedVersion.Name;
            textBox1.Text = originalName;
            textBox2.Text = installedVersion.Folder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            // Select folder
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox2.Text = fbd.SelectedPath;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            if (originalName != textBox1.Text)
            {
                // We have changed the name, does it clash?
                foreach (var iv in installedVersions)
                {
                    if (iv.Name == textBox1.Text)
                    {
                        MessageBox.Show(Resources.fEditInstalledVersion_button4_Click_Please_select_another_name_before_continuing, Resources.fEditInstalledVersion_button4_Click_Name_exists_, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }
            }
            installedVersion.Name = textBox1.Text;
            installedVersion.Folder = textBox2.Text;

            UpdateVersionName();

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateVersionName()
        {
            if (installedVersion.Version == "")
            {
                int idx = installedVersion.Folder.LastIndexOf(Path.DirectorySeparatorChar);
                if (idx != -1)
                {
                    installedVersion.Version = installedVersion.Folder.Substring(idx + 1);
                }
            }

            if (installedVersion.Name == "")
            {
                installedVersion.Name = installedVersion.Version;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
