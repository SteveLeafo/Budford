using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
