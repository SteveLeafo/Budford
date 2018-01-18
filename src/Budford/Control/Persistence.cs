using Budford.Model;
using Budford.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Budford.Control
{
    internal static class Persistence
    {
        /// <summary>
        /// Save to XML
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static Model.Model Load(string fileName)
        {
            Model.Model model;

            if (File.Exists(fileName))
            {
                XmlSerializer xsSubmit = new XmlSerializer(typeof(Model.Model));

                using (var sww = new StreamReader(fileName))
                {
                    XmlReader writer = XmlReader.Create(sww);
                    model = (Model.Model)xsSubmit.Deserialize(writer);
                }


                FileManager.SearchForInstalledVersions(model);
                FolderScanner.GetGameInformation(null, "", "");
                SetGameTypes(model);
                CemuFeatures.UpdateFeaturesForInstalledVersions(model);

                SetSaveDirs(model);

                return model;
            }
            model = new Model.Model();
            FileManager.SearchForInstalledVersions(model);
            FolderScanner.GetGameInformation(null, "", "");
            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        internal static void SetGameTypes(Model.Model model)
        {
            foreach (var gd in model.GameData2)
            {
                string key = gd.ProductCode.Replace("WUP-P-", "").Replace("WUP-U-", "").Replace("WUP-N-", "") + gd.CompanyCode;

                if (gd.SaveDir.Contains("??") )
                {
                    //model.Sa
                }

                if (FolderScanner.regions.TryGetValue(key.Substring(0, 4), out gd.Type))
                {
                    if (!model.GameData.ContainsKey(key))
                    {
                        model.GameData.Add(key, gd);
                    }
                }
                else
                {
                    gd.Type = "eShop";
                    if (!model.GameData.ContainsKey(key))
                    {
                        model.GameData.Add(key, gd);
                    }
                }
            }
            Persistence.PurgeGames(model);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Save(Model.Model model)
        {
            SaveDirs saveDirs = new SaveDirs();
            AddSaveDirs(model, saveDirs);

            XmlSerializer modelSerializer = new XmlSerializer(typeof(SaveDirs));

            using (var sww = new StreamWriter("SaveDirDatabase.xml"))
            {
                XmlWriterSettings xws = new XmlWriterSettings() { Indent = true, NewLineHandling = NewLineHandling.Entitize };
                XmlWriter writer = XmlWriter.Create(sww, xws);
                modelSerializer.Serialize(writer, saveDirs);
            }
        }
    
        /// <summary>
        /// 
        /// </summary>
        public static SaveDirs Load()
        {
            SaveDirs saveDirs;
            XmlSerializer xsSubmit = new XmlSerializer(typeof(SaveDirs));

            using (var sww = new StreamReader("SaveDirDatabase.xml"))
            {
                XmlReader writer = XmlReader.Create(sww);
                saveDirs = (SaveDirs)xsSubmit.Deserialize(writer);
            }
            return saveDirs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="saveDirs"></param>
        static void AddSaveDirs(Model.Model model, SaveDirs saveDirs)
        {
            saveDirs.AllSaveDirs = Load().AllSaveDirs;

            foreach (var game in model.GameData2)
            {
                if (!game.SaveDir.StartsWith("??"))
                {
                    SaveDir sd = saveDirs.AllSaveDirs.FirstOrDefault(g => g.GameId == game.TitleId);
                    if (sd == null)
                    {
                        sd = new SaveDir();
                        saveDirs.AllSaveDirs.Add(sd);
                    }
                    // We have a valid save dir
                    sd.GameId = game.TitleId;
                    sd.SaveDirId = game.SaveDir;
                    sd.GameName = game.Name;
                    sd.Region = game.Region;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        internal static void SetSaveDirs(Model.Model model)
        {
            foreach (var gd in model.GameData)
            {
                gd.Value.SaveDir = Tools.HashGenerator.GetHash(gd.Value.LaunchFile).ToString("x8");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldVersions"></param>
        internal static void LoadFromXml(List<OldVersion> oldVersions)
        {
            oldVersions.Clear();
            if (File.Exists("OldVersions.xml"))
            {
                XDocument xDoc = XDocument.Load("OldVersions.xml");
                IEnumerable<XElement> games = xDoc.Elements();

                // Read the entire XML
                foreach (var gd in games.Elements())
                {
                    oldVersions.Add(new OldVersion()
                    {
                        Name = Xml.GetValue(gd, "Name"),
                        Folder = Xml.GetValue(gd, "Folder"),
                        Uri = Xml.GetValue(gd, "Uri"),
                        ReleaseDate = Xml.GetValue(gd, "ReleaseDate"),
                    });
                }
            }
        }

        /// <summary>
        /// Load from XML
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fileName"></param>
        internal static void Save(Model.Model model, string fileName)
        {
            model.GameData2 = new List<GameInformation>();
            foreach (var gd in model.GameData)
            {
                model.GameData2.Add(gd.Value);
            }

            try
            {
                XmlSerializer modelSerializer = new XmlSerializer(typeof(Model.Model));
                using (var sww = new StreamWriter(fileName))
                {
                    XmlWriterSettings xws = new XmlWriterSettings() { Indent = true, NewLineHandling = NewLineHandling.Entitize };
                    XmlWriter writer = XmlWriter.Create(sww, xws);
                    {
                        modelSerializer.Serialize(writer, model);
                    }
                } 
            }
            catch (Exception ex)
            {
                model.Errors.Add(ex.Message);
            }
        }

        internal static void PurgeGames(Model.Model model)
        {
            model.GameData2 = new List<GameInformation>();
            foreach (var gd in model.GameData)
            {
                model.GameData2.Add(gd.Value);
            }

            model.GameData.Clear();

            foreach (var game in model.GameData2)
            {
                if (IsCurrentFolder(model, game))
                {
                    string key = game.ProductCode.Replace("WUP-P-", "").Replace("WUP-U-", "").Replace("WUP-N-", "") + game.CompanyCode;
                    model.GameData.Add(key, game);
                }
            }
        }

        static bool IsCurrentFolder(Model.Model model, GameInformation game)
        {
            foreach (var folder in model.Settings.RomFolders)
            {
                if (game.LaunchFile.StartsWith(folder))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Returns a list of games with matching titles
        /// </summary>
        /// <param name="model"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static List<GameInformation> GetGames(Model.Model model, string name)
        {
            List<GameInformation> games = new List<GameInformation>();
            foreach (var g in model.GameData)
            {
                if (g.Value.Name.ToUpper() == name.ToUpper() || g.Value.Name.ToUpper().StartsWith(name.ToUpper()))
                {
                    games.Add(g.Value);
                }
                else if (LevenshteinDistance.Compute(g.Value.Name.ToUpper(), name.ToUpper()) < 5)
                {
                    if (g.Value.GameSetting.OfficialEmulationState == GameSettings.EmulationStateType.NotSet)
                    {
                        games.Add(g.Value);
                    }
                }
            }
            return games;
        }
    }
}
