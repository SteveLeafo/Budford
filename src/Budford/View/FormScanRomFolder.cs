using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Budford.Control;
using Budford.Model;
using Budford.Properties;
using Budford.Utilities;

namespace Budford.View
{
    public partial class FormScanRomFolder : Form
    {
        readonly string romFolder;
        readonly Dictionary<string, GameInformation> gameData;
        readonly BackgroundWorker backgroundWorker1 = new BackgroundWorker();

        /// <summary>
        ///
        /// </summary>
        public FormScanRomFolder(string romFolderIn, Dictionary<string, GameInformation> gameDataIn)
        {
            InitializeComponent();
            romFolder = romFolderIn;
            gameData = gameDataIn;
            Text = Resources.fScanRomFolder_fScanRomFolder_Scanning_ + romFolder + Resources.fScanRomFolder_fScanRomFolder_____;
            label1.Text = Text;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            // Cancel
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fFileDownload_Load(object sender, EventArgs e)
        {
            // To report progress from the background worker we need to set this property
            backgroundWorker1.WorkerReportsProgress = true;
            // This event will be raised on the worker thread when the worker starts
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            // This event will be raised when we call ReportProgress
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;

            backgroundWorker1.RunWorkerCompleted += BackgroundWorker1_RunWorkerCompleted;

            backgroundWorker1.RunWorkerAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// On worker thread so do our thing!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Directory.Exists(romFolder))
            {
                if (Directory.Exists(Path.Combine(romFolder, "code")))
                {
                    CheckFolder(romFolder);
                }

                int currentFolder = 0;

                int folderCount = Directory.EnumerateDirectories(romFolder).Count();

                foreach (var folder in Directory.EnumerateDirectories(romFolder))
                {
                    if (Directory.Exists(Path.Combine(folder, "code")))
                    {
                        CheckFolder(folder);
                    }

                    float percent = currentFolder / (float)folderCount * 100.0f;
                    backgroundWorker1.ReportProgress((int)percent);
                    currentFolder++;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        private void CheckFolder(string folder)
        {
            foreach (var file in Directory.EnumerateFiles(Path.Combine(folder, "code")))
            {
                var extension = Path.GetExtension(file);
                if (extension != null && extension.ToUpper() == ".RPX")
                {
                    if (Directory.Exists(Path.Combine(folder, "meta")))
                    {
                        if (File.Exists(Path.Combine(folder, Path.Combine("meta", "meta.xml"))))
                        {
                            XDocument xDoc = XDocument.Load(Path.Combine(folder, Path.Combine("meta", "meta.xml")));
                            XElement xElement = XElement.Parse(xDoc.ToString());

                            ReadMetaData(folder, file, xElement);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="file"></param>
        /// <param name="xElement"></param>
        private void ReadMetaData(string folder, string file, XElement xElement)
        {
            var productCode = Xml.GetValue(xElement, "product_code");
            if (productCode != null)
            {
                var companyCode = Xml.GetValue(xElement, "company_code");
                if (companyCode != null)
                {
                    string key = productCode.Replace("WUP-P-", "").Replace("WUP-U-", "").Replace("WUP-N-", "") + companyCode;

                    GameInformation game;

                    if (!gameData.ContainsKey(key))
                    {
                        game = new GameInformation { GameSetting = new GameSettings() };
                        gameData.Add(key, game);
                        game.Name = Xml.GetValue(xElement, "longname_en").Replace("\n", " ");
                        game.Region = Nintendo.GetRegion(Xml.GetValue(xElement, "region"));
                        game.Publisher = Xml.GetValue(xElement, "publisher_en");
                        game.ProductCode = productCode;
                        game.CompanyCode = companyCode;
                        game.TitleId = Xml.GetValue(xElement, "title_id").ToUpper();
                        game.GroupId = Xml.GetValue(xElement, "group_id").ToUpper();
                        game.Size = (FolderScanner.GetDirectorySize(folder) / 1024 / 1024).ToString("N0") + " MB";
                        game.LaunchFile = file;
                        game.LaunchFileName = Path.GetFileName(file);
                    }
                    else
                    {
                        game = gameData[key];
                        game.Name = Xml.GetValue(xElement, "longname_en").Replace("\n", " ");
                        game.Region = Nintendo.GetRegion(Xml.GetValue(xElement, "region"));
                        game.Publisher = Xml.GetValue(xElement, "publisher_en");
                        game.ProductCode = productCode;
                        game.CompanyCode = companyCode;
                        game.TitleId = Xml.GetValue(xElement, "title_id").ToUpper();
                        game.GroupId = Xml.GetValue(xElement, "group_id").ToUpper();
                        game.LaunchFile = file;
                        game.LaunchFileName = Path.GetFileName(file);
                    }

                   
                }
            }
        }

        // Back on the 'UI' thread so we can update the progress bar
        void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // The progress percentage is a property of e
            progressBar1.Value = e.ProgressPercentage;
        }
    }
}
