using System;
using System.Net;
using System.Windows.Forms;
using Budford.Properties;

namespace Budford
{
    public partial class FormWebpageDownload : Form
    {
        readonly string uri;
        internal string Result = "";

        /// <summary>
        ///
        /// </summary>
        public FormWebpageDownload(string urlIn, string fileNameIn)
        {
            InitializeComponent();
            uri = urlIn;
            label1.Text = Resources.fWebpageDownload_fWebpageDownload_Downloading_ + fileNameIn + Resources.fWebpageDownload_fWebpageDownload____;
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
            // TODO - Use scraper to find current latest version
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadStringCompleted += Wc_DownloadStringCompleted;
                wc.DownloadStringAsync(new Uri(uri));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Result = e.Result;
            DialogResult = DialogResult.OK;
            Close();
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
