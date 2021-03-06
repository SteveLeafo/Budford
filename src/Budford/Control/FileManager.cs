﻿using Budford.Model;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Security.AccessControl;
using System.Security.Principal;
using Budford.Properties;
using Budford.View;
using Budford.Utilities;
using System.Diagnostics;

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
                FileManager.SafeCreateDirectory(target.FullName);
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
                CopyFilesToFolder(source, target, overrideit);
            }
        }

        private static void CopyFilesToFolder(DirectoryInfo source, DirectoryInfo target, bool overrideit)
        {
            foreach (FileInfo file in source.GetFiles())
            {
                if (!File.Exists(Path.Combine(target.FullName, file.Name)))
                {
                    CopyFiles(target, file);
                }
                else
                {
                    CopyFilesOverrideExisting(target, overrideit, file);
                }
            }
        }

        private static void CopyFilesOverrideExisting(DirectoryInfo target, bool overrideit, FileInfo file)
        {
            if (overrideit)
            {
                if (!Path.GetFileName(file.Name).Contains("Budford"))
                {
                    FileInfo dest = new FileInfo(target.FullName);
                    if (file.LastWriteTime > dest.LastWriteTime)
                    {
                        SafeCopy(file.FullName, Path.Combine(target.FullName, file.Name), true);
                    }
                }
            }
        }

        private static void CopyFiles(DirectoryInfo target, FileInfo file)
        {
            try
            {
                FileManager.SafeCreateDirectory(target.FullName);
                if (!Path.GetFileName(file.Name).Contains("Budford"))
                {
                    SafeCopy(file.FullName, Path.Combine(target.FullName, file.Name));
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="model"></param>
        /// <param name="fileName"></param>
        internal static void ImportShaderCache(Form parent, Model.Model model, string fileName)
        {
            string id = Path.GetFileNameWithoutExtension(fileName);
            if (id != null)
            {
                id = id.Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").Replace("(4)", "").Replace(" - Copy", "");

                string budfordFolder = Path.Combine(model.Settings.SavesFolder, "Budford", id);

                budfordFolder = CheckGameExists(parent, model, budfordFolder);

                if (Directory.Exists(budfordFolder))
                {
                    bool copy;
                    string destination;
                    CheckShouldImport(fileName, budfordFolder, out copy, out destination);

                    if (copy)
                    {
                        string message = "Shader cache import succesfull";
                        if (File.Exists(destination))
                        {
                            FileCache srcCache = FileCache.fileCache_openExisting(fileName, 1);
                            if (srcCache == null)
                            {
                                MessageBox.Show(Resources.FileManager_ImportShaderCache_, Resources.FileManager_ImportShaderCache_Invalid_Shader_Cache);
                                return;
                            }
                            FileCache destCache = FileCache.fileCache_openExisting(destination, 1);
                            message = message + "\r\n\r\nExisting cache: " + destCache.FileTableEntryCount + " shaders.\r\nNew Cache: " + srcCache.FileTableEntryCount + " shaders.";
                        }
                        SafeCopy(fileName, destination, true);
                        MessageBox.Show(message, Resources.FileManager_ImportShaderCache_Imported_OK);
                    }
                }
            }
        }

        private static void CheckShouldImport(string fileName, string budfordFolder, out bool copy, out string destination)
        {
            copy = false;
            destination = Path.Combine(budfordFolder, "post_180.bin");
            if (!File.Exists(destination))
            {
                copy = true;
            }
            else
            {
                FileInfo srcInfo = new FileInfo(fileName);
                FileInfo destInfo = new FileInfo(destination);

                if (srcInfo.Length > destInfo.Length)
                {
                    copy = true;
                }
                else
                {
                    if (MessageBox.Show(Resources.FileManager_ImportShaderCache_The_shader_cache_file_is_smaller_than_the_current_file__are_you_sure_want_to_over_ride_it_, Resources.FileManager_ImportShaderCache_Are_you_sure, MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        copy = true;
                    }
                }
            }
        }

        private static string CheckGameExists(Form parent, Model.Model model, string budfordFolder)
        {
            if (!Directory.Exists(budfordFolder))
            {
                using (FormSelectGameForShaderImport selectGame = new FormSelectGameForShaderImport(model))
                {
                    if (selectGame.ShowDialog(parent) == DialogResult.OK)
                    {
                        budfordFolder = Path.Combine(model.Settings.SavesFolder, "Budford", selectGame.Id);
                    }
                }
            }
            return budfordFolder;
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

        internal static void DownloadCemu(Form parent, Model.Model model)
        {
            using (FormMultiFileDownload dl = new FormMultiFileDownload(model))
            {
                dl.ShowDialog(parent);
            }

            SearchForInstalledVersions(model);

            if (Directory.Exists("graphicsPacks"))
            {
                FolderScanner.FindGraphicsPacks(new DirectoryInfo(Path.Combine("graphicsPacks", "graphicsPacks")), model.GraphicsPacks);
            }
            FolderScanner.AddGraphicsPacksToGames(model);
            CemuFeatures.UpdateFeaturesForInstalledVersions(model);
        }

        internal static bool ClearFolder(string folderName)
        {
            try
            {
                var di = new DirectoryInfo(folderName);

                foreach (FileInfo file in di.GetFiles())
                {
                    SafeDelete(file.FullName); 
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

        internal static void DeleteShaderCache(InstalledVersion version)
        {
            DirectoryInfo di1 = new DirectoryInfo(Path.Combine(version.Folder, "shaderCache", "transferable"));
            foreach (FileInfo file in di1.GetFiles())
            {
                SafeDelete(file.FullName);
            }
            DirectoryInfo di2 = new DirectoryInfo(Path.Combine(version.Folder, "shaderCache", "precompiled"));
            foreach (FileInfo file in di2.GetFiles())
            {
                SafeDelete(file.FullName);
            }
        }

        internal static bool SafeCopy(string sourceFileName, string destinationFileName, bool overwrite = false)
        {
            try
            {
                Logger.Log("Copying " + sourceFileName + " to " + destinationFileName);
                if (File.Exists(sourceFileName))
                {
                    if (overwrite || !File.Exists(destinationFileName))
                    {
                        SafeCreateDirectory(Path.GetDirectoryName(destinationFileName));
                        File.Copy(sourceFileName, destinationFileName, overwrite);
                    }
                    return true;
                }
            }
            catch (Exception)
            {
                // No code
            }
            return false;
        }

        internal static bool SafeDelete(string fileName)
        {
            try
            {
                Logger.Log("Deleting " + fileName);
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    return true;
                }
            }
            catch (Exception)
            {
                // No code
            }
            return false;
        }


        internal static bool SafeMove(string originalFileName, string newFileName)
        {
            try
            {
                Logger.Log("Moving " + originalFileName + " to " + newFileName);
                if (File.Exists(originalFileName))
                {
                    File.Move(originalFileName, newFileName);
                    return true;
                }
            }
            catch (Exception)
            {
                // No code
            }
            return false;
        }

        internal static bool SafeCreateDirectory(string newDirectory)
        {
            if (newDirectory == null)
            {
                return false;
            }
            try
            {
                Logger.Log("Creating new directory: " + newDirectory);
                if (!Directory.Exists(newDirectory))
                {
                    DirectorySecurity ds = new DirectorySecurity();
                    ds.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
                    Directory.CreateDirectory(newDirectory, ds);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        internal static bool SafeDeleteDirectory(string newDirectory, bool recurse)
        {
            if (newDirectory == null)
            {
                return false;
            }
            try
            {
                Logger.Log("Deleting directory: " + newDirectory);
                if (Directory.Exists(newDirectory))
                {
                    Directory.Delete(newDirectory, true);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        internal static void OpenSaveFileLocation(Model.Model model, InstalledVersion version)
        {
           string folder = SpecialFolders.CurrentUserSaveDirCemu(model, version, model.GameData[model.CurrentId]);
           FileManager.SafeCreateDirectory(folder);
           Process.Start(folder);
        }

        internal static void OpenShaderCacheFolder(Model.Model model, InstalledVersion version)
        {
            if (Directory.Exists(Path.Combine(version.Folder, "shaderCache", "transferable")))
            {
                if (File.Exists(Path.Combine(version.Folder, "shaderCache", "transferable", model.GameData[model.CurrentId].SaveDir + ".bin")))
                {
                    Process.Start("explorer.exe", "/select, " + Path.Combine(version.Folder, "shaderCache", "transferable", model.GameData[model.CurrentId].SaveDir + ".bin"));
                }
                else
                {
                    Process.Start("explorer.exe", "/select, " + Path.Combine(version.Folder, "shaderCache", "transferable"));
                }
            }
            else
            {
                MessageBox.Show(Resources.fMainWindow_openSaveFileLocationToolStripMenuItem_Click_, Resources.fMainWindow_openSaveFileLocationToolStripMenuItem_Click_Folder_not_found, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }   
}
