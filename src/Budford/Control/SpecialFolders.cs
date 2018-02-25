using Budford.Model;
using System;
using System.IO;

namespace Budford.Control
{
    internal static class SpecialFolders
    {
        #region Save Games

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="game"></param>
        /// <param name="snapShotDir"></param>
        /// <returns></returns>
        internal static string CurrentUserSaveDirBudford(string currentUser, GameInformation game, string snapShotDir)
        {
            string gameId = game.TitleId.Replace("00050000", "");
            if (snapShotDir.Length == 0)
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Budford", game.SaveDir, "00050000", gameId, "user", currentUser);
            }
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Budford", game.SaveDir, "00050000", gameId, "user", currentUser + "_" + snapShotDir);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="snapShotDir"></param>
        /// <returns></returns>
        internal static string CommonSaveDirBudford(GameInformation game, string snapShotDir)
        {
            string gameId = game.TitleId.Replace("00050000", "");
            if (snapShotDir.Length == 0)
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Budford", game.SaveDir, "00050000", gameId, "user", "common");
            }
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Budford", game.SaveDir, "00050000", gameId, "user", "common" + "_" + snapShotDir);
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
        internal static string CurrenUserSaveDirCemu(InstalledVersion version, GameInformation game)
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
            return Path.Combine(ShaderCacheFolderCemu(version),  game.SaveDir + ".bin");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static string ShaderCacheBudford(GameInformation game)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Budford", game.SaveDir, "post_180.bin");
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="snapShotDir"></param>
        /// <returns></returns>
        internal static string CafeLibDirBudford()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Budford", "cafeLibs");
        }
    }
}
