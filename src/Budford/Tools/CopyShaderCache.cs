using System;
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
                string saveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Budford");
                if (!game.Value.SaveDir.StartsWith("??"))
                {
                    foreach (var latest in model.Settings.InstalledVersions)
                    {
                        string src = Path.Combine(latest.Folder, "shaderCache", "transferable" , game.Value.SaveDir + ".bin");
                        string dest = Path.Combine(saveFolder, folder, "post_180.bin");

                        if (File.Exists(src))
                        {
                            if (File.Exists(dest))
                            {
                                FileInfo srcFi = new FileInfo(src);
                                FileInfo destFi = new FileInfo(dest);
                                if (srcFi.Length > destFi.Length)
                                {
                                    // Always keep a copy of the largest
                                    File.Copy(src, dest, true);
                                }
                            }
                            else                            
                            {
                                if (!Directory.Exists(saveFolder + folder))
                                {
                                    Directory.CreateDirectory(saveFolder + folder);
                                }
                                File.Copy(src, dest, true);
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
