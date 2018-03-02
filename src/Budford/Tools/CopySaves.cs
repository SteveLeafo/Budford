using System;
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
                    foreach (var version in model.Settings.InstalledVersions)
                    {
                        DirectoryInfo src = new DirectoryInfo(SpecialFolders.CurrenUserSaveDirCemu(version, game.Value));
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
                    if (latest != null)
                    {
                        CopySaveFolder(game.Value, latest);
                    }
                }
            }
            return true;
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
            DirectoryInfo src = new DirectoryInfo(SpecialFolders.CurrenUserSaveDirCemu(latest, game));
            DirectoryInfo src255 = new DirectoryInfo(SpecialFolders.CommonUserFolderCemu(latest, game));

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
