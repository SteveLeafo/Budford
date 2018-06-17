using Budford.Model;
using Budford.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Budford.Control
{
    internal static class FolderScanner
    {
        internal static readonly Dictionary<string, string> Regions = new Dictionary<string, string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameData"></param>
        /// <param name="folder"></param>
        /// <param name="launchFile"></param>
        internal static void GetGameInformation(Dictionary<string, GameInformation> gameData, string folder, string launchFile)
        {
            Regions.Clear();
            if (File.Exists("wiiutdb.xml"))
            {
                XElement xElement = XElement.Parse(XDocument.Load("wiiutdb.xml").ToString());

                foreach (var g in xElement.Elements("game"))
                {
                    string type = Xml.GetValue(g, "type");
                    string id = Xml.GetValue(g, "id").Substring(0, 4);

                    if (!Regions.ContainsKey(id))
                    {
                        Regions.Add(id, type);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentDirectory"></param>
        /// <returns></returns>
        public static long GetDirectorySize(string parentDirectory)
        {
            return new DirectoryInfo(parentDirectory).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => file.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="graphicsPacks"></param>
        internal static void FindGraphicsPacks(DirectoryInfo source, Dictionary<string, List<GraphicsPack>> graphicsPacks)
        {
            if (Directory.Exists(source.FullName))
            {
                foreach (DirectoryInfo dir in source.GetDirectories())
                {
                    FindGraphicsPacks(dir, graphicsPacks);
                }

                foreach (FileInfo file in source.GetFiles("rules.txt"))
                {
                    ParseGraphicsPack(file, graphicsPacks);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        internal static void AddGraphicsPacksToGames(Model.Model model)
        {
            foreach (var game in model.GameData)
            {
                if (model.GraphicsPacks.ContainsKey(game.Value.TitleId))
                {
                    foreach (var pack in model.GraphicsPacks[game.Value.TitleId])
                    {
                        AddGraphicsPack(game, pack);
                    }
                }
                game.Value.GraphicsPacksCount = game.Value.GameSetting.graphicsPacks.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        internal static void CheckGames(Model.Model model)
        {
            foreach (var game in model.GameData)
            {
                game.Value.Exists = File.Exists(game.Value.LaunchFile);

                if (!game.Value.Exists)
                {
                    string currentRomFolder = game.Value.LaunchFile;
                    for (int i = 0; i < 3; ++i)
                    {
                        currentRomFolder = currentRomFolder.Substring(0, currentRomFolder.LastIndexOf(Path.DirectorySeparatorChar));
                    }
                    foreach (var version in model.Settings.RomFolders)
                    {
                        string attempt = game.Value.LaunchFile.Replace(currentRomFolder, version);
                        if (File.Exists(attempt))
                        {
                            game.Value.LaunchFile = attempt;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        private static void AddGraphicsPack(KeyValuePair<string, GameInformation> game, GraphicsPack pack)
        {
            if (!PackAdded(game.Value, pack.Folder))
            {
                if (!game.Value.GameSetting.GraphicsPacksFolders.Contains(pack.Folder))
                {
                    game.Value.GameSetting.GraphicsPacksFolders.Add(pack.Folder);
                    game.Value.GameSetting.graphicsPacks.Add(pack);
                    game.Value.GraphicsPacksCount = game.Value.GameSetting.graphicsPacks.Count;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="packIn"></param>
        /// <returns></returns>
        static bool PackAdded(GameInformation game, string packIn)
        {
            foreach (var pack in game.GameSetting.graphicsPacks)
            {
                if (pack.Folder == packIn)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="graphicsPacks"></param>
        static void ParseGraphicsPack(FileInfo file, Dictionary<string, List<GraphicsPack>> graphicsPacks)
        {
            if (file.Name != "rules.txt")
            {
                return;
            }

            using (StreamReader sr = new StreamReader(file.FullName))
            {
                GraphicsPack pack = new GraphicsPack { File = file.FullName };

                string[] ids = ExtractIds(sr, pack);

                if (ids != null)
                {
                    AssignIdsToPack(file, graphicsPacks, pack, ids);
                }
            }
        }

        private static void AssignIdsToPack(FileInfo file, Dictionary<string, List<GraphicsPack>> graphicsPacks, GraphicsPack pack, string[] ids)
        {
            foreach (string i in ids)
            {
                string id = i.ToUpper();
                if (id.Length == 15)
                {
                    id = "0" + id;
                }

                if (file.DirectoryName != null)
                {
                    pack.Folder = file.DirectoryName.Substring(1 + file.DirectoryName.LastIndexOf(Path.DirectorySeparatorChar));
                }

                if (!graphicsPacks.ContainsKey(id))
                {
                    graphicsPacks.Add(id, new List<GraphicsPack>());
                }

                if (!IsContained(graphicsPacks[id], pack))
                {
                    graphicsPacks[id].Add(pack);
                }
            }
        }

        private static string[] ExtractIds(StreamReader sr, GraphicsPack pack)
        {
            string[] ids = null;

            foreach (string line in sr.ReadToEnd().Split('\n'))
            {
                if (line.Contains("titleIds"))
                {
                    ids = ExtractTitleIds(ids, line);
                }

                if (line.Contains("name = "))
                {
                    ExtractName(pack, line);
                }
            }
            return ids;
        }

        private static bool IsContained(List<GraphicsPack> list, GraphicsPack pack)
        {
            foreach (var p in list)
            {
                if (p.Folder == pack.Folder)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="line"></param>
        private static void ExtractName(GraphicsPack pack, string line)
        {
            string[] toks = line.Split('=');
            if (toks.Length > 0)
            {
                pack.Title = toks[1].Trim().TrimEnd('\"').TrimStart('\"');
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string[] ExtractTitleIds(string[] ids, string line)
        {
            string[] toks = line.Split('=');
            if (toks.Length > 1)
            {
                ids = toks[1].Trim().TrimEnd('\"').TrimStart('\"').Split(',');
            }
            else
            {
                toks = line.Split(' ');
                if (toks.Length > 1)
                {
                    ids = toks[1].Trim().TrimEnd('\"').TrimStart('\"').Split(',');
                }
            }

            return ids;
        }
    }
}
