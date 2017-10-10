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
    }
}
