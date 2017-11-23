using Budford.Model;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Budford.Properties;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Budford.Control
{
    internal class FileManager
    {
        readonly Model.Model model;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelIn"></param>
        internal FileManager(Model.Model modelIn)
        {
            model = modelIn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="ten80p"></param>
        internal static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target, bool ten80p = false, bool overrideit = false)
        {
            if (source.Exists)
            {
                if (!target.Exists)
                {
                    target.Create();
                }
                foreach (DirectoryInfo dir in source.GetDirectories())
                {
                    if (ten80p)
                    {
                        if (dir.Name.EndsWith("1080p"))
                        {
                            CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name), true);
                        }
                    }
                    else
                    {
                        CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
                    }
                }
                foreach (FileInfo file in source.GetFiles())
                {
                    if (!File.Exists(Path.Combine(target.FullName, file.Name)))
                    {
                        file.CopyTo(Path.Combine(target.FullName, file.Name));
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
        /// <param name="user"></param>
        internal void LoadUserSaves(User user)
        {
            if (Directory.Exists("Cemu\\cemu_" + model.Settings.CurrentCemuVersion + "\\mlc01\\emulatorSave"))
            {
                Directory.Delete("Cemu\\cemu_" + model.Settings.CurrentCemuVersion + "\\mlc01\\emulatorSave");
            }
            try
            {
                Directory.Move("Users\\" + user.Name + "\\SaveFiles\\emulatorSave", "Cemu\\cemu_" + model.Settings.CurrentCemuVersion + "\\mlc01\\emulatorSave");
            }
            catch (Exception ex)
            {
                model.Errors.Add(ex.Message);
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
            foreach (var s in Persistence.Load().AllSaveDirs)
            {
                if (s.SaveDirId == id)
                {
                    if (MessageBox.Show(Resources.FileManager_ImportShaderCache_About_to_import_shader_cache_for_ + s.GameName + Resources.FileManager_ImportShaderCache_, Resources.FileManager_ImportShaderCache_Continue, MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) == DialogResult.OK)
                    {
                        if (!File.Exists("Cemu\\cemu_" + model.Settings.CurrentCemuVersion + "\\shaderCache\\transferable"))
                        {
                            File.Copy(fileName, "Cemu\\cemu_" + model.Settings.CurrentCemuVersion + "\\shaderCache\\transferable\\" + Path.GetFileName(fileName), true);
                        }
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        internal void SaveUserSaves(User user)
        {
            try
            {
                if (Directory.Exists("Users\\" + user.Name + "\\SaveFiles\\emulatorSave"))
                {
                    Directory.Delete("Users\\" + user.Name + "\\SaveFiles\\emulatorSave");
                }
                Directory.Move("Cemu\\cemu_" + model.Settings.CurrentCemuVersion + "\\mlc01\\emulatorSave", "Users\\" + user.Name + "\\SaveFiles\\emulatorSave");
            }
            catch (Exception ex)
            {
                model.Errors.Add(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void InitialiseFolderStructure(Model.Model modelIn)
        {
            try
            {
                foreach (var user in modelIn.Users)
                {
                    if (!Directory.Exists("Users\\" + user.Name + "\\SaveFiles")) Directory.CreateDirectory("Users\\" + user.Name + "\\SaveFiles");
                    if (!Directory.Exists("Users\\" + user.Name + "\\SaveFiles\\emulatorSave")) Directory.CreateDirectory("Users\\" + user.Name + "\\SaveFiles\\emulatorSave");
                }
            }
            catch (Exception ex)
            {
                model.Errors.Add(ex.Message);
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

            iv.Name = folder.Replace(model.Settings.DefaultInstallFolder + "\\", "");
            iv.Folder = folder;

            if (version == "??")
            {
                string name = iv.Name.TrimStart('_').ToLower(); ;
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
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            if (dInfo.Exists)
            {
                dInfo.SetAccessControl(dSecurity);
            }
        }

        internal static void DownloadCemu(Form parent, Unpacker unpacker, Model.Model model, string[] uris, string[] filenames)
        {
            using (FormMultiFileDownload dl = new FormMultiFileDownload(uris, filenames))
            {
                dl.ShowDialog(parent);
            }
            unpacker.Unpack(Path.GetFileName(uris[0]), model.Settings.DefaultInstallFolder);
            unpacker.Unpack("cemu_hook.zip", model.Settings.DefaultInstallFolder + "\\cemu_" + model.Settings.CurrentCemuVersion + "");
            unpacker.Unpack("sharedFonts.zip", model.Settings.DefaultInstallFolder + "\\cemu_" + model.Settings.CurrentCemuVersion + "");
            unpacker.Unpack("shaderCache.zip", model.Settings.DefaultInstallFolder + "\\cemu_" + model.Settings.CurrentCemuVersion + "");
            unpacker.Unpack("controllerProfiles.zip", model.Settings.DefaultInstallFolder + "\\cemu_" + model.Settings.CurrentCemuVersion + "");

            unpacker.ExtractToDirectory("sys.zip", model.Settings.DefaultInstallFolder + "\\cemu_" + model.Settings.CurrentCemuVersion + "\\mlc01\\", true);
            unpacker.ExtractToDirectory("graphicsPack.zip", "graphicsPacks", true);

            if (Directory.Exists("graphicsPacks"))
            {
                FolderScanner.FindGraphicsPacks(new DirectoryInfo("graphicsPacks\\graphicsPacks"), model.GraphicsPacks);
            }
            FolderScanner.AddGraphicsPacksToGames(model);
            CemuFeatures.UpdateFeaturesForInstalledVersions(model);
        }
    }   
}
