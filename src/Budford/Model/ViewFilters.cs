using Budford.View;

namespace Budford.Model
{
    public class ViewFilters
    {
        // Region
        public bool ViewRegionUsa = true;
        public bool ViewRegionEur = true;
        public bool ViewRegionJap = true;
        public bool ViewRegionAll = true;

        // Type
        public bool ViewTypeWiiU = true;
        public bool ViewTypeEshop = true;
        public bool ViewTypeChannel = true;
        public bool ViewTypeVc = true;

        // Type
        public bool ViewPlatformWiiU = true;
        public bool ViewPlatformHtml5 = true;
        public bool ViewPlatformNes = true;
        public bool ViewPlatformSnes = true;
        public bool ViewPlatformDs = true;
        public bool ViewPlatformN64 = true;

        // Status
        public bool ViewStatusNotSet = true;
        public bool ViewStatusPerfect = true;
        public bool ViewStatusPlayable = true;
        public bool ViewStatusRuns = true;
        public bool ViewStatusLoads = true;
        public bool ViewStatusUnplayable = true;

        // Decaf Status
        public bool ViewDecafStatusNotSet = true;
        public bool ViewDecafStatusPerfect = true;
        public bool ViewDecafStatusPlayable = true;
        public bool ViewDecafStatusRuns = true;
        public bool ViewDecafStatusLoads = true;
        public bool ViewDecafStatusUnplayable = true;

        // Official Status
        public bool ViewOfficialStatusNotSet = true;
        public bool ViewOfficialStatusPerfect = true;
        public bool ViewOfficialStatusPlayable = true;
        public bool ViewOfficialStatusRuns = true;
        public bool ViewOfficialStatusLoads = true;
        public bool ViewOfficialStatusUnplayable = true;

        // Ratings
        public bool ViewRating5 = true;
        public bool ViewRating4 = true;
        public bool ViewRating3 = true;
        public bool ViewRating2 = true;
        public bool ViewRating1 = true;

        internal static void OfficiallyAll(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewOfficialStatusPerfect = true;
            model.Filters.ViewOfficialStatusPlayable = true;
            model.Filters.ViewOfficialStatusRuns = true;
            model.Filters.ViewOfficialStatusLoads = true;
            model.Filters.ViewOfficialStatusUnplayable = true;
            model.Filters.ViewOfficialStatusNotSet = true;

            mainWindow.OfficiallyPerfectToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusPerfect;
            mainWindow.OfficiallyPlayableToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusPlayable;
            mainWindow.OfficiallyRunsToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusRuns;
            mainWindow.OfficiallyLoadsToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusLoads;
            mainWindow.OfficiallyUnplayableToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusUnplayable;
            mainWindow.OfficiallyNotSetToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusNotSet;

            mainWindow.PopulateListView();
        }

