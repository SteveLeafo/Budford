using Budford.View;
using System.Windows.Forms;
namespace Budford.Model
{
    public class ViewFilters
    {
        // Region
        public bool ViewRegionUsa  = true;
        public bool ViewRegionEur  = true;
        public bool ViewRegionJap  = true;
        public bool ViewRegionAll = true;

        // Type
        public bool ViewTypeWiiU = true;
        public bool ViewTypeEshop = true;
        public bool ViewTypeChannel = true;
        public bool ViewTypeVc = true;

        // Status
        public bool ViewStatusNotSet  = true;
        public bool ViewStatusPerfect  = true;
        public bool ViewStatusPlayable  = true;
        public bool ViewStatusRuns  = true;
        public bool ViewStatusLoads = true;
        public bool ViewStatusUnplayable  = true;

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

        internal static void OfficiallyAll(Model Model, FormMainWindow mainWindow)
        {
            Model.Filters.ViewOfficialStatusPerfect = true;
            Model.Filters.ViewOfficialStatusPlayable = true;
            Model.Filters.ViewOfficialStatusRuns = true;
            Model.Filters.ViewOfficialStatusLoads = true;
            Model.Filters.ViewOfficialStatusUnplayable = true;
            Model.Filters.ViewOfficialStatusNotSet = true;

            mainWindow.OfficiallyPerfectToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusPerfect;
            mainWindow.OfficiallyPlayableToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusPlayable;
            mainWindow.OfficiallyRunsToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusRuns;
            mainWindow.OfficiallyLoadsToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusLoads;
            mainWindow.OfficiallyUnplayableToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusUnplayable;
            mainWindow.OfficiallyNotSetToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusNotSet;

            mainWindow.PopulateListView();
        }

