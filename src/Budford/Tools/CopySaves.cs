﻿using System;
using System.Linq;
using System.IO;
using Budford.Control;

namespace Budford.Tools
{
    class CopySaves : Tool
    {
        readonly Model.Model model;

        internal CopySaves(Model.Model modelIn) : base(modelIn)
        {
            model = modelIn;
        }

        public override bool Execute()
        {
            foreach (var game in model.GameData)
            {
                if (!game.Value.SaveDir.StartsWith("??"))
                {
                    Model.InstalledVersion latest = null;
                    FileInfo latestFile = null;
                    GetLatestVersion(game.Value, ref latest, ref latestFile);
                    if (latest != null)
                    {
                        CopySaveFolder(game.Value, latest);
                    }
                }
            }
            return true;
        }

        private void GetLatestVersion(Model.GameInformation game, ref Model.InstalledVersion latest, ref FileInfo latestFile)
        {
            foreach (var version in model.Settings.InstalledVersions)
            {
                DirectoryInfo src = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirCemu(model, version, game));
                FileInfo fi = GetNewestFile(src);
                if (fi != null)
                {
                    if (latestFile == null || latestFile.LastWriteTime < fi.LastWriteTime)
                    {
                        latestFile = fi;
                        latest = version;
                    }
                }
            }
        }

        public static FileInfo GetNewestFile(DirectoryInfo directory)
        {
            if (Directory.Exists(directory.FullName))
            {
                return directory.GetFiles()
                    .Union(directory.GetDirectories().Select(GetNewestFile))
                    .OrderByDescending(f => (f == null ? DateTime.MinValue : f.LastWriteTime))
                    .FirstOrDefault();
            }
            return null;
        }

        private void CopySaveFolder(Model.GameInformation game, Model.InstalledVersion latest)
        {
            DirectoryInfo src = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirCemu(model, latest, game));
            DirectoryInfo src255 = new DirectoryInfo(SpecialFolders.CommonUserFolderCemu(model, latest, game));

            DirectoryInfo dest = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirBudford(model, model.CurrentUser, game, ""));
            DirectoryInfo dest255 = new DirectoryInfo(SpecialFolders.CommonSaveDirBudford(model, game, ""));

            if (Directory.Exists(src.FullName))
            {
                if (src.GetFiles().Any() || src.GetDirectories().Any() || (Directory.Exists(src255.FullName) && (src255.GetFiles().Any() || src255.GetDirectories().Any())))
                {
                    Launcher.UpdateFolder(src, dest, true);
                    Launcher.UpdateFolder(src255, dest255, true);
                }
            }
        }
    }
}
