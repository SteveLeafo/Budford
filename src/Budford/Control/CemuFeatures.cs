using Budford.Model;
using Budford.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Budford.Properties;
using Budford.View;
using Settings = Budford.Model.Settings;

namespace Budford.Control
{
    internal static class CemuFeatures
    {
        internal const string Cemu = "Cemu.exe";

        internal const string CemuUrl = "http://cemu.info/";

        // Lookup table for each of the released Cemu versions - will replace with a hash code one day
        static Dictionary<long, string> versionSizes;

        /// <summary>
        /// 
        /// </summary>
        internal static void SetVersionSizes()
        {
            if (versionSizes == null)
            {
                versionSizes = new Dictionary<long, string>
                {
                    {4451328, "1.0.0"},
                    {4468224, "1.0.1"},
                    {4478976, "1.0.2"},
                    {4483072, "1.1.0"},
                    {4487680, "1.1.1"},
                    {4569600, "1.1.2"},
                    {4594176, "1.2.0"},
                    {4614656, "1.3.0"},
                    {4647424, "1.3.1"},
                    {4703744, "1.3.3"},
                    {4817920, "1.4.0"},
                    {4835328, "1.4.1"},
                    {4863488, "1.4.2"},
                    {5638656, "1.5.0"},
                    {5673472, "1.5.1"},
                    {5730304, "1.5.2"},
                    {5748736, "1.5.3"},
                    {5771776, "1.5.4"},
                    {5799936, "1.5.5"},
                    {5807616, "1.5.6"},
                    {5820928, "1.6.0"},
                    {5834752, "1.6.1"},
                    {5844992, "1.6.2"},
                    {5855744, "1.6.3"},
                    {5876736, "1.6.4"},
                    {5919744, "1.7.0"},
                    {5936640, "1.7.1"},
                    {5989376, "1.7.2"},
                    {6013952, "1.7.3"},
                    {6298112, "1.7.4"},
                    {6310400, "1.7.5"},
                    {6339584, "1.8.0"},
                    {7080960, "1.8.0"},
                    {7181312, "1.8.1"},
                    {7965184, "1.8.2"},
                    {7905280, "1.9.0"},
                    {7933952, "1.9.1"},
                    {8014336, "1.10.0"}
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        internal static void UpdateFeaturesForInstalledVersions(Model.Model model)
        {
            foreach (var version in model.Settings.InstalledVersions)
            {
                version.HasFonts = HasFontsInstalled(version.Folder);
                version.HasOnlineFiles = HasOnlineFiles(version.Folder);
                version.HasCemuHook = HasCemuHookInstalled(version.Folder);
                version.HasPatch = HasPatchInstalled(version.Folder);
                version.HasDlc = HasDlcInstalled(version.Folder);
                version.HasControllerProfiles = HasControllerProfiles(version.Folder);
                if (version.HasDlc)
                {
                    version.DlcSource = JunctionPoint.GetTarget(Path.Combine(version.Folder, "mlc01", "usr", "title"));
                    if (JunctionPoint.Exists(Path.Combine(version.Folder, "mlc01", "usr", "title")))
                    {
                        if (Directory.Exists(version.DlcSource))
                        {
                            version.DlcType = 2;
                        }
                        else
                        {
                            version.DlcType = 3;
                        }

                    }
                    else
                    {
                        version.DlcType = 1;
                    }
                }
                else
                {
                    version.DlcType = 0;
                    version.DlcSource = "";
                }

                if (version.Version.StartsWith("cemu"))
                {
                    version.Version = version.Name.Replace("cemu_", "").Replace("a", "").Replace("b", "").Replace("c", "").Replace("d", "").Replace("e", "").Replace("f", "").Replace("g", "");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="form"></param>
        /// <param name="model"></param>
        internal static void RepairInstalledVersions(Form form, Model.Model model)
        {
            Unpacker unpacker = new Unpacker(form);

            PopulateBudfordDataBase(model);
            PopulateBudfordVersions(model);

            foreach (var v in model.Settings.InstalledVersions)
            {
                RepairPatch(model, v);

                RepairFonts(unpacker, v);

                InstallCemuHook(unpacker, v);

                RepairControllers(model, v);

                RepairOnlineFiles(model, v);

                RepairUpdateFolder(model, v);
            }

            UpdateFeaturesForInstalledVersions(model);
        }

        private static void RepairUpdateFolder(Model.Model model, InstalledVersion v)
        {
            InstalledVersion dlcSource = GetLatestDlcVersion(model);

            if (v.VersionNumber < 1110 || model.Settings.MlcFolder == "")
            {
                if (!v.HasDlc)
                {
                    if (dlcSource != null)
                    {
                        try
                        {
                            JunctionPoint.Create(Path.Combine(dlcSource.Folder, "mlc01", "usr", "title"), Path.Combine(v.Folder, "mlc01", "usr", "title"), true);
                        }
                        catch (Exception)
                        {
                            // No code
                        }
                    }
                }
            }
        }

        private static void RepairOnlineFiles(Model.Model model, InstalledVersion v)
        {
            InstalledVersion onlineSource = GetLatestOnlineVersion(model);

            if (!v.HasOnlineFiles)
            {
                if (onlineSource != null)
                {
                    CopyOnlineFiles(onlineSource, v);
                }
            }
        }

        private static void RepairControllers(Model.Model model, InstalledVersion v)
        {
            if (!v.HasControllerProfiles)
            {
                CopyLatestControllerProfiles(model, v);
            }
        }

        private static void RepairFonts(Unpacker unpacker, InstalledVersion v)
        {
            if (!v.HasFonts)
            {
                unpacker.Unpack("sharedFonts.zip", v.Folder);
            }
        }

        private static void RepairPatch(Model.Model model, InstalledVersion v)
        {
            InstalledVersion patchSource = GetLatestPatchVersion(model);

            if (!v.HasPatch)
            {
                if (File.Exists("sys.zip"))
                {
                    Unpacker.ExtractToDirectory("sys.zip", Path.Combine(v.Folder, "mlc01"), true);
                }
                else
                {
                    if (patchSource != null)
                    {
                        CopyPatchFiles(patchSource, v);
                    }
                }
            }
        }

        private static void PopulateBudfordDataBase(Model.Model model)
        {
            foreach (var data in WiiU.Dumps)
            {
                if (!data[0].Contains("*"))
                {
                    CopySingleFileToCemu(model, data);
                }
                else
                {
                    CopyFolderToCemu(model, data);
                }
            }
        }

        private static void CopySingleFileToCemu(Model.Model model, string[] data)
        {
            string destinationFile = Path.Combine(SpecialFolders.BudfordDir(model), data[1], data[0]);
            string destinationFolder = Path.Combine(SpecialFolders.BudfordDir(model), data[1]);
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }
            if (!File.Exists(destinationFile))
            {
                foreach (var v in model.Settings.InstalledVersions)
                {
                    string sourceFile = Path.Combine(v.Folder, data[2], data[0]);
                    if (File.Exists(sourceFile))
                    {
                        File.Copy(sourceFile, destinationFile);
                        break;
                    }
                }
            }
        }

        private static void CopyFolderToCemu(Model.Model model, string[] data)
        {
            string destinationFolder = Path.Combine(SpecialFolders.BudfordDir(model), data[1]);
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }
            foreach (var v in model.Settings.InstalledVersions)
            {
                string sourceFolder = Path.Combine(v.Folder, data[2]);
                if (Directory.Exists(sourceFolder))
                {
                    foreach (var file in Directory.EnumerateFiles(sourceFolder))
                    {
                        string destinationFile = Path.Combine(destinationFolder, Path.GetFileName(file));
                        if (!File.Exists(destinationFile))
                        {
                            File.Copy(file, destinationFile);
                        }
                    }
                }
            }
        }

        private static void PopulateBudfordVersions(Model.Model model)
        {
            foreach (var data in WiiU.Dumps)
            {
                int minVersion = Convert.ToInt32(data[4]);
                if (!data[0].Contains("*"))
                {
                    CopySingleFileToBudford(model, data, minVersion);
                }
                else
                {
                    CopyFolderToBudford(model, data, minVersion);
                }
            }
        }

        private static void CopyFolderToBudford(Model.Model model, string[] data, int minVersion)
        {
            string sourceFolder = Path.Combine(SpecialFolders.BudfordDir(model), data[1]);
            if (Directory.Exists(sourceFolder))
            {
                foreach (var v in model.Settings.InstalledVersions)
                {
                    if (v.VersionNumber >= minVersion)
                    {
                        string destinationFolder = Path.Combine(v.Folder, data[2]);
                        if (!Directory.Exists(destinationFolder))
                        {
                            Directory.CreateDirectory(destinationFolder);
                        }

                        foreach (var file in Directory.EnumerateFiles(sourceFolder))
                        {
                            string destinationFile = Path.Combine(destinationFolder, Path.GetFileName(file));
                            if (!File.Exists(destinationFile))
                            {
                                File.Copy(file, destinationFile);
                            }
                        }
                    }
                }
            }
        }

        private static void CopySingleFileToBudford(Model.Model model, string[] data, int minVersion)
        {
            string sourceFile = Path.Combine(SpecialFolders.BudfordDir(model), data[1], data[0]);
            if (File.Exists(sourceFile))
            {
                foreach (var v in model.Settings.InstalledVersions)
                {
                    if (v.VersionNumber >= minVersion)
                    {
                        string destinationFile = Path.Combine(v.Folder, data[2], data[0]);
                        string destinationFolder = Path.Combine(v.Folder, data[2]);
                        if (!Directory.Exists(destinationFolder))
                        {
                            Directory.CreateDirectory(destinationFolder);
                        }
                        if (!File.Exists(destinationFile))
                        {
                            File.Copy(sourceFile, destinationFile);
                        }
                    }
                }
            }
        }

        private static void CopyPatchFiles(InstalledVersion onlineSource, InstalledVersion onlineDestination)
        {
            Directory.CreateDirectory(Path.Combine(onlineSource.Folder, "mlc01", "sys", "title", "0005001b", "10056000", "content"));
            File.Copy(Path.Combine(onlineSource.Folder, "mlc01", "sys", "title", "0005001b", "10056000", "content", "FFLResHigh.dat"), Path.Combine(onlineDestination.Folder, "mlc01", "sys", "title", "0005001b", "10056000", "content", "FFLResHigh.dat"), true);
            File.Copy(Path.Combine(onlineSource.Folder, "mlc01", "sys", "title", "0005001b", "10056000", "content", "FFLResHighLG.dat"), Path.Combine(onlineDestination.Folder, "mlc01", "sys", "title", "0005001b", "10056000", "content", "FFLResHighLG.dat"), true);
            File.Copy(Path.Combine(onlineSource.Folder, "mlc01", "sys", "title", "0005001b", "10056000", "content", "FFLResMiddle.dat"), Path.Combine(onlineDestination.Folder, "mlc01", "sys", "title", "0005001b", "10056000", "content", "FFLResMiddle.dat"), true);
            File.Copy(Path.Combine(onlineSource.Folder, "mlc01", "sys", "title", "0005001b", "10056000", "content", "FFLResMiddleLG.dat"), Path.Combine(onlineDestination.Folder, "mlc01", "sys", "title", "0005001b", "10056000", "content", "FFLResMiddleLG.dat"), true);
        }

        private static void CopyOnlineFiles(InstalledVersion onlineSource, InstalledVersion onlineDestination)
        {
            if (onlineDestination.VersionNumber > 1110)
            {
                File.Copy(Path.Combine(onlineSource.Folder, "otp.bin"), Path.Combine(onlineDestination.Folder, "otp.bin"), true);
                File.Copy(Path.Combine(onlineSource.Folder, "seeprom.bin"), Path.Combine(onlineDestination.Folder, "seeprom.bin"), true);

                Directory.CreateDirectory(Path.Combine(onlineDestination.Folder, "mlc01", "usr", "save", "system", "act", "80000001"));
                File.Copy(Path.Combine(onlineSource.Folder, "mlc01", "usr", "save", "system", "act", "80000001", "account.dat"), Path.Combine(onlineDestination.Folder, "mlc01", "usr", "save", "system", "act", "80000001", "account.dat"), true);

                Directory.CreateDirectory(Path.Combine(onlineSource.Folder, "mlc01", "sys", "title", "0005001b", "10054000", "content", "ccerts"));
                DirectoryInfo s1 = new DirectoryInfo(Path.Combine(onlineSource.Folder, "mlc01", "sys", "title", "0005001b", "10054000", "content", "ccerts"));
                DirectoryInfo d1 = new DirectoryInfo(Path.Combine(onlineDestination.Folder, "mlc01", "sys", "title", "0005001b", "10054000", "content", "ccerts"));
                FileManager.CopyFilesRecursively(s1, d1);

                Directory.CreateDirectory(Path.Combine(onlineDestination.Folder, "mlc01", "sys", "title", "0005001b", "10054000", "content", "scerts"));
                DirectoryInfo s2 = new DirectoryInfo(Path.Combine(onlineSource.Folder, "mlc01", "sys", "title", "0005001b", "10054000", "content", "scerts"));
                DirectoryInfo d2 = new DirectoryInfo(Path.Combine(onlineDestination.Folder, "mlc01", "sys", "title", "0005001b", "10054000", "content", "scerts"));
                FileManager.CopyFilesRecursively(s2, d2);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        static InstalledVersion GetLatestDlcVersion(Model.Model model)
        {
            InstalledVersion lastestWithDlc = null;
            int latestVersion = 0;
            foreach (var v in model.Settings.InstalledVersions)
            {
                if (v.DlcType == 1)
                {
                    if (v.VersionNumber > latestVersion)
                    {
                        lastestWithDlc = v;
                        latestVersion = v.VersionNumber;
                    }
                }
            }
            return lastestWithDlc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        static InstalledVersion GetLatestOnlineVersion(Model.Model model)
        {
            InstalledVersion lastestWithDlc = null;
            int latestVersion = 0;
            foreach (var v in model.Settings.InstalledVersions)
            {
                if (v.HasOnlineFiles)
                {
                    if (v.VersionNumber > latestVersion)
                    {
                        lastestWithDlc = v;
                        latestVersion = v.VersionNumber;
                    }
                }
            }
            return lastestWithDlc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        static InstalledVersion GetLatestPatchVersion(Model.Model model)
        {
            InstalledVersion lastestWithDlc = null;
            int latestVersion = 0;
            foreach (var v in model.Settings.InstalledVersions)
            {
                if (v.HasPatch)
                {
                    if (v.VersionNumber > latestVersion)
                    {
                        lastestWithDlc = v;
                        latestVersion = v.VersionNumber;
                    }
                }
            }
            return lastestWithDlc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        internal static InstalledVersion GetLatestVersion(Model.Model model, int maxVersion = int.MaxValue)
        {
            InstalledVersion latest = null;
            int latestVersion = 0;
            foreach (var v in model.Settings.InstalledVersions)
            {
                int version = v.VersionNumber;

                if (v.VersionNumber >= latestVersion)
                {
                    if (v.VersionNumber < maxVersion)
                    {
                        if (version == latestVersion)
                        {
                            if (v.IsLatest)
                            {
                                latest = v;
                                latestVersion = v.VersionNumber;
                            }
                        }
                        else
                        {
                            latest = v;
                            latestVersion = version;
                        }
                    }
                }
            }
            return latest;
        }

        internal static bool DownloadLatestVersion(Form parent, Settings settings)
        {
            using (FormWebpageDownload dlc = new FormWebpageDownload(CemuUrl, "Latest Version"))
            {
                dlc.ShowDialog(parent);
                foreach (var line in dlc.Result.Split('\n'))
                {
                    if (line.Contains("name=\"download\""))
                    {
                        string[] toks = line.Split('=');
                        string ver = toks[1].Substring(1, toks[1].LastIndexOf('\"') - 1);
                        int currentVersion = InstalledVersion.GetVersionNumber(Path.GetFileName(ver));
                        if (!IsInstalled(currentVersion, settings))
                        {
                            return true;
                        }
                        else
                        {
                            MessageBox.Show(Resources.CemuFeatures_DownloadLatestVersion_The_latest_version_of_Cemu_is_already_installed_, Resources.CemuFeatures_DownloadLatestVersion_Information___);
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if requested version is installed
        /// </summary>
        /// <param name="versionNo"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        static bool IsInstalled(int versionNo, Settings settings)
        {
            foreach (var version in settings.InstalledVersions)
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
        /// <param name="folder"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        internal static bool IsCemuFolder(string folder, out string version)
        {
            version = "??";
            if (File.Exists(Path.Combine(folder, Cemu)))
            {
                FileInfo fi = new FileInfo(Path.Combine(folder, Cemu));
                if (versionSizes.ContainsKey(fi.Length))
                {
                    version = versionSizes[fi.Length];
                }
                return Directory.Exists(Path.Combine(folder, "mlc01"));
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        static bool HasCemuHookInstalled(string folder)
        {
            return File.Exists(Path.Combine(folder, "dbghelp.dll"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        static bool HasFontsInstalled(string folder)
        {
            if (Directory.Exists(Path.Combine(folder, "sharedFonts")))
            {
                if (File.Exists(Path.Combine(folder, "sharedFonts", "CafeCn.ttf")))
                {
                    if (File.Exists(Path.Combine(folder, "sharedFonts", "CafeKr.ttf")))
                    {
                        if (File.Exists(Path.Combine(folder, "sharedFonts", "CafeStd.ttf")))
                        {
                            if (File.Exists(Path.Combine(folder, "sharedFonts", "CafeTw.ttf")))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        static bool HasOnlineFiles(string folder)
        {
            if (File.Exists(Path.Combine(folder, "otp.bin")))
            {
                if (File.Exists(Path.Combine(folder, "seeprom.bin")))
                {
                    if (File.Exists(Path.Combine(folder, "mlc01", "usr", "save", "system", "act", "80000001", "account.dat")))
                    {                        
                        if (Directory.Exists(Path.Combine(folder, "mlc01", "sys", "title", "0005001b", "10054000", "content", "ccerts")))
                        {
                            if (Directory.Exists(Path.Combine(folder, "mlc01", "sys", "title", "0005001b", "10054000", "content", "scerts")))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        static bool HasPatchInstalled(string folder)
        {
            if (Directory.Exists(Path.Combine(folder, "mlc01", "sys", "title", "0005001b", "10056000", "content")))
            {
                if (File.Exists(Path.Combine(folder, "mlc01", "sys", "title", "0005001b", "10056000", "content", "FFLResHigh.dat")))
                {
                    if (File.Exists(Path.Combine(folder, "mlc01", "sys", "title", "0005001b", "10056000", "content", "FFLResHighLG.dat")))
                    {
                        if (File.Exists(Path.Combine(folder, "mlc01", "sys", "title", "0005001b", "10056000", "content", "FFLResMiddle.dat")))
                        {
                            if (File.Exists(Path.Combine(folder, "mlc01", "sys", "title", "0005001b", "10056000", "content", "FFLResMiddleLG.dat")))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        static bool HasControllerProfiles(string folder)
        {
            if (Directory.Exists(Path.Combine(folder, "controllerProfiles")))
            {
                return Directory.EnumerateFiles(Path.Combine(folder, "controllerProfiles")).Any();
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        static bool HasDlcInstalled(string folder)
        {
            if (Directory.Exists(Path.Combine(new[] { folder,  "mlc01","usr", "title" })))
            {
                string dest = JunctionPoint.GetTarget(Path.Combine(new[] { folder, "mlc01", "usr", "title" }));
                if (dest != null)
                {
                    if (!Directory.Exists(dest))
                    {
                        return true;
                    }
                }
                if (Directory.EnumerateDirectories(Path.Combine(new[] { folder,  "mlc01", "usr", "title" })).Any())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void CopyLatestControllerProfiles(Model.Model model, InstalledVersion installedVersion)
        {
            int latestVersionWithProfiles = -1;
            InstalledVersion versionWithControllerProfiles = null;

            foreach (var v in model.Settings.InstalledVersions)
            {
                if (v.HasControllerProfiles)
                {
                    if (v.VersionNumber > latestVersionWithProfiles)
                    {
                        latestVersionWithProfiles = v.VersionNumber;
                        versionWithControllerProfiles = v;
                    }
                }
            }

            if (versionWithControllerProfiles != null)
            {
                FileManager.CopyFilesRecursively(
                    new DirectoryInfo(Path.Combine(versionWithControllerProfiles.Folder, "controllerProfiles")),
                    new DirectoryInfo(Path.Combine(installedVersion.Folder, "controllerProfiles")));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unpacker"></param>
        /// <param name="v"></param>
        internal static void InstallCemuHook(Unpacker unpacker, InstalledVersion v)
        {
            if (v.VersionNumber >= 181)
            {
                if (File.Exists("cemu_hook.zip"))
                {
                    unpacker.Unpack("cemu_hook.zip", v.Folder);
                }
            }
            else if (v.VersionNumber >= 173)
            {
                if (File.Exists("OldCemuHook.zip"))
                {
                    unpacker.Unpack("OldCemuHook.zip", v.Folder);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="model"></param>
        internal static void DownloadCompatibilityStatus(Form parent, Model.Model model)
        {
            using (FormWebpageDownload dlc = new FormWebpageDownload("http://compat.cemu.info/", "Game Status"))
            {
                dlc.ShowDialog(parent);

                List<GameInformation> currentGames = null;
                string url = "";

                foreach (var line in dlc.Result.Split('\n'))
                {
                    if (line.Contains("<td class=\"title\">"))
                    {
                        string name = line.Substring(line.LastIndexOf("\">", StringComparison.Ordinal) + 2, line.LastIndexOf("/a", StringComparison.Ordinal) - line.LastIndexOf("\">", StringComparison.Ordinal) - 3).Replace("&amp;", "&");
                        url = line.Substring(line.IndexOf("http", StringComparison.Ordinal), (line.LastIndexOf("\">", StringComparison.Ordinal) - line.IndexOf("http", StringComparison.Ordinal))).Replace("&amp;", "&");
                        currentGames = Persistence.GetGames(model, name);
                    }
                    if (line.Contains("<td class=\"rating\">"))
                    {
                        string rating = line.Substring(line.LastIndexOf("title=", StringComparison.Ordinal) + 7, line.LastIndexOf("\"", StringComparison.Ordinal) - line.LastIndexOf("title=", StringComparison.Ordinal) - 7);
                        if (currentGames != null)
                        {
                            SetGameStatus(currentGames, rating, url);
                            currentGames.Clear();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentGames"></param>
        /// <param name="rating"></param>
        /// <param name="url"></param>
        private static void SetGameStatus(List<GameInformation> currentGames, string rating, string url)
        {
            GameSettings.EmulationStateType state;

            switch (rating)
            {
                case "Playable":
                    state = GameSettings.EmulationStateType.Playable;
                    break;
                case "Perfect":
                    state = GameSettings.EmulationStateType.Perfect;
                    break;
                case "Loads":
                    state = GameSettings.EmulationStateType.Loads;
                    break;
                case "Runs":
                    state = GameSettings.EmulationStateType.Runs;
                    break;
                case "Unplayable":
                    state = GameSettings.EmulationStateType.Unplayable;
                    break;
                default:
                    state = GameSettings.EmulationStateType.NotSet;
                    break;
            }
            foreach (var currentGame in currentGames)
            {
                currentGame.GameSetting.OfficialEmulationState = state;
                currentGame.GameSetting.CompatibilityUrl = url;
            }
        }

        internal static void DownloadLatestGraphicPack(Form parent, string jsonString, bool showMessage = true)
        {
            // For that you will need to add reference to System.Runtime.Serialization
            var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(jsonString.ToCharArray()), new System.Xml.XmlDictionaryReaderQuotas());

            // For that you will need to add reference to System.Xml and System.Xml.Linq
            var root = XElement.Load(jsonReader);
            var xElement = root.Elements("assets").First().Elements().First().Element("browser_download_url");
            if (xElement != null)
            {
                string uri = xElement.Value;

                string packName = Path.GetFileNameWithoutExtension(uri);

                if (!IsGraphicPackInstalled(packName))
                {
                    try
                    {
                        if (File.Exists("tempGraphicPack.zip"))
                        {
                            File.Delete("tempGraphicPack.zip");
                        }
                        var unpacker = new Unpacker(parent);
                        unpacker.DownloadAndUnpack("tempGraphicPack.zip", uri, Path.Combine("graphicsPacks", packName), "Graphic Pack");
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    if (showMessage)
                    {
                        MessageBox.Show(Resources.CemuFeatures_DownloadLatestGraphicPack_Latest_version_is_already_installed);
                    }
                }
            }
        }

        internal static bool IsGraphicPackInstalled(string pack)
        {
            if (!Directory.Exists("graphicsPacks"))
            {
                Directory.CreateDirectory("graphicsPacks");
            }
            foreach (var dir in Directory.EnumerateDirectories("graphicsPacks"))
            {
                string folder = Path.GetFileName(dir);
                if (folder != null && folder.StartsWith("graphicPacks_2-"))
                {
                    if (pack == folder)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static void DownloadLatestGraphicsPack(Form form, Model.Model modelIn, bool showMessage = true)
        {
            try
            {
                using (FormWebpageDownload dlc = new FormWebpageDownload("https://api.github.com/repos/slashiee/cemu_graphic_packs/releases/latest", "Latest Graphic Pack"))
                {
                    if (dlc.ShowDialog(form) == DialogResult.OK)
                    {
                        DownloadLatestGraphicPack(form, dlc.Result, showMessage);
                        string pack = "";
                        foreach (var dir in Directory.EnumerateDirectories("graphicsPacks"))
                        {
                            string folder = dir.Replace("graphicsPacks" + Path.DirectorySeparatorChar, "");
                            if (folder.StartsWith("graphicPacks_2-"))
                            {
                                pack = folder.Replace("graphicPacks_2-", "");
                            }
                        }

                        if (pack != "")
                        {
                            modelIn.Settings.GraphicsPackRevision = pack;
                            FolderScanner.FindGraphicsPacks(new DirectoryInfo(Path.Combine("graphicsPacks", "graphicPacks_2-" + modelIn.Settings.GraphicsPackRevision)), modelIn.GraphicsPacks);
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show(Resources.FormMainWindow_DownloadLatestGraphicsPack_Unable_to_download_graphic_packs_at_this_time___Try_again_later_or_upgrate_to_latest_version_of_Budford);
            }
        }
    }
}