        internal static void OfficiallyNone(Model Model, FormMainWindow mainWindow)
        {
            Model.Filters.ViewOfficialStatusPerfect = false;
            Model.Filters.ViewOfficialStatusPlayable = false;
            Model.Filters.ViewOfficialStatusRuns = false;
            Model.Filters.ViewOfficialStatusLoads = false;
            Model.Filters.ViewOfficialStatusUnplayable = false;
            Model.Filters.ViewOfficialStatusNotSet = false;

            mainWindow.OfficiallyPerfectToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusPerfect;
            mainWindow.OfficiallyPlayableToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusPlayable;
            mainWindow.OfficiallyRunsToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusRuns;
            mainWindow.OfficiallyLoadsToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusLoads;
            mainWindow.OfficiallyUnplayableToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusUnplayable;
            mainWindow.OfficiallyNotSetToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusNotSet;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void AllRegions(Model Model, FormMainWindow mainWindow)
        {
            Model.Filters.ViewRegionUsa = true;
            Model.Filters.ViewRegionEur = true;
            Model.Filters.ViewRegionJap = true;

            mainWindow.UsaToolStripMenuItem.Checked = Model.Filters.ViewRegionUsa;
            mainWindow.EuropeToolStripMenuItem.Checked = Model.Filters.ViewRegionEur;
            mainWindow.JapanToolStripMenuItem.Checked = Model.Filters.ViewRegionJap;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void NoRegions(Model Model, FormMainWindow mainWindow)
        {
            Model.Filters.ViewRegionUsa = false;
            Model.Filters.ViewRegionEur = false;
            Model.Filters.ViewRegionJap = false;

            mainWindow.UsaToolStripMenuItem.Checked = Model.Filters.ViewRegionUsa;
            mainWindow.EuropeToolStripMenuItem.Checked = Model.Filters.ViewRegionEur;
            mainWindow.JapanToolStripMenuItem.Checked = Model.Filters.ViewRegionJap;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void AllTypes(Model Model, FormMainWindow mainWindow)
        {
            Model.Filters.ViewTypeWiiU = true;
            Model.Filters.ViewTypeEshop = true;
            Model.Filters.ViewTypeChannel = true;
            Model.Filters.ViewTypeVc = true;

            mainWindow.WiiUToolStripMenuItem.Checked = Model.Filters.ViewTypeWiiU;
            mainWindow.EShopToolStripMenuItem.Checked = Model.Filters.ViewTypeEshop;
            mainWindow.ChannelToolStripMenuItem.Checked = Model.Filters.ViewTypeChannel;
            mainWindow.VirtualConsoleToolStripMenuItem.Checked = Model.Filters.ViewTypeVc;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void NoTypes(Model Model, FormMainWindow mainWindow)
        {
            Model.Filters.ViewTypeWiiU = false;
            Model.Filters.ViewTypeEshop = false;
            Model.Filters.ViewTypeChannel = false;
            Model.Filters.ViewTypeVc = false;

            mainWindow.WiiUToolStripMenuItem.Checked = Model.Filters.ViewTypeWiiU;
            mainWindow.EShopToolStripMenuItem.Checked = Model.Filters.ViewTypeEshop;
            mainWindow.ChannelToolStripMenuItem.Checked = Model.Filters.ViewTypeChannel;
            mainWindow.VirtualConsoleToolStripMenuItem.Checked = Model.Filters.ViewTypeVc;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void AllRatings(Model Model, FormMainWindow mainWindow)
        {
            Model.Filters.ViewRating5 = true;
            Model.Filters.ViewRating4 = true;
            Model.Filters.ViewRating3 = true;
            Model.Filters.ViewRating2 = true;
            Model.Filters.ViewRating1 = true;

            mainWindow.Rating5ToolStripMenuItem.Checked = Model.Filters.ViewRating5;
            mainWindow.Rating4ToolStripMenuItem.Checked = Model.Filters.ViewRating4;
            mainWindow.Rating3ToolStripMenuItem.Checked = Model.Filters.ViewRating3;
            mainWindow.Rating2ToolStripMenuItem.Checked = Model.Filters.ViewRating2;
            mainWindow.Rating1ToolStripMenuItem.Checked = Model.Filters.ViewRating1;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void NoRatings(Model Model, FormMainWindow mainWindow)
        {
            Model.Filters.ViewRating5 = false;
            Model.Filters.ViewRating4 = false;
            Model.Filters.ViewRating3 = false;
            Model.Filters.ViewRating2 = false;
            Model.Filters.ViewRating1 = false;

            mainWindow.Rating5ToolStripMenuItem.Checked = Model.Filters.ViewRating5;
            mainWindow.Rating4ToolStripMenuItem.Checked = Model.Filters.ViewRating4;
            mainWindow.Rating3ToolStripMenuItem.Checked = Model.Filters.ViewRating3;
            mainWindow.Rating2ToolStripMenuItem.Checked = Model.Filters.ViewRating2;
            mainWindow.Rating1ToolStripMenuItem.Checked = Model.Filters.ViewRating1;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void AllStatus(Model Model, FormMainWindow mainWindow)
        {
            Model.Filters.ViewStatusPerfect = true;
            Model.Filters.ViewStatusPlayable = true;
            Model.Filters.ViewStatusRuns = true;
            Model.Filters.ViewStatusLoads = true;
            Model.Filters.ViewStatusUnplayable = true;
            Model.Filters.ViewStatusNotSet = true;

            mainWindow.PerfectToolStripMenuItem.Checked = Model.Filters.ViewStatusPerfect;
            mainWindow.PlayableToolStripMenuItem.Checked = Model.Filters.ViewStatusPlayable;
            mainWindow.RunsToolStripMenuItem.Checked = Model.Filters.ViewStatusRuns;
            mainWindow.LoadsToolStripMenuItem.Checked = Model.Filters.ViewStatusLoads;
            mainWindow.UnplayableToolStripMenuItem.Checked = Model.Filters.ViewStatusUnplayable;
            mainWindow.NotSetToolStripMenuItem.Checked = Model.Filters.ViewStatusNotSet;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void NoStatus(Model Model, FormMainWindow mainWindow)
        {
            Model.Filters.ViewStatusPerfect = false;
            Model.Filters.ViewStatusPlayable = false;
            Model.Filters.ViewStatusRuns = false;
            Model.Filters.ViewStatusLoads = false;
            Model.Filters.ViewStatusUnplayable = false;
            Model.Filters.ViewStatusNotSet = false;

            mainWindow.PerfectToolStripMenuItem.Checked = Model.Filters.ViewStatusPerfect;
            mainWindow.PlayableToolStripMenuItem.Checked = Model.Filters.ViewStatusPlayable;
            mainWindow.RunsToolStripMenuItem.Checked = Model.Filters.ViewStatusRuns;
            mainWindow.LoadsToolStripMenuItem.Checked = Model.Filters.ViewStatusLoads;
            mainWindow.UnplayableToolStripMenuItem.Checked = Model.Filters.ViewStatusUnplayable;
            mainWindow.NotSetToolStripMenuItem.Checked = Model.Filters.ViewStatusNotSet;

            mainWindow.PopulateListView();
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void UpdateMenuItemChecks(Model Model, FormMainWindow mainWindow)
        {
            mainWindow.UsaToolStripMenuItem.Checked = Model.Filters.ViewRegionUsa;
            mainWindow.EuropeToolStripMenuItem.Checked = Model.Filters.ViewRegionEur;

            mainWindow.WiiUToolStripMenuItem.Checked = Model.Filters.ViewTypeWiiU;
            mainWindow.EShopToolStripMenuItem.Checked = Model.Filters.ViewTypeEshop;
            mainWindow.ChannelToolStripMenuItem.Checked = Model.Filters.ViewTypeChannel;
            mainWindow.VirtualConsoleToolStripMenuItem.Checked = Model.Filters.ViewTypeVc;
            
            mainWindow.Rating5ToolStripMenuItem.Checked = Model.Filters.ViewRating5;
            mainWindow.Rating4ToolStripMenuItem.Checked = Model.Filters.ViewRating4;
            mainWindow.Rating3ToolStripMenuItem.Checked = Model.Filters.ViewRating3;
            mainWindow.Rating2ToolStripMenuItem.Checked = Model.Filters.ViewRating2;
            mainWindow.Rating1ToolStripMenuItem.Checked = Model.Filters.ViewRating1;
            
            mainWindow.PerfectToolStripMenuItem.Checked = Model.Filters.ViewStatusPerfect;
            mainWindow.PlayableToolStripMenuItem.Checked = Model.Filters.ViewStatusPlayable;
            mainWindow.RunsToolStripMenuItem.Checked = Model.Filters.ViewStatusRuns;
            mainWindow.LoadsToolStripMenuItem.Checked = Model.Filters.ViewStatusLoads;
            mainWindow.UnplayableToolStripMenuItem.Checked = Model.Filters.ViewStatusUnplayable;
            mainWindow.NotSetToolStripMenuItem.Checked = Model.Filters.ViewStatusNotSet;
            
            mainWindow.OfficiallyPerfectToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusPerfect;
            mainWindow.OfficiallyPlayableToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusPlayable;
            mainWindow.OfficiallyRunsToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusRuns;
            mainWindow.OfficiallyLoadsToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusLoads;
            mainWindow.OfficiallyUnplayableToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusUnplayable;
            mainWindow.OfficiallyNotSetToolStripMenuItem.Checked = Model.Filters.ViewOfficialStatusNotSet;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void UpdateFiltersItems(Model Model, FormMainWindow mainWindow)
        {
            Model.Filters.ViewRegionUsa = mainWindow.UsaToolStripMenuItem.Checked;
            Model.Filters.ViewRegionEur = mainWindow.EuropeToolStripMenuItem.Checked;
            Model.Filters.ViewRegionJap = mainWindow.JapanToolStripMenuItem.Checked;

            Model.Filters.ViewTypeWiiU = mainWindow.WiiUToolStripMenuItem.Checked;
            Model.Filters.ViewTypeEshop = mainWindow.EShopToolStripMenuItem.Checked;
            Model.Filters.ViewTypeChannel = mainWindow.ChannelToolStripMenuItem.Checked;
            Model.Filters.ViewTypeVc = mainWindow.VirtualConsoleToolStripMenuItem.Checked;

            Model.Filters.ViewRating5 = mainWindow.Rating5ToolStripMenuItem.Checked;
            Model.Filters.ViewRating4 = mainWindow.Rating4ToolStripMenuItem.Checked;
            Model.Filters.ViewRating3 = mainWindow.Rating3ToolStripMenuItem.Checked;
            Model.Filters.ViewRating2 = mainWindow.Rating2ToolStripMenuItem.Checked;
            Model.Filters.ViewRating1 = mainWindow.Rating1ToolStripMenuItem.Checked;

            Model.Filters.ViewStatusPerfect = mainWindow.PerfectToolStripMenuItem.Checked;
            Model.Filters.ViewStatusPlayable = mainWindow.PlayableToolStripMenuItem.Checked;
            Model.Filters.ViewStatusRuns = mainWindow.RunsToolStripMenuItem.Checked;
            Model.Filters.ViewStatusLoads = mainWindow.LoadsToolStripMenuItem.Checked;
            Model.Filters.ViewStatusUnplayable = mainWindow.UnplayableToolStripMenuItem.Checked;
            Model.Filters.ViewStatusNotSet = mainWindow.NotSetToolStripMenuItem.Checked;

            Model.Filters.ViewOfficialStatusPerfect = mainWindow.OfficiallyPerfectToolStripMenuItem.Checked;
            Model.Filters.ViewOfficialStatusPlayable = mainWindow.OfficiallyPlayableToolStripMenuItem.Checked;
            Model.Filters.ViewOfficialStatusRuns = mainWindow.OfficiallyRunsToolStripMenuItem.Checked;
            Model.Filters.ViewOfficialStatusLoads = mainWindow.OfficiallyLoadsToolStripMenuItem.Checked;
            Model.Filters.ViewOfficialStatusUnplayable = mainWindow.OfficiallyUnplayableToolStripMenuItem.Checked;
            Model.Filters.ViewOfficialStatusNotSet = mainWindow.OfficiallyNotSetToolStripMenuItem.Checked;

            mainWindow.PopulateListView();
        }   
    }
}
