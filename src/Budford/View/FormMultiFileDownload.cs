using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;
using Budford.Properties;

namespace Budford
{
    public partial class FormMultiFileDownload : Form
    {
        // Files to download
        private readonly string[] uris;

        // Local file names
        private readonly string[] fileNames;

        // The file we are currently downloading
        private int currentFile;

        /// <summary>
        ///
        /// </summary>
        public FormMultiFileDownload(string[] urlsIn, string[] fileNamesIn)
        {
            InitializeComponent();
            uris = urlsIn;
            fileNames = fileNamesIn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            // TODO - Cancel
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fFileDownload_Load(object sender, EventArgs e)
        {
            progressBar2.Step = 1;
            progressBar2.Maximum = uris.Length;
            LoadNextFile();
        }

        /// <summary>
        /// 
        /// </summary>
        void LoadNextFile()
        {
            progressBar1.Value = 0;
            progressBar2.Value = currentFile;
            label1.Text = Resources.fFileDownload_fFileDownload_Downloading_ + fileNames[currentFile] + Resources.fFileDownload_fFileDownload____;

            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                wc.DownloadFileAsync(new Uri(uris[currentFile]), fileNames[currentFile]);
            }
            currentFile++;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (currentFile < uris.Length)
            {
                LoadNextFile();
            }
            else
            {
                Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }
    }
}
