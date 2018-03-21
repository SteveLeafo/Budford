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
using CNUSLib;

namespace Budford.View
{
    public partial class FormScanRomFolder : Form
    {
        readonly Dictionary<string, GameInformation> gameData;
        readonly BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        Model.Model model;
        /// <summary>
        ///
        /// </summary>
        public FormScanRomFolder(Model.Model modelIn, Dictionary<string, GameInformation> gameDataIn)
        {
            InitializeComponent();
            model = modelIn;
            gameData = gameDataIn;
            Text = Resources.fScanRomFolder_fScanRomFolder_Scanning_ +  Resources.fScanRomFolder_fScanRomFolder_____;
            label1.Text = Text;
        }

        void UpdateDisplay(string romFolder)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => { UpdateDisplay(romFolder); }));
            }
            else
            {
                Text = Resources.fScanRomFolder_fScanRomFolder_Scanning_ + romFolder + Resources.fScanRomFolder_fScanRomFolder_____;
                label1.Text = Text;
            }
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
            Persistence.SetSaveDirs(model);
            Persistence.SetGameTypes(model);
            FolderScanner.AddGraphicsPacksToGames(model);

            Close();
        }

        /// <summary>
        /// On worker thread so do our thing!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            showCommonKeyError = true;

            int folderCount = GetFolderCount();
            int currentFolder = 0;

            foreach (var romFolder in model.Settings.RomFolders)
            {
                if (Directory.Exists(romFolder))
                {
                    UpdateDisplay(romFolder);

                    CheckForImageFiles(romFolder);

                    if (Directory.Exists(Path.Combine(romFolder, "code")))
                    {
                        CheckFolder(romFolder);
                    }

                    foreach (var folder in Directory.EnumerateDirectories(romFolder))
                    {
                        CheckForImageFiles(folder);

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
        }

        private int GetFolderCount()
        {
            int folderCount = 0;
            foreach (var romFolder in model.Settings.RomFolders)
            {
                if (Directory.Exists(romFolder))
                {
                    folderCount += Directory.EnumerateDirectories(romFolder).Count();
                }
            }
            return folderCount;
        }

        private void CheckForImageFiles(string folderIn)
        {
            foreach (var file in Directory.EnumerateFiles(folderIn))
            { 
                 var extension = Path.GetExtension(file);
                 if (extension != null && (extension.ToUpper() == ".WUD" || extension.ToUpper() == ".WUX"))
                 {
                     if (GrabKeys())
                     {
                         string folder = decryptFile(file, "WudData", "/code/.*.rpx", true, null);
                         if (folder == "")
                         {
                             TryCemuKeys(file);
                         }
                         else
                         {
                             UseProvidedKey(file, folder);
                         }
                     }
                     else
                     {
                         break;
                     }
                 }
            }
        }

        private void UseProvidedKey(string file, string folder)
        {
            decryptFile(file, "WudData", "/meta/meta.xml", true, null);
            decryptFile(file, "WudData", "/meta/iconTex.tga", true, null);
            decryptFile(file, "WudData", "/meta/bootLogoTex.tga", true, null);

            ProcessImageFile(file, folder);
        }

        private void TryCemuKeys(string file)
        {
            foreach (var key in keys)
            {
                string folder = decryptFile(file, "WudData", "/code/.*.rpx", true, key);
                if (folder != "")
                {
                    decryptFile(file, "WudData", "/meta/meta.xml", true, key);
                    decryptFile(file, "WudData", "/meta/iconTex.tga", true, key);
                    decryptFile(file, "WudData", "/meta/bootLogoTex.tga", true, key);

                    ProcessImageFile(file, folder);

                    break;
                }
            }
        }

        private void ProcessImageFile(string file, string folder)
        {
            GameInformation gi = CheckWudFolder(Path.Combine("WudData", folder));
            if (gi != null)
            {
                gi.LaunchFile = file;
                gi.LaunchFileName = Path.GetFileName(file);
                gi.Image = true;
                foreach (var rpxFile in Directory.EnumerateFiles(Path.Combine("WudData", folder, "code")))
                {
                    if (Path.GetExtension(rpxFile).ToLower().Contains("rpx"))
                    {
                        gi.RpxFile = rpxFile;
                        FileInfo fi = new FileInfo(file);
                        gi.Size = (fi.Length / 1024 / 1024).ToString("N0") + " MB";
                    }
                }
            }
        }

        bool showCommonKeyError = true;
        bool GrabKeys()
        {
            if (CNUSLib.Settings.commonKey != null)
            {
                String commonKey = model.Settings.WiiUCommonKey;
                if (commonKey == null || commonKey == "")
                {
                    if (showCommonKeyError)
                    {
                        MessageBox.Show("No Common Key found, please set you Wii U common key in the Budford configuration form");
                        showCommonKeyError = false;
                    }
                    return false;
                }
                LoadKeysFromCemu();
                byte[] key = Utils.StringToByteArray(commonKey);
                SetCommonKey(key);
            }
            return true;
        }

        private static void SetCommonKey(byte[] key)
        {
            CNUSLib.Settings.commonKey = key;
        }

        List<byte[]> keys = new List<byte[]>();
        void LoadKeysFromCemu()
        {
            keys.Clear();
            var version = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
            string  keysFile = Path.Combine(version.Folder, "keys.txt");
            if (File.Exists(keysFile))
            {
                string[] lines = File.ReadAllLines(keysFile);
                foreach (string line in lines)
                {
                    string keyEntry = "";
                    if (line.Contains("#"))
                    {
                        string[] toks = line.Split('#');
                        if (toks.Length > 0)
                        {
                            keyEntry = toks[0].Trim();
                        }
                    }
                    else
                    {
                        keyEntry = line.Trim();
                    }
                    if (keyEntry.Length == 32)
                    {
                        byte[] key = Utils.StringToByteArray(keyEntry);
                        keys.Add(key);
                    }
                }
            }
        }


        private static string decryptFile(String input, String output, String regex, bool overwrite, byte[] titlekey)
        {
            if (input == null)
            {
                MessageBox.Show("You need to provide an input file");
            }
            FileInfo inputFile = new FileInfo(input);

            NUSTitle title = NUSTitleLoaderWUD.loadNUSTitle(inputFile.FullName, titlekey);
            if (title == null)
            {
                return "";
            }


            if (output == null)
            {
                output = title.TMD.titleID.ToString("X16");
            }
            else
            {
                output += Path.DirectorySeparatorChar + title.TMD.titleID.ToString("X16");
            }

            FileInfo outputFolder = new FileInfo(output);

            title.skipExistingFiles = (!overwrite);
            DecryptionService decryption = DecryptionService.getInstance(title);


            decryption.decryptFSTEntriesTo(regex, outputFolder.FullName);

            return title.TMD.titleID.ToString("X16");
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
        private GameInformation CheckWudFolder(string folder)
        {
            if (Directory.Exists(Path.Combine(folder, "meta")))
            {
                if (Directory.Exists(Path.Combine(folder, "meta")))
                {
                    if (File.Exists(Path.Combine(folder, Path.Combine("meta", "meta.xml"))))
                    {
                        XDocument xDoc = XDocument.Load(Path.Combine(folder, Path.Combine("meta", "meta.xml")));
                        XElement xElement = XElement.Parse(xDoc.ToString());

                        return ReadMetaData(folder, Path.Combine(folder, Path.Combine("meta", "meta.xml")), xElement);
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="file"></param>
        /// <param name="xElement"></param>
        private GameInformation ReadMetaData(string folder, string file, XElement xElement)
        {
            var productCode = Xml.GetValue(xElement, "product_code");
            if (productCode != null)
            {
                var companyCode = Xml.GetValue(xElement, "company_code");
                if (companyCode != null)
                {
                    string key = productCode.Replace("WUP-P-", "").Replace("WUP-U-", "").Replace("WUP-N-", "") + companyCode;

                    GameInformation game;

                    string version = Xml.GetValue(xElement, "title_version");
                    int titleVersion;
                    int.TryParse(version, out titleVersion);

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
                        game.TitleVersion = titleVersion;
                        game.GroupId = Xml.GetValue(xElement, "group_id").ToUpper();
                        game.Size = (FolderScanner.GetDirectorySize(folder) / 1024 / 1024).ToString("N0") + " MB";
                        game.LaunchFile = file;
                        game.LaunchFileName = Path.GetFileName(file);
                    }
                    else
                    {
                        game = gameData[key];
                        if (titleVersion < game.TitleVersion)
                        {
                            ClearSaveDir(game, titleVersion);
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
                    return game;
                   
                }
            }
            return null;
        }

        private static void ClearSaveDir(GameInformation game, int titleVersion)
        {
            if (titleVersion != 0 && game.TitleVersion != int.MaxValue)
            {
                game.SaveDir = "??";
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
