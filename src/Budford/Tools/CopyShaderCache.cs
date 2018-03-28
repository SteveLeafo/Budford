using Budford.Control;
using System.IO;

namespace Budford.Tools
{
    class CopyShaderCache: Tool
    {
        readonly Model.Model model;

        internal CopyShaderCache(Model.Model modelIn)
            : base(modelIn)
        {
            model = modelIn;
        }

        public override bool Execute()
        {
            foreach (var game in model.GameData)
            {
                string folder = game.Value.SaveDir;
                string saveFolder = Path.Combine(model.Settings.SavesFolder, "Budford");
                if (!game.Value.SaveDir.StartsWith("??"))
                {
                    foreach (var latest in model.Settings.InstalledVersions)
                    {
                        string src = Path.Combine(latest.Folder, "shaderCache", "transferable" , game.Value.SaveDir + ".bin");
                        string dest = Path.Combine(saveFolder, folder, "post_180.bin");

                        if (File.Exists(src))
                        {
                            CopyFile(folder, saveFolder, src, dest);
                        }
                    }
                }
            }
            return true;
        }

        private static void CopyFile(string folder, string saveFolder, string src, string dest)
        {
            if (File.Exists(dest))
            {
                FileInfo srcFi = new FileInfo(src);
                FileInfo destFi = new FileInfo(dest);
                if (srcFi.Length > destFi.Length)
                {
                    // Always keep a copy of the largest
                    FileManager.SafeCopy(src, dest, true);
                }
            }
            else
            {
                FileManager.SafeCreateDirectory(saveFolder + folder);
                FileManager.SafeCopy(src, dest, true);
            }
        }
    }
}
