using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;
using Budford.Properties;

namespace Budford.View
{
    public sealed partial class FormFileDownload : Form
    {
        // The uri to download from
        readonly string uri;

        // Where we want to put it
        readonly string fileName;

        // The man of the moment...
        WebClient webClient;

        /// <summary>
        ///
        /// </summary>
        public FormFileDownload(string urlIn, string fileNameIn)
        {
            InitializeComponent();
            uri = urlIn;
            fileName = fileNameIn;
            label1.Text = Resources.fFileDownload_fFileDownload_Downloading_ + fileName + Resources.fFileDownload_fFileDownload____;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            webClient.CancelAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fFileDownload_Load(object sender, EventArgs e)
        {
            webClient = new WebClient();
            webClient.DownloadProgressChanged += wc_DownloadProgressChanged;
            webClient.DownloadFileCompleted += Wc_DownloadFileCompleted;
            webClient.DownloadFileAsync(new Uri(uri), fileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
           
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
