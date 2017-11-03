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
                    //var latest = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);

                    foreach (var latest in model.Settings.InstalledVersions)
                    {
                        DirectoryInfo src = new DirectoryInfo(latest.Folder + "\\mlc01\\emulatorSave\\" + game.Value.SaveDir);
                        DirectoryInfo dest = new DirectoryInfo(saveFolder + folder + "\\00050000" + "\\" + game.Value.TitleId.Replace("00050000", "") + "\\user\\80000001");
                        DirectoryInfo src_255 = new DirectoryInfo(latest.Folder + "\\mlc01\\emulatorSave\\" + game.Value.SaveDir + "_255");
                        DirectoryInfo dest_255 = new DirectoryInfo(saveFolder + folder + "\\00050000" + "\\" + game.Value.TitleId.Replace("00050000", "") + "\\user\\common");

                        if (src.Exists)
                        {
                            if (src.GetFiles().Any() || (src_255.Exists && src_255.GetFiles().Any()))
                            {
                                if (!Directory.Exists(saveFolder + folder + "\\00050000" + "\\" + game.Value.TitleId.Replace("00050000", "") + "\\user\\80000001"))
                                {
                                    Directory.CreateDirectory(saveFolder + folder + "\\00050000" + "\\" + game.Value.TitleId.Replace("00050000", "") + "\\user\\80000001");
                                }
                                if (!Directory.Exists(saveFolder + folder + "\\00050000" + "\\" + game.Value.TitleId.Replace("00050000", "") + "\\user\\common"))
                                {
                                    Directory.CreateDirectory(saveFolder + folder + "\\00050000" + "\\" + game.Value.TitleId.Replace("00050000", "") + "\\user\\common");
                                }

                                FileManager.CopyFilesRecursively(src, dest, false, true);
                                FileManager.CopyFilesRecursively(src_255, dest_255, false, true);
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
