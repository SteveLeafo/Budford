using Budford.Model;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Budford.Properties;
using System.Security.AccessControl;
using System.Security.Principal;
using Budford.View;
using Budford.Utilities;

namespace Budford.Control
{
    internal static class FileManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="ten80P"></param>
        /// <param name="overrideit"></param>
        internal static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target, bool ten80P = false, bool overrideit = false)
        {
            if (Directory.Exists(source.FullName))
            {
                if (!Directory.Exists(target.FullName))
                {
                    Directory.CreateDirectory(target.FullName);
                }
                foreach (DirectoryInfo dir in source.GetDirectories())
                {
                    if (ten80P)
                    {
                        if (dir.Name.EndsWith("1080p"))
                        {
                            CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name), true, overrideit);
                        }
                    }
                    else
                    {
                        CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name), false, overrideit);
                    }
                }
                foreach (FileInfo file in source.GetFiles())
                {
                    if (!File.Exists(Path.Combine(target.FullName, file.Name)))
                    {
                        try
                        {
                            if (!Directory.Exists(target.FullName))
                            {
                                Directory.CreateDirectory(target.FullName);
                            }
                            file.CopyTo(Path.Combine(target.FullName, file.Name));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    else
                    {
                        if (overrideit)
                        {
                            FileInfo dest = new FileInfo(target.FullName);
                            if (file.LastWriteTime > dest.LastWriteTime)
                            {
                                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fileName"></param>
        internal static void ImportShaderCache(Model.Model model, string fileName)
        {
            string id = Path.GetFileNameWithoutExtension(fileName);
            id = id.Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").Replace("(4)", "").Replace(" - Copy", "");

            string budfordFolder =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Budford", id);

            if (Directory.Exists(budfordFolder))
            {
                bool copy = false;
                string destination = Path.Combine(budfordFolder, "post_180.bin");
                if (!File.Exists(destination))
                {
                    copy = true;
                }
                else
                {
                    FileInfo srcInfo = new FileInfo(fileName);
                    FileInfo DestInfo = new FileInfo(destination);

                    if (srcInfo.Length > DestInfo.Length)
                    {
                        copy = true;
                    }
                    else
                    {
                        if (MessageBox.Show("The shader cache file is smaller than the current file, are you sure want to over ride it?", "Are you sure", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            copy = true;
                        }
                    }
                }
                if (copy)
                {
                    string message = "Shader cache import succesfull";
                    if (File.Exists(destination))
                    {
                        FileCache srcCache = FileCache.fileCache_openExisting(fileName, 1);
                        if (srcCache == null)
                        {
                            MessageBox.Show("The file doesn't appear to be a valid shader cache.\r\nPlease check the file and try again.", "Invalid Shader Cache");
                            return;
                        }
                        FileCache destCache = FileCache.fileCache_openExisting(destination, 1);
                        message = message + "\r\n\r\nExisting cache: " + destCache.FileTableEntryCount + " shaders.\r\nNew Cache: " + srcCache.FileTableEntryCount + " shaders.";
                    }
                    File.Copy(fileName, destination, true);
                    MessageBox.Show(message, "Imported OK");
                }
            }
            else
            {
                MessageBox.Show("Could not find game with ID: " + id + ".\r\nPlease check the filename and try again.", "Game not found");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        internal static void SearchForInstalledVersions(Model.Model model)
        {
            CemuFeatures.SetVersionSizes();

            if (Directory.Exists(model.Settings.DefaultInstallFolder))
            {
                foreach (var folder in Directory.EnumerateDirectories(model.Settings.DefaultInstallFolder))
                {
                    string version;
                    if (CemuFeatures.IsCemuFolder(folder, out version))
                    {
                        AddInstalledVersion(model, folder, version);
                    }
                }
            }
            else
            {
                MessageBox.Show(Resources.FileManager_SearchForInstalledVersions_Folder_doesn_t_exist__ + model.Settings.DefaultInstallFolder);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="folder"></param>
        /// <param name="version"></param>
        internal static void AddInstalledVersion(Model.Model model, string folder, string version)
        {
            InstalledVersion iv = model.Settings.InstalledVersions.FirstOrDefault(v => v.Folder == folder);

            if (iv == null)
            {
                iv = new InstalledVersion();
                var latest = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
                if (latest == null)
                {
                    iv.IsLatest = true;
                }
                model.Settings.InstalledVersions.Add(iv);
            }

            iv.Name = folder.Replace(model.Settings.DefaultInstallFolder + Path.DirectorySeparatorChar, "");
            iv.Folder = folder;

            if (version == "??" || version == "")
            {
                string name = iv.Name.TrimStart('_').ToLower();

                if (name.StartsWith("cemu_"))
                {
                    iv.Version = name.Replace("cemu_", "").Replace("a", "").Replace("b", "").Replace("c", "").Replace("d", "").Replace("e", "").Replace("f", "").Replace("g", "");
                }
                else if (name.StartsWith("cemu"))
                {
                    if (iv.Name.Contains("_"))
                    {
                        iv.Version = iv.Name.Split('_')[0].Replace("cemu", "");
                    }
                    else
                    {
                        iv.Version = name.Replace("cemu", "");
                        iv.Version = iv.Version.Replace("a", "").Replace("b", "").Replace("c", "").Replace("d", "").Replace("e", "").Replace("f", "").Replace("g", "");
                    }
                }
            }
            else
            {
                iv.Version = version;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPath"></param>
        internal static void GrantAccess(string fullPath)
        {
            try
            {
                DirectoryInfo dInfo = new DirectoryInfo(fullPath);
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                if (Directory.Exists(dInfo.FullName))
                {
                    dInfo.SetAccessControl(dSecurity);
                }
            }
            catch (Exception)
            {
                // Linux hates this
            }
        }

        internal static void DownloadCemu(Form parent, Unpacker unpacker, Model.Model model)
        {
            using (FormMultiFileDownload dl = new FormMultiFileDownload(model))
            {
                dl.ShowDialog(parent);
            }
            
            if (Directory.Exists("graphicsPacks"))
            {
                FolderScanner.FindGraphicsPacks(new DirectoryInfo(Path.Combine("graphicsPacks", "graphicsPacks")), model.GraphicsPacks);
            }
            FolderScanner.AddGraphicsPacksToGames(model);
            CemuFeatures.UpdateFeaturesForInstalledVersions(model);
        }

        static internal bool ClearFolder(string folderName)
        {
            try
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(folderName);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        static internal void DeleteShaderCache(InstalledVersion version)
        {
            DirectoryInfo di1 = new DirectoryInfo(Path.Combine(version.Folder, "shaderCache", "transferable"));
            foreach (FileInfo file in di1.GetFiles())
            {
                file.Delete();
            }
            DirectoryInfo di2 = new DirectoryInfo(Path.Combine(version.Folder, "shaderCache", "precompiled"));
            foreach (FileInfo file in di2.GetFiles())
            {
                file.Delete();
            }
        }

    }   
}
