using System.Text.RegularExpressions;
namespace Budford.Model
{
    public class InstalledVersion
    {
        public string Name  = "";
        public string Folder  = "";
        public string Version  = "";
        public bool HasFonts;
        public bool HasCemuHook ;
        public bool HasPatch ;
        public bool HasDlc ;
        public int DlcType ;
        public bool IsLatest;
        internal string DlcSource = "Local";

        internal int VersionNumber
        {
            get            
            {
                string currentCemuVersion = Regex.Replace(Version, "[A-Za-z ]", "").Replace("_", "");
                int version;
                if (int.TryParse(currentCemuVersion.Replace(".", "").Replace("Cemu_", ""), out version))
                {
                    return version;
                }
                return -1;
            }
        }

        static internal int GetVersionNumber(string versionIn)
        {
            string currentCemuVersion = Regex.Replace(versionIn, "[A-Za-z ]", "").Replace("_", "");
            int version;
            if (int.TryParse(currentCemuVersion.Replace(".", "").Replace("Cemu_", ""), out version))
            {
                return version;
            }
            return -1;
        }
    }
}
