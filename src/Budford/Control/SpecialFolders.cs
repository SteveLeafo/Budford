using Budford.Model;
using System.IO;

namespace Budford.Control
{
    internal static class SpecialFolders
    {
        #region Save Games

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="currentUser"></param>
        /// <param name="game"></param>
        /// <param name="snapShotDir"></param>
        /// <returns></returns>
        internal static string CurrentUserSaveDirBudford(Model.Model model, string currentUser, GameInformation game, string snapShotDir)
        {
            string gameId = game.TitleId.Replace("00050000", "");
            if (snapShotDir.Length == 0)
            {
                return Path.Combine(model.Settings.SavesFolder, "Budford", game.SaveDir, "00050000", gameId, "user", currentUser);
            }
            return Path.Combine(model.Settings.SavesFolder, "Budford", game.SaveDir, "00050000", gameId, "user", currentUser + "_" + snapShotDir);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="game"></param>
        /// <param name="snapShotDir"></param>
        /// <returns></returns>
        internal static string CommonSaveDirBudford(Model.Model model, GameInformation game, string snapShotDir)
        {
            string gameId = game.TitleId.Replace("00050000", "");
            if (snapShotDir.Length == 0)
            {
                return Path.Combine(model.Settings.SavesFolder, "Budford", game.SaveDir, "00050000", gameId, "user", "common");
            }
            return Path.Combine(model.Settings.SavesFolder, "Budford", game.SaveDir, "00050000", gameId, "user", "common" + "_" + snapShotDir);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static string CommonUserFolderCemu(InstalledVersion version, GameInformation game)
        {
            string gameId = game.TitleId.Replace("00050000", "");
            if (version.VersionNumber >= 1110)
            {
                return Path.Combine(version.Folder, "mlc01", "usr", "save", "00050000", gameId, "user", "common");
            }
            return Path.Combine(version.Folder, "mlc01", "emulatorSave", game.SaveDir + "_255");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static string CurrentUserSaveDirCemu(InstalledVersion version, GameInformation game)
        {
            string gameId = game.TitleId.Replace("00050000", "");
            if (version.VersionNumber >= 1110)
            {
                return Path.Combine(version.Folder, "mlc01", "usr", "save", "00050000", gameId, "user", "80000001");
            }
            return Path.Combine(version.Folder, "mlc01", "emulatorSave", game.SaveDir);
        }

        #endregion

        #region Shader Cache

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        internal static string ShaderCacheFolderCemu(InstalledVersion version)
        {
            if (version != null)
            {
                return Path.Combine(version.Folder, "shaderCache", "transferable");
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static string ShaderCacheCemu(InstalledVersion version, GameInformation game)
        {
            string suffix = "";

            if (game.GameSetting.UseSeperableShaders == 0)
            {
                suffix = "_j";
            }

            return Path.Combine(ShaderCacheFolderCemu(version),  game.SaveDir + suffix + ".bin");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static string ShaderCacheBudford(Model.Model model, GameInformation game)
        {
            string suffix = "";

            if (game.GameSetting.UseSeperableShaders == 0)
            {
                suffix = "_j";
            }

            return Path.Combine(model.Settings.SavesFolder, "Budford", game.SaveDir, "post_180" + suffix + ".bin");
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static string CafeLibDirBudford(Model.Model model)
        {
            return Path.Combine(model.Settings.SavesFolder, "Budford", "cafeLibs");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static string BudfordDir(Model.Model model)
        {
            return Path.Combine(model.Settings.SavesFolder, "Budford");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static string PlugInFolder(Model.Model model)
        {
            return Path.Combine(model.Settings.SavesFolder, "Budford", "PlugIns");
        }
    }
}
