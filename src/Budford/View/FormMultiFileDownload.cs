using Budford.Model;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Budford.Control;
using System.Net;
using System.ComponentModel;

namespace Budford.View
{
    public partial class FormMultiFileDownload : Form
    {
        // Files to download
        private readonly string[] uris = { 
            "http://cemu.info/",
            "https://sshnuke.net/cemuhook",
            "https://api.github.com/repos/slashiee/cemu_graphic_packs/releases/latest",
            "","","" };

        private readonly string[] labels = { 
            "Checking latest Cemu version",
            "Checking latest Cemu Hook version",
            "Checking latest Graphic Packs version",
            "Downloading latest Cemu",
            "Downloading latest Cemu hook",
            "Downloading latest Graphic Packs" };

        // Local file names
        private readonly string[] fileNames = { "", "", "", "", "", "" };

        // The file we are currently downloading
        private int currentFile;

        // Where the web pages end up
        internal string Result = "";

        readonly Model.Model model;
        readonly Unpacker unpacker;
        /// <summary>
        ///
        /// </summary>
        public FormMultiFileDownload(Model.Model modelIn)
        {
            InitializeComponent();
            model = modelIn;
            unpacker = new Unpacker(this);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            currentFile = uris.Length;
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
            if (currentFile < uris.Length)
            {
                if (uris[currentFile].Length == 0)
                {
                    currentFile++;
                    LoadNextFile();
                    return;
                }
            }
            else
            {
                Close();
            }
            progressBar1.Value = 0;
            progressBar2.Value = currentFile;
            if (currentFile < fileNames.Length)
            {
                label1.Text = labels[currentFile];
                if (uris[currentFile].Length > 0)
                {
                    if (fileNames[currentFile].Length > 0)
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                            wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                            wc.DownloadFileAsync(new Uri(uris[currentFile]), fileNames[currentFile]);
                        }
                    }
                    else
                    {
                        using (CustomWebClient wc = new CustomWebClient())
                        {
                            ServicePointManager.Expect100Continue = true;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons
                            wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                            wc.UseDefaultCredentials = true;
                            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                            wc.DownloadStringCompleted += Wc_DownloadStringCompleted;
                            wc.DownloadStringAsync(new Uri(uris[currentFile]));
                        }
                    }
                }
                currentFile++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            switch (currentFile)
            {
                case 4: DoTheCemu2();
                    break;
                case 5: DoTheCemuHook2();
                    break;
                case 6: DoTheGraphicPack2();
                    break;
            }

            if (currentFile < uris.Length)
            {
                LoadNextFile();
            }
            else
            {
                Close();
            }
        }

        private void DoTheCemu()
        {
            foreach (var line in Result.Split('\n'))
            {
                if (line.Contains("name=\"download\""))
                {
                    string[] toks = line.Split('=');
                    uris[3] = toks[1].Substring(1, toks[1].LastIndexOf('\"') - 1);
                    fileNames[3] = uris[3].Substring(1 + uris[3].LastIndexOf('/'));
                    int currentVersion = InstalledVersion.GetVersionNumber(Path.GetFileName(fileNames[3]));
                    if (IsInstalled(currentVersion))
                    {
                        uris[3] = "";
                    }
                }
            }
        }

        void DoTheCemu2()
        {
            unpacker.Unpack(fileNames[3], model.Settings.DefaultInstallFolder);
            FileManager.SearchForInstalledVersions(model);
            CemuFeatures.UpdateFeaturesForInstalledVersions(model);
            int latestVersion = 0;
            InstalledVersion latest = null;
            foreach (var v in model.Settings.InstalledVersions)
            {
                if (v.VersionNumber > latestVersion)
                {
                    latestVersion = v.VersionNumber;
                    latest = v;
                    latest.IsLatest = false;
                }
            }
            if (latest != null)
            {
                latest.IsLatest = true;
            }
        }

        void DoTheCemuHook2()
        {
            InstalledVersion ver = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
            if (ver != null)
            {
                unpacker.Unpack(fileNames[4], ver.Folder);
                File.Copy(fileNames[4], "cemu_hook.zip", true);
                CemuFeatures.RepairInstalledVersions(this, model);
            }
        }

        void DoTheGraphicPack2()
        {
            string packName = Path.GetFileNameWithoutExtension(uris[5]);
            unpacker.Unpack(fileNames[5], Path.Combine("graphicsPacks", packName));            
        }

        private void DoTheCemuHook()
        {
            foreach (var line in Result.Split('\n'))
            {
                string s = line.Trim();
                if (s.Contains(".zip"))
                {
                    if (s.Length > 20)
                    {
                        s = s.Substring(39);
                        int p = s.IndexOf("\"", StringComparison.Ordinal);
                        if (p > -1)
                        {
                            if (!File.Exists(s.Substring(0, p)))
                            {
                            uris[4] = "https://files.sshnuke.net/" + s.Substring(0, p);
                            fileNames[4] = s.Substring(0, p);
                            }
                            return;
                        }
                    }
                }
            }
        }

        private void DoTheGraphicPack()
        {
            // For that you will need to add reference to System.Runtime.Serialization
            var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(Result.ToCharArray()), new System.Xml.XmlDictionaryReaderQuotas());

            // For that you will need to add reference to System.Xml and System.Xml.Linq
            var root = XElement.Load(jsonReader);
            var xElement = root.Elements("assets").First().Elements().First().Element("browser_download_url");
            if (xElement != null)
            {
                string uri = xElement.Value;

                string packName = Path.GetFileNameWithoutExtension(uri);

                if (!CemuFeatures.IsGraphicPackInstalled(packName))
                {
                    if (File.Exists("tempGraphicPack.zip"))
                    {
                        File.Delete("tempGraphicPack.zip");
                    }
                    uris[5] = uri;
                    fileNames[5] = "tempGraphicPack.zip";
                }
            }
        }

        /// <summary>
        /// Returns true if requested version is installed
        /// </summary>
        /// <param name="versionNo"></param>
        /// <returns></returns>
        bool IsInstalled(int versionNo)
        {
            foreach (var version in model.Settings.InstalledVersions)
            {
                if (version.VersionNumber == versionNo)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Result = e.Result;

            switch (currentFile)
            {
                case 1: DoTheCemu();
                    break;
                case 2: DoTheCemuHook();
                    break;
                case 3: DoTheGraphicPack();
                    break;
            }

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
