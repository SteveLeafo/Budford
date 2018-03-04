using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using Budford.Control;
using Budford.Model;

namespace Budford.View
{
    public partial class FormExecutePlugIn : Form
    {
        Model.PlugIns.PlugIn plugIn;
        Model.Model model;

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
                        break;
                }
                DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception)
            {
                DialogResult = System.Windows.Forms.DialogResult.Abort;
            }
            Close();
        }

        private void ZipImport()
        {
            InstalledVersion version = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);

            // Configure open file dialog box 
            if (version != null)
            {
                string pluginFileName = Path.Combine(SpecialFolders.PlugInFolder(model), plugIn.FileName);

                if (!File.Exists(pluginFileName))
                {
                    using (OpenFileDialog dlg = new OpenFileDialog())
                    {
                        dlg.Filter = plugIn.FileName + " | " + plugIn.FileName + ";";

                        // Show open file dialog box 
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            if (File.Exists(dlg.FileName))
                            {
                                File.Copy(dlg.FileName, Path.Combine(SpecialFolders.PlugInFolder(model), plugIn.FileName));
                            }
                        }
                    }
                }

                if (File.Exists(pluginFileName))
                {
                    ZipArchive zipArchive = ZipFile.OpenRead(pluginFileName);
                    foreach (var file in plugIn.Files)
                    {
                        if (File.Exists(Path.Combine(version.Folder, file.DestinationFolder, file.Name)))
                        {
                            if (!File.Exists(Path.Combine(version.Folder, file.DestinationFolder, "_" + file.Name)))
                            {
                                File.Move(Path.Combine(version.Folder, file.DestinationFolder, file.Name), Path.Combine(version.Folder, file.DestinationFolder, "_" + file.Name));
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
        }
    }
}
