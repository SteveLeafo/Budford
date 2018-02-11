using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Budford.Control;

namespace Budford.Tools
{
    class CopySaves : Tool
    {
        Model.Model model;

        internal CopySaves(Model.Model modelIn) : base(modelIn)
        {
            model = modelIn;
        }

        public override bool Execute()
        {
            foreach (var game in model.GameData)
            {
                string folder = game.Value.Name.Replace(":", "_");
                folder = game.Value.SaveDir;
                string saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Budford\\";

                if (!game.Value.SaveDir.StartsWith("??"))
                {
                    Budford.Model.InstalledVersion latest = null;
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
                        CopySaveFolder(game.Value, folder, saveFolder, latest);
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
                    .Union(directory.GetDirectories().Select(d => GetNewestFile(d)))
                    .OrderByDescending(f => (f == null ? DateTime.MinValue : f.LastWriteTime))
                    .FirstOrDefault();
            }
            return null;
        }

        private void CopySaveFolder(Budford.Model.GameInformation game, string folder, string saveFolder, Budford.Model.InstalledVersion latest)
        {
            DirectoryInfo src = new DirectoryInfo(SpecialFolders.CurrenUserSaveDirCemu(latest, game));
            DirectoryInfo src_255 = new DirectoryInfo(SpecialFolders.CommonUserFolderCemu(latest, game));

            DirectoryInfo dest = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirBudford(model.CurrentUser, game, ""));
            DirectoryInfo dest_255 = new DirectoryInfo(SpecialFolders.CommonSaveDirBudford(game, ""));

            if (Directory.Exists(src.FullName))
            {
                if (src.GetFiles().Any() || src.GetDirectories().Any() || (Directory.Exists(src_255.FullName) && (src_255.GetFiles().Any() || src_255.GetDirectories().Any())))
                {
                    Launcher.UpdateFolder(src, dest, true);
                    Launcher.UpdateFolder(src_255, dest_255, true);
                }
            }
        }
    }
}
