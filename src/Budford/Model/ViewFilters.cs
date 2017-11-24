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
        public bool ViewStatusLoads  = true;
        public bool ViewStatusUnplayable  = true;

        // Official Status
        public bool ViewOfficialStatusNotSet = true;
        public bool ViewOfficialStatusPerfect = true;
        public bool ViewOfficialStatusPlayable = true;
        public bool ViewOfficialStatusRuns = true;
        public bool ViewOfficialStatusLoads = true;
        public bool ViewOfficialStatusUnplayable = true;
    }
}