        internal static void OfficiallyNone(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewOfficialStatusPerfect = false;
            model.Filters.ViewOfficialStatusPlayable = false;
            model.Filters.ViewOfficialStatusRuns = false;
            model.Filters.ViewOfficialStatusLoads = false;
            model.Filters.ViewOfficialStatusUnplayable = false;
            model.Filters.ViewOfficialStatusNotSet = false;

            mainWindow.OfficiallyPerfectToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusPerfect;
            mainWindow.OfficiallyPlayableToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusPlayable;
            mainWindow.OfficiallyRunsToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusRuns;
            mainWindow.OfficiallyLoadsToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusLoads;
            mainWindow.OfficiallyUnplayableToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusUnplayable;
            mainWindow.OfficiallyNotSetToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusNotSet;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="mainWindow"></param>
        internal static void AllRegions(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewRegionUsa = true;
            model.Filters.ViewRegionEur = true;
            model.Filters.ViewRegionJap = true;

            mainWindow.UsaToolStripMenuItem.Checked = model.Filters.ViewRegionUsa;
            mainWindow.EuropeToolStripMenuItem.Checked = model.Filters.ViewRegionEur;
            mainWindow.JapanToolStripMenuItem.Checked = model.Filters.ViewRegionJap;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="mainWindow"></param>
        internal static void NoRegions(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewRegionUsa = false;
            model.Filters.ViewRegionEur = false;
            model.Filters.ViewRegionJap = false;

            mainWindow.UsaToolStripMenuItem.Checked = model.Filters.ViewRegionUsa;
            mainWindow.EuropeToolStripMenuItem.Checked = model.Filters.ViewRegionEur;
            mainWindow.JapanToolStripMenuItem.Checked = model.Filters.ViewRegionJap;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="mainWindow"></param>
        internal static void AllTypes(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewTypeWiiU = true;
            model.Filters.ViewTypeEshop = true;
            model.Filters.ViewTypeChannel = true;
            model.Filters.ViewTypeVc = true;

            mainWindow.WiiUToolStripMenuItem.Checked = model.Filters.ViewTypeWiiU;
            mainWindow.EShopToolStripMenuItem.Checked = model.Filters.ViewTypeEshop;
            mainWindow.ChannelToolStripMenuItem.Checked = model.Filters.ViewTypeChannel;
            mainWindow.VirtualConsoleToolStripMenuItem.Checked = model.Filters.ViewTypeVc;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="mainWindow"></param>
        internal static void NoTypes(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewTypeWiiU = false;
            model.Filters.ViewTypeEshop = false;
            model.Filters.ViewTypeChannel = false;
            model.Filters.ViewTypeVc = false;

            mainWindow.WiiUToolStripMenuItem.Checked = model.Filters.ViewTypeWiiU;
            mainWindow.EShopToolStripMenuItem.Checked = model.Filters.ViewTypeEshop;
            mainWindow.ChannelToolStripMenuItem.Checked = model.Filters.ViewTypeChannel;
            mainWindow.VirtualConsoleToolStripMenuItem.Checked = model.Filters.ViewTypeVc;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="mainWindow"></param>
        internal static void AllPlatforms(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewPlatformWiiU = true;
            model.Filters.ViewPlatformHtml5 = true;
            model.Filters.ViewPlatformNes = true;
            model.Filters.ViewPlatformSnes = true;
            model.Filters.ViewPlatformDs = true;
            model.Filters.ViewPlatformN64 = true;

            mainWindow.PlatWiiUToolStripMenuItem.Checked = model.Filters.ViewPlatformWiiU;
            mainWindow.PlatHtml5ToolStripMenuItem.Checked = model.Filters.ViewPlatformHtml5;
            mainWindow.PlatNintendo64ToolStripMenuItem.Checked = model.Filters.ViewPlatformN64;
            mainWindow.PlatNintendoDsToolStripMenuItem.Checked = model.Filters.ViewPlatformDs;
            mainWindow.PlatNESToolStripMenuItem.Checked = model.Filters.ViewPlatformNes;
            mainWindow.PlatSNESToolStripMenuItem.Checked = model.Filters.ViewPlatformSnes;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="mainWindow"></param>
        internal static void NoPlatforms(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewPlatformWiiU = false;
            model.Filters.ViewPlatformHtml5 = false;
            model.Filters.ViewPlatformNes = false;
            model.Filters.ViewPlatformSnes = false;
            model.Filters.ViewPlatformDs = false;
            model.Filters.ViewPlatformN64 = false;

            mainWindow.PlatWiiUToolStripMenuItem.Checked = model.Filters.ViewPlatformWiiU;
            mainWindow.PlatHtml5ToolStripMenuItem.Checked = model.Filters.ViewPlatformHtml5;
            mainWindow.PlatNintendo64ToolStripMenuItem.Checked = model.Filters.ViewPlatformN64;
            mainWindow.PlatNintendoDsToolStripMenuItem.Checked = model.Filters.ViewPlatformDs;
            mainWindow.PlatNESToolStripMenuItem.Checked = model.Filters.ViewPlatformNes;
            mainWindow.PlatSNESToolStripMenuItem.Checked = model.Filters.ViewPlatformSnes;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model" />
        /// <param name="mainWindow"></param>
        internal static void AllRatings(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewRating5 = true;
            model.Filters.ViewRating4 = true;
            model.Filters.ViewRating3 = true;
            model.Filters.ViewRating2 = true;
            model.Filters.ViewRating1 = true;

            mainWindow.Rating5ToolStripMenuItem.Checked = model.Filters.ViewRating5;
            mainWindow.Rating4ToolStripMenuItem.Checked = model.Filters.ViewRating4;
            mainWindow.Rating3ToolStripMenuItem.Checked = model.Filters.ViewRating3;
            mainWindow.Rating2ToolStripMenuItem.Checked = model.Filters.ViewRating2;
            mainWindow.Rating1ToolStripMenuItem.Checked = model.Filters.ViewRating1;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="mainWindow"></param>
        internal static void NoRatings(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewRating5 = false;
            model.Filters.ViewRating4 = false;
            model.Filters.ViewRating3 = false;
            model.Filters.ViewRating2 = false;
            model.Filters.ViewRating1 = false;

            mainWindow.Rating5ToolStripMenuItem.Checked = model.Filters.ViewRating5;
            mainWindow.Rating4ToolStripMenuItem.Checked = model.Filters.ViewRating4;
            mainWindow.Rating3ToolStripMenuItem.Checked = model.Filters.ViewRating3;
            mainWindow.Rating2ToolStripMenuItem.Checked = model.Filters.ViewRating2;
            mainWindow.Rating1ToolStripMenuItem.Checked = model.Filters.ViewRating1;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="mainWindow"></param>
        internal static void AllStatus(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewStatusPerfect = true;
            model.Filters.ViewStatusPlayable = true;
            model.Filters.ViewStatusRuns = true;
            model.Filters.ViewStatusLoads = true;
            model.Filters.ViewStatusUnplayable = true;
            model.Filters.ViewStatusNotSet = true;

            mainWindow.PerfectToolStripMenuItem.Checked = model.Filters.ViewStatusPerfect;
            mainWindow.PlayableToolStripMenuItem.Checked = model.Filters.ViewStatusPlayable;
            mainWindow.RunsToolStripMenuItem.Checked = model.Filters.ViewStatusRuns;
            mainWindow.LoadsToolStripMenuItem.Checked = model.Filters.ViewStatusLoads;
            mainWindow.UnplayableToolStripMenuItem.Checked = model.Filters.ViewStatusUnplayable;
            mainWindow.NotSetToolStripMenuItem.Checked = model.Filters.ViewStatusNotSet;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="mainWindow"></param>
        internal static void AllDecafStatus(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewDecafStatusPerfect = true;
            model.Filters.ViewDecafStatusPlayable = true;
            model.Filters.ViewDecafStatusRuns = true;
            model.Filters.ViewDecafStatusLoads = true;
            model.Filters.ViewDecafStatusUnplayable = true;
            model.Filters.ViewDecafStatusNotSet = true;

            mainWindow.DecafPerfectToolStripMenuItem.Checked = model.Filters.ViewDecafStatusPerfect;
            mainWindow.DecafPlayableToolStripMenuItem.Checked = model.Filters.ViewDecafStatusPlayable;
            mainWindow.DecafRunsToolStripMenuItem.Checked = model.Filters.ViewDecafStatusRuns;
            mainWindow.DecafLoadsToolStripMenuItem.Checked = model.Filters.ViewDecafStatusLoads;
            mainWindow.DecafUnplayableToolStripMenuItem.Checked = model.Filters.ViewDecafStatusUnplayable;
            mainWindow.DecafNotSetToolStripMenuItem.Checked = model.Filters.ViewDecafStatusNotSet;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="mainWindow"></param>
        internal static void NoDecafStatus(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewDecafStatusPerfect = false;
            model.Filters.ViewDecafStatusPlayable = false;
            model.Filters.ViewDecafStatusRuns = false;
            model.Filters.ViewDecafStatusLoads = false;
            model.Filters.ViewDecafStatusUnplayable = false;
            model.Filters.ViewDecafStatusNotSet = false;

            mainWindow.DecafPerfectToolStripMenuItem.Checked = model.Filters.ViewDecafStatusPerfect;
            mainWindow.DecafPlayableToolStripMenuItem.Checked = model.Filters.ViewDecafStatusPlayable;
            mainWindow.DecafRunsToolStripMenuItem.Checked = model.Filters.ViewDecafStatusRuns;
            mainWindow.DecafLoadsToolStripMenuItem.Checked = model.Filters.ViewDecafStatusLoads;
            mainWindow.DecafUnplayableToolStripMenuItem.Checked = model.Filters.ViewDecafStatusUnplayable;
            mainWindow.DecafNotSetToolStripMenuItem.Checked = model.Filters.ViewDecafStatusNotSet;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="mainWindow"></param>
        internal static void NoStatus(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewStatusPerfect = false;
            model.Filters.ViewStatusPlayable = false;
            model.Filters.ViewStatusRuns = false;
            model.Filters.ViewStatusLoads = false;
            model.Filters.ViewStatusUnplayable = false;
            model.Filters.ViewStatusNotSet = false;

            mainWindow.PerfectToolStripMenuItem.Checked = model.Filters.ViewStatusPerfect;
            mainWindow.PlayableToolStripMenuItem.Checked = model.Filters.ViewStatusPlayable;
            mainWindow.RunsToolStripMenuItem.Checked = model.Filters.ViewStatusRuns;
            mainWindow.LoadsToolStripMenuItem.Checked = model.Filters.ViewStatusLoads;
            mainWindow.UnplayableToolStripMenuItem.Checked = model.Filters.ViewStatusUnplayable;
            mainWindow.NotSetToolStripMenuItem.Checked = model.Filters.ViewStatusNotSet;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void UpdateMenuItemChecks(Model model, FormMainWindow mainWindow)
        {
            mainWindow.UsaToolStripMenuItem.Checked = model.Filters.ViewRegionUsa;
            mainWindow.EuropeToolStripMenuItem.Checked = model.Filters.ViewRegionEur;

            mainWindow.WiiUToolStripMenuItem.Checked = model.Filters.ViewTypeWiiU;
            mainWindow.EShopToolStripMenuItem.Checked = model.Filters.ViewTypeEshop;
            mainWindow.ChannelToolStripMenuItem.Checked = model.Filters.ViewTypeChannel;
            mainWindow.VirtualConsoleToolStripMenuItem.Checked = model.Filters.ViewTypeVc;

            mainWindow.PlatWiiUToolStripMenuItem.Checked = model.Filters.ViewPlatformWiiU;
            mainWindow.PlatHtml5ToolStripMenuItem.Checked = model.Filters.ViewPlatformHtml5;
            mainWindow.PlatNintendo64ToolStripMenuItem.Checked = model.Filters.ViewPlatformDs;
            mainWindow.PlatNintendoDsToolStripMenuItem.Checked = model.Filters.ViewPlatformN64;
            mainWindow.PlatNESToolStripMenuItem.Checked = model.Filters.ViewPlatformNes;
            mainWindow.PlatSNESToolStripMenuItem.Checked = model.Filters.ViewPlatformSnes;

            mainWindow.Rating5ToolStripMenuItem.Checked = model.Filters.ViewRating5;
            mainWindow.Rating4ToolStripMenuItem.Checked = model.Filters.ViewRating4;
            mainWindow.Rating3ToolStripMenuItem.Checked = model.Filters.ViewRating3;
            mainWindow.Rating2ToolStripMenuItem.Checked = model.Filters.ViewRating2;
            mainWindow.Rating1ToolStripMenuItem.Checked = model.Filters.ViewRating1;

            mainWindow.PerfectToolStripMenuItem.Checked = model.Filters.ViewStatusPerfect;
            mainWindow.PlayableToolStripMenuItem.Checked = model.Filters.ViewStatusPlayable;
            mainWindow.RunsToolStripMenuItem.Checked = model.Filters.ViewStatusRuns;
            mainWindow.LoadsToolStripMenuItem.Checked = model.Filters.ViewStatusLoads;
            mainWindow.UnplayableToolStripMenuItem.Checked = model.Filters.ViewStatusUnplayable;
            mainWindow.NotSetToolStripMenuItem.Checked = model.Filters.ViewStatusNotSet;

            mainWindow.DecafPerfectToolStripMenuItem.Checked = model.Filters.ViewDecafStatusPerfect;
            mainWindow.DecafPlayableToolStripMenuItem.Checked = model.Filters.ViewDecafStatusPlayable;
            mainWindow.DecafRunsToolStripMenuItem.Checked = model.Filters.ViewDecafStatusRuns;
            mainWindow.DecafLoadsToolStripMenuItem.Checked = model.Filters.ViewDecafStatusLoads;
            mainWindow.DecafUnplayableToolStripMenuItem.Checked = model.Filters.ViewDecafStatusUnplayable;
            mainWindow.DecafNotSetToolStripMenuItem.Checked = model.Filters.ViewDecafStatusNotSet;

            mainWindow.OfficiallyPerfectToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusPerfect;
            mainWindow.OfficiallyPlayableToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusPlayable;
            mainWindow.OfficiallyRunsToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusRuns;
            mainWindow.OfficiallyLoadsToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusLoads;
            mainWindow.OfficiallyUnplayableToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusUnplayable;
            mainWindow.OfficiallyNotSetToolStripMenuItem.Checked = model.Filters.ViewOfficialStatusNotSet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="mainWindow"></param>
        internal static void UpdateFiltersItems(Model model, FormMainWindow mainWindow)
        {
            model.Filters.ViewRegionUsa = mainWindow.UsaToolStripMenuItem.Checked;
            model.Filters.ViewRegionEur = mainWindow.EuropeToolStripMenuItem.Checked;
            model.Filters.ViewRegionJap = mainWindow.JapanToolStripMenuItem.Checked;

            model.Filters.ViewPlatformWiiU = mainWindow.PlatWiiUToolStripMenuItem.Checked;
            model.Filters.ViewPlatformHtml5 = mainWindow.PlatHtml5ToolStripMenuItem.Checked;
            model.Filters.ViewPlatformN64 = mainWindow.PlatNintendo64ToolStripMenuItem.Checked;
            model.Filters.ViewPlatformDs = mainWindow.PlatNintendoDsToolStripMenuItem.Checked;
            model.Filters.ViewPlatformNes = mainWindow.PlatNESToolStripMenuItem.Checked;
            model.Filters.ViewPlatformSnes = mainWindow.PlatSNESToolStripMenuItem.Checked;

            model.Filters.ViewTypeWiiU = mainWindow.WiiUToolStripMenuItem.Checked;
            model.Filters.ViewTypeEshop = mainWindow.EShopToolStripMenuItem.Checked;
            model.Filters.ViewTypeChannel = mainWindow.ChannelToolStripMenuItem.Checked;
            model.Filters.ViewTypeVc = mainWindow.VirtualConsoleToolStripMenuItem.Checked;

            model.Filters.ViewRating5 = mainWindow.Rating5ToolStripMenuItem.Checked;
            model.Filters.ViewRating4 = mainWindow.Rating4ToolStripMenuItem.Checked;
            model.Filters.ViewRating3 = mainWindow.Rating3ToolStripMenuItem.Checked;
            model.Filters.ViewRating2 = mainWindow.Rating2ToolStripMenuItem.Checked;
            model.Filters.ViewRating1 = mainWindow.Rating1ToolStripMenuItem.Checked;

            model.Filters.ViewStatusPerfect = mainWindow.PerfectToolStripMenuItem.Checked;
            model.Filters.ViewStatusPlayable = mainWindow.PlayableToolStripMenuItem.Checked;
            model.Filters.ViewStatusRuns = mainWindow.RunsToolStripMenuItem.Checked;
            model.Filters.ViewStatusLoads = mainWindow.LoadsToolStripMenuItem.Checked;
            model.Filters.ViewStatusUnplayable = mainWindow.UnplayableToolStripMenuItem.Checked;
            model.Filters.ViewStatusNotSet = mainWindow.NotSetToolStripMenuItem.Checked;

            model.Filters.ViewDecafStatusPerfect = mainWindow.DecafPerfectToolStripMenuItem.Checked;
            model.Filters.ViewDecafStatusPlayable = mainWindow.DecafPlayableToolStripMenuItem.Checked;
            model.Filters.ViewDecafStatusRuns = mainWindow.DecafRunsToolStripMenuItem.Checked;
            model.Filters.ViewDecafStatusLoads = mainWindow.DecafLoadsToolStripMenuItem.Checked;
            model.Filters.ViewDecafStatusUnplayable = mainWindow.DecafUnplayableToolStripMenuItem.Checked;
            model.Filters.ViewDecafStatusNotSet = mainWindow.DecafNotSetToolStripMenuItem.Checked;

            model.Filters.ViewOfficialStatusPerfect = mainWindow.OfficiallyPerfectToolStripMenuItem.Checked;
            model.Filters.ViewOfficialStatusPlayable = mainWindow.OfficiallyPlayableToolStripMenuItem.Checked;
            model.Filters.ViewOfficialStatusRuns = mainWindow.OfficiallyRunsToolStripMenuItem.Checked;
            model.Filters.ViewOfficialStatusLoads = mainWindow.OfficiallyLoadsToolStripMenuItem.Checked;
            model.Filters.ViewOfficialStatusUnplayable = mainWindow.OfficiallyUnplayableToolStripMenuItem.Checked;
            model.Filters.ViewOfficialStatusNotSet = mainWindow.OfficiallyNotSetToolStripMenuItem.Checked;

            mainWindow.PopulateListView();
        }
    }
}
