using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Budford.Control;

namespace Budford.Tools
{
    class CopyShaderCache: Tool
    {
        Model.Model model;

        internal CopyShaderCache(Model.Model modelIn)
            : base(modelIn)
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
                    //var latest = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);

                    foreach (var latest in model.Settings.InstalledVersions)
                    {
                        string src = latest.Folder + "\\shaderCache\\transferable\\" + game.Value.SaveDir + ".bin";
                        string dest = saveFolder + folder + "\\post_180.bin";

                        if (File.Exists(src))
                        {
                            if (File.Exists(dest))
                            {
                                FileInfo srcFI = new FileInfo(src);
                                FileInfo destFI = new FileInfo(dest);
                                if (srcFI.Length > destFI.Length)
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
