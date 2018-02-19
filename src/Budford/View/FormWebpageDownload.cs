using System;
using System.Net;
using System.Windows.Forms;
using Budford.Properties;

namespace Budford.View
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
            using (CustomWebClient wc = new CustomWebClient())
            {
                //wc.Headers[HttpRequestHeader.Authorization] = "Basic " + "Steve";
                wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                wc.UseDefaultCredentials = true;
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

    class CustomWebClient : WebClient
    {
        /// <summary>
        /// Returns a <see cref="T:System.Net.WebRequest" /> object for the specified resource.
        /// </summary>
        /// <param name="address">A <see cref="T:System.Uri" /> that identifies the resource to request.</param>
        /// <returns>
        /// A new <see cref="T:System.Net.WebRequest" /> object for the specified resource.
        /// </returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).KeepAlive = false;
            }
            return request;
        }
    }
}
