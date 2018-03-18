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
        const int LevenshteinTolerence = 5;

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
            return model;
        }

        /// <summary>
        /// Save to XML
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static Model.PlugIns.PlugIn LoadPlugin(string fileName)
        {
            Model.PlugIns.PlugIn plugin;

            if (File.Exists(fileName))
            {
                XmlSerializer xsSubmit = new XmlSerializer(typeof(Model.PlugIns.PlugIn));

                using (var sww = new StreamReader(fileName))
                {
                    XmlReader writer = XmlReader.Create(sww);
                    plugin = (Model.PlugIns.PlugIn)xsSubmit.Deserialize(writer);
                }

                return plugin;
            }
            plugin = new Model.PlugIns.PlugIn();
            return plugin;
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

                if (gd.StatusUpdates.Count == 0)
                {
                    StatusUpdate su = new StatusUpdate()
                    {
                        UpdateDate = DateTime.Now.Ticks.ToString(),
                        Status = gd.GameSetting.OfficialEmulationState

                    };
                   gd.StatusUpdates.Add(su);
                }
                if (gd.SaveDir.Contains("??") )
                {
                    //model.Sa
                }

                if (FolderScanner.Regions.TryGetValue(key.Substring(0, 4), out gd.Type))
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
            PurgeGames(model);
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
                if (gd.Value.SaveDir.StartsWith("??") || gd.Value.SaveDir.Contains("."))
                {
                    if (!gd.Value.Image)
                    {
                        gd.Value.SaveDir = Tools.HashGenerator.GetHash(gd.Value.LaunchFile).ToString("x8");
                    }
                    else
                    {
                        gd.Value.SaveDir = Tools.HashGenerator.GetHash(gd.Value.RpxFile).ToString("x8");
                    }
                }

                gd.Value.GameSetting.PreviousOfficialEmulationState = gd.Value.GameSetting.OfficialEmulationState;

                if (gd.Value.ShaderCacheFileSize == -1)
                {
                    var version = CemuFeatures.GetLatestVersion(model);
                    FileInfo info = new FileInfo(Path.Combine(SpecialFolders.ShaderCacheFolderCemu(version), gd.Value.SaveDir + ".bin"));
                    if (File.Exists(info.FullName))
                    {
                        gd.Value.ShaderCacheFileSize = (int)info.Length;
                    }
                    else
                    {
                        gd.Value.ShaderCacheFileSize = 999;
                    }

                }

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

            name = SetCleanName(name);

            CheckForExactMatches(model, name, games);

            CheckForCloseMatches(model, name, games);

            CheckForContainedMatches(model, name, games);

            CheckForStartingMatches(model, name, games);

            return games;
        }

        private static void CheckForStartingMatches(Model.Model model, string name, List<GameInformation> games)
        {
            if (games.Count == 0)
            {
                foreach (var g in model.GameData)
                {
                    if (name.StartsWith(g.Value.CleanName))
                    {
                        if (g.Value.CleanName.Length > 8)
                        {
                            games.Add(g.Value);
                            g.Value.CleanName = "";
                        }
                    }
                }
            }
        }

        private static void CheckForContainedMatches(Model.Model model, string name, List<GameInformation> games)
        {
            if (games.Count == 0)
            {
                foreach (var g in model.GameData)
                {
                    if (g.Value.CleanName.Contains(name))
                    {
                        games.Add(g.Value);
                        g.Value.CleanName = "";
                    }
                }
            }
        }

        private static void CheckForCloseMatches(Model.Model model, string name, List<GameInformation> games)
        {
            if (games.Count == 0)
            {
                foreach (var g in model.GameData)
                {
                    bool add = false;
                    if (g.Value.CleanName.StartsWith(name))
                    {
                        add = true;
                    }
                    else if (name.Length > 10 && LevenshteinDistance.Compute(g.Value.CleanName, name) < LevenshteinTolerence)
                    {
                        add = true;
                    }
                    else if (name.Length <= 10 && LevenshteinDistance.Compute(g.Value.CleanName, name) < 2)
                    {
                        add = true;
                    }
                    if (add)
                    {
                        games.Add(g.Value);
                        g.Value.CleanName = "";
                    }
                }
            }
        }

        private static void CheckForExactMatches(Model.Model model, string name, List<GameInformation> games)
        {
            foreach (var g in model.GameData)
            {
                if (name == g.Value.CleanName)
                {
                    games.Add(g.Value);
                    g.Value.CleanName = "";
                }
            }
        }

        /// <summary>
        /// Returns a list of games with matching titles
        /// </summary>
        /// <param name="model"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static GameInformation GetClosestMatchingGame(Model.Model model, string name)
        {
            name = SetCleanName(name);
            foreach (var g in model.GameData)
            {
                if (name == g.Value.CleanName)
                {
                    return g.Value;
                }
            }
            for (int levenshteinTolerence = 5; levenshteinTolerence < 10; levenshteinTolerence += 2)
            {
                foreach (var g in model.GameData)
                {
                    if (g.Value.CleanName == name || g.Value.CleanName.StartsWith(name))
                    {
                        return g.Value;
                    }
                    else if (LevenshteinDistance.Compute(g.Value.CleanName, name) < levenshteinTolerence)
                    {
                        return g.Value;
                    }
                }
            }
            foreach (var g in model.GameData)
            {
                if (name.StartsWith(g.Value.CleanName))
                {
                    if (g.Value.CleanName.Length > 12)
                    {
                        return g.Value;
                    }
                }
            }

            foreach (var g in model.GameData)
            {
                if (g.Value.CleanName.Contains(name))
                {
                    return g.Value;
                }
            }

            return null;
        }

        internal static void SetCleanNames(Model.Model model)
        {
            foreach (var g in model.GameData)
            {
                g.Value.CleanName = SetCleanName(g.Value.Name);
            }
        }

        private static string SetCleanName(string name)
        {
            string cleanName = name.ToUpper();
            if (cleanName == "DISNEY INFINITY")
            {
                cleanName = "INFINITY DISNEY";
            }
            cleanName = cleanName.Replace("TOM CLANCY'S", "");
            cleanName = cleanName.Replace("REV.", "Revolution");
            cleanName = cleanName.Replace(":", "");
            cleanName = cleanName.Replace("®", "");
            cleanName = cleanName.Replace("®", "");
            cleanName = cleanName.Replace("™", "");
            cleanName = cleanName.Replace("!", "");
            cleanName = cleanName.Replace("+", "");
            cleanName = cleanName.Replace("[", "");
            cleanName = cleanName.Replace("]", "");
            cleanName = cleanName.Replace(".", "");
            cleanName = cleanName.Replace("’", "");
            cleanName = cleanName.Replace("'", "");
            cleanName = cleanName.Replace("-", "");
            cleanName = cleanName.Replace("REMIX 1", "REMIX");
            cleanName = cleanName.Replace("ADVENTURES 1", "ADVENTURES");
            cleanName = cleanName.Replace("MARIO & SONIC", "M & S");
            cleanName = cleanName.Replace("PAINTBRUSH", "");
            cleanName = cleanName.Replace("FOR", "");
            cleanName = cleanName.Replace("WII", "");
            cleanName = cleanName.Replace(" EP ", " Extended Play ");
            cleanName = cleanName.Replace("  ", " ");
            cleanName = cleanName.Replace("  ", " ");
            cleanName = cleanName.ToUpper();
            cleanName = cleanName.Trim();
            return cleanName;
        }
    
    }
}
