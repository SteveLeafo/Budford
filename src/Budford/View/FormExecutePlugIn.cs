using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.IO.Compression;
using Budford.Control;
using Budford.Model;
using System.Diagnostics;
using Budford.Properties;

namespace Budford.View
{
    public partial class FormExecutePlugIn : Form
    {
        readonly Model.PlugIns.PlugIn plugIn;
        readonly Model.Model model;

        public FormExecutePlugIn()
        {
            InitializeComponent();
        }

        public FormExecutePlugIn(Model.Model modelIn, Model.PlugIns.PlugIn plugInIn) 
        {
            InitializeComponent();

            model = modelIn;
            plugIn = plugInIn;
        }

        private void FormExecutePlugIn_Load(object sender, EventArgs e)
        {
            try
            {
                switch (plugIn.Type)
                {
                    case "ZipImport":
                        ZipImport();
                        DialogResult = DialogResult.OK;
                        Close();
                        break;
                    case "ExternalTool":
                        ExternalTool();
                        break;
                }
            }
            catch (Exception)
            {
                DialogResult = DialogResult.Abort;
                Close();
            }
        }

        private void ExternalTool()
        {
            if (File.Exists(plugIn.FileName))
            {
                Hide();
                ProcessStartInfo start = new ProcessStartInfo {FileName = plugIn.FileName};
                var runningProcess = Process.Start(start);
                if (runningProcess != null) runningProcess.Exited += runningProcess_Exited;
            }
        }

        void runningProcess_Exited(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void ZipImport()
        {
            InstalledVersion version = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);

            // Configure open file dialog box 
            if (version != null)
            {
                string pluginFileName = Path.Combine(SpecialFolders.PlugInFolder(model), plugIn.FileName);

                CheckZipFileExists(pluginFileName);

                ExtractZipFile(version, pluginFileName);
            }
        }

        private void ExtractZipFile(InstalledVersion version, string pluginFileName)
        {
            if (File.Exists(pluginFileName))
            {
                ZipArchive zipArchive = ZipFile.OpenRead(pluginFileName);
                foreach (var file in plugIn.Files)
                {
                    if (File.Exists(Path.Combine(version.Folder, file.DestinationFolder, file.Name)))
                    {
                        if (!File.Exists(Path.Combine(version.Folder, file.DestinationFolder, "_" + file.Name)))
                        {
                            FileManager.SafeMove(Path.Combine(version.Folder, file.DestinationFolder, file.Name), Path.Combine(version.Folder, file.DestinationFolder, "_" + file.Name));
                        }
                    }
                    foreach (ZipArchiveEntry zippedFile in zipArchive.Entries)
                    {
                        if (zippedFile.FullName.Contains(file.SourceFolder + "/" + file.Name))
                        {
                            Unpacker.ExtractFile(zippedFile, Path.Combine(version.Folder, file.DestinationFolder, file.Name));
                        }
                    }
                }
            }
        }

        private void CheckZipFileExists(string pluginFileName)
        {
            if (!File.Exists(pluginFileName))
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Filter = plugIn.FileName + Resources.FormExecutePlugIn_ZipImport____ + plugIn.FileName + Resources.FormExecutePlugIn_ZipImport__;

                    // Show open file dialog box 
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        if (File.Exists(dlg.FileName))
                        {
                            FileManager.SafeCopy(dlg.FileName, Path.Combine(SpecialFolders.PlugInFolder(model), plugIn.FileName));
                        }
                    }
                }
            }
        }
    }
}
