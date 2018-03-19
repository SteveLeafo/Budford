using System.Collections.Generic;

namespace Budford.Model
{
    public class Settings
    {
        internal string CurrentCemuVersion  = "1.9.0";

        // Where to find the games
        public List<string> RomFolders  = new List<string>();

        // What versions have we downloads and installed
        public List<InstalledVersion> InstalledVersions  = new List<InstalledVersion>();

        public string DefaultInstallFolder  = "Cemu";

        public string MlcFolder = "";

        public string SavesFolder = "";

        public string DownloadsFolder = "";

        public string CemuHookServerIp = "";

        public string CemuHookServerPort = "";

        public string CurrentView  = "Detailed";

        public string GraphicsPackRevision = "347";

        public bool BorderlessFullScreen = false;

        public int CurrentSortColumn = 1;

        public int CurrentSortDirection = 0;

        public int GlobalVolume = 85;

        public int SingleCorePriority = 1;

        public int DualCorePriority = 1;

        public int TripleCorePriority = 3;

        public int ShaderPriority = 2;

        public bool UpdateDiscordPresence = false;

        public bool ShowToolBar  = true;

        public bool ShowStausBar  = true;

        public bool DisableShaderCache;

        public bool ForceLowResolutionGraphicsPacks;

        public bool UseGlobalVolumeSettings = false;

        public bool LegacyIntelGpuMode = false;

        public bool ScanGameFoldersOnStart = false;

        public bool IncludeWiiULauncherRpx = true;

        public bool AutomaticallyDownloadGraphicsPackOnStart = false;

        public bool AutomaticallyDownloadLatestEverythingOnStart = false;

        public string DefaultResolution = "default";

        public ConsoleRegionType ConsoleRegion = ConsoleRegionType.Auto;

        public ConsoleLanguageType ConsoleLanguage = ConsoleLanguageType.English;

        public string LaunchBoxExe = "";

        public string WineExe = "";

        public string StopHotkey = "None";

        internal bool AutoSizeColumns = true;

        public bool HideWindowWhenCaching = true;

        public int Monitor = 1;

        public int GamePadMonitor = 1;

        public string WiiUCommonKey;

        public enum ConsoleRegionType : byte
        {
            Auto = 0xFF,
            Jap = 0x01,
            Usa = 0x02,
            Eur = 0x04,
            China = 0x10,
            Korea = 0x20,
            Taiwan = 0x40
        }

        public enum ConsoleLanguageType : byte
        {
            Jap = 0x00,
            English = 0x01,
            French = 0x02,
            German = 0x03,
            Italian = 0x04,
            Spanish = 0x05,
            Chinese = 0x06,
            Korean = 0x07,
            Dutch = 0x08,
            Portugese = 0x09,
            Russian = 0x0A,
            Taiwanese = 0x0B
        }
    }
}
