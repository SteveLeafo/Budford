using System.Collections.Generic;

namespace Budford.Model.PlugIns
{
    public class PlugIn
    {
        public string Name;
        public string Type;
        public string FileName;
        public List<File> Files = new List<File>();
    }
}
