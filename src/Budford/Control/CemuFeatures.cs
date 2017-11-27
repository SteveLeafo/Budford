using Budford.Model;
using Budford.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Budford.Control
{
    static class CemuFeatures
    {
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
                version.HasCemuHook = HasCemuHookInstalled(version.Folder);
                version.HasPatch = HasPatchInstalled(version.Folder);
                version.HasDlc = HasDlcInstalled(version.Folder);
                version.HasControllerProfiles = HasControllerProfiles(version.Folder);
                if (version.HasDlc)
                {
                    version.DlcSource = JunctionPoint.GetTarget(version.Folder + @"\mlc01\usr\title");
                    if (JunctionPoint.Exists(version.Folder + @"\mlc01\usr\title"))
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
            InstalledVersion dlcSource = GetLatestDlcVersion(model);
            foreach (var v in model.Settings.InstalledVersions)
            {
                if (!v.HasPatch) unpacker.ExtractToDirectory("sys.zip", v.Folder + "\\mlc01\\", true);
                if (!v.HasFonts) unpacker.Unpack("sharedFonts.zip", v.Folder);
                //if (!v.HasCemuHook) 
                    InstallCemuHook(unpacker, v);
                if (!v.HasControllerProfiles) CopyLatestControllerProfiles(model, v);
                if (!v.HasDlc)
                {
                    if (dlcSource != null)
                    {
                        JunctionPoint.Create(dlcSource.Folder + @"\mlc01\usr\title", v.Folder + @"\mlc01\usr\title", true);
                    }
                }
            }

            UpdateFeaturesForInstalledVersions(model);
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
                    int version;
                    if (int.TryParse(v.Version.Replace(".", ""), out version))
                    {
                        if (version > latestVersion)
                        {
                            lastestWithDlc = v;
                            latestVersion = version;
                        }
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
        internal static InstalledVersion GetLatestVersion(Model.Model model, int maxVersion = int.MaxValue)
        {
            InstalledVersion latest = null;
            int latestVersion = 0;
            foreach (var v in model.Settings.InstalledVersions)
            {
                int version;
                if (int.TryParse(v.Version.Replace(".", ""), out version))
                {
                    if (version > latestVersion)
                    {
                        if (version < maxVersion)
                        {
                            latest = v;
                            latestVersion = version;
                        }
                    }
                }
            }
            return latest;
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
            if (File.Exists(folder + "\\cemu.exe"))
            {
                FileInfo fi = new FileInfo(folder + "\\cemu.exe");
                if (versionSizes.ContainsKey(fi.Length))
                {
                    version = versionSizes[fi.Length];
                }
                return Directory.Exists(folder + "\\mlc01");
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
            return File.Exists(folder + "\\dbghelp.dll");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        static bool HasFontsInstalled(string folder)
        {
            if (Directory.Exists(folder + "\\sharedFonts"))
            {
                if (File.Exists(folder + "\\sharedFonts\\CafeCn.ttf"))
                {
                    if (File.Exists(folder + "\\sharedFonts\\CafeKr.ttf"))
                    {
                        if (File.Exists(folder + "\\sharedFonts\\CafeStd.ttf"))
                        {
                            if (File.Exists(folder + "\\sharedFonts\\CafeTw.ttf"))
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
            if (Directory.Exists(folder + "\\mlc01\\sys\\title\\0005001b\\10056000\\content"))
            {
                if (File.Exists(folder + "\\mlc01\\sys\\title\\0005001b\\10056000\\content\\FFLResHigh.dat"))
                {
                    if (File.Exists(folder + "\\mlc01\\sys\\title\\0005001b\\10056000\\content\\FFLResHighLG.dat"))
                    {
                        if (File.Exists(folder + "\\mlc01\\sys\\title\\0005001b\\10056000\\content\\FFLResMiddle.dat"))
                        {
                            if (File.Exists(folder + "\\mlc01\\sys\\title\\0005001b\\10056000\\content\\FFLResMiddleLG.dat"))
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
            if (Directory.Exists(folder + "\\controllerProfiles"))
            {
                return Directory.EnumerateFiles(folder + "\\controllerProfiles").Any();
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
            if (Directory.Exists(folder + @"\mlc01\usr\title"))
            {
                string dest = JunctionPoint.GetTarget(folder + @"\mlc01\usr\title");
                if (dest != null)
                {
                    if (!Directory.Exists(dest))
                    {
                        return true;
                    }
                }
                if (Directory.EnumerateDirectories(folder + @"\mlc01\usr\title").Any())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
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
                    new DirectoryInfo(versionWithControllerProfiles.Folder + "\\controllerProfiles"),
                    new DirectoryInfo(installedVersion.Folder + "\\controllerProfiles"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unpacker"></param>
        /// <param name="v"></param>
        internal static void InstallCemuHook(Unpacker unpacker, InstalledVersion v)
        {
            int version;
            if (int.TryParse(v.Version.Replace(".", ""), out version))
            {
                if (version >= 181)
                {
                    if (File.Exists("cemu_hook.zip"))
                    {
                        unpacker.Unpack("cemu_hook.zip", v.Folder);
                    }
                }
                else if (version >= 173)
                {
                    if (File.Exists("OldCemuHook.zip"))
                    {
                        unpacker.Unpack("OldCemuHook.zip", v.Folder);
                    }
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
                foreach (var line in dlc.Result.Split('\n'))
                {
                    if (line.Contains("<td class=\"title\">"))
                    {
                        string name = line.Substring(line.LastIndexOf("\">", StringComparison.Ordinal) + 2, line.LastIndexOf("/a", StringComparison.Ordinal) - line.LastIndexOf("\">", StringComparison.Ordinal) - 3).Replace("&amp;", "&");
                        currentGames = Persistence.GetGames(model, name);
                    }
                    if (line.Contains("<td class=\"rating\">"))
                    {
                        string rating = line.Substring(line.LastIndexOf("title=", StringComparison.Ordinal) + 7, line.LastIndexOf("\"", StringComparison.Ordinal) - line.LastIndexOf("title=", StringComparison.Ordinal) - 7);
                        if (currentGames != null)
                        {
                            SetGameStatus(currentGames, rating);
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
        private static void SetGameStatus(List<GameInformation> currentGames, string rating)
        {
            switch (rating)
            {
                case "Playable":
                    foreach (var currentGame in currentGames)
                        currentGame.GameSetting.OfficialEmulationState =
                            GameSettings.EmulationStateType.Playable;
                    break;
                case "Perfect":
                    foreach (var currentGame in currentGames)
                        currentGame.GameSetting.OfficialEmulationState = GameSettings.EmulationStateType.Perfect;
                    break;
                case "Loads":
                    foreach (var currentGame in currentGames)
                        currentGame.GameSetting.OfficialEmulationState = GameSettings.EmulationStateType.Loads;
                    break;
                case "Runs":
                    foreach (var currentGame in currentGames)
                        currentGame.GameSetting.OfficialEmulationState = GameSettings.EmulationStateType.Runs;
                    break;
                case "Unplayable":
                    foreach (var currentGame in currentGames)
                        currentGame.GameSetting.OfficialEmulationState =
                            GameSettings.EmulationStateType.Unplayable;
                    break;
                default:
                    foreach (var currentGame in currentGames)
                        currentGame.GameSetting.OfficialEmulationState = GameSettings.EmulationStateType.NotSet;
                    break;
            }
        }
    }
}
