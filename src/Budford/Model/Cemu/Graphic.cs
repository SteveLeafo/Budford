using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budford.Model.Cemu
{
    public class Graphic
    {        
        public bool VSync = false;
        public bool GX2DrawdoneSync = true;
        public bool SeparableShaders = true;
        public bool DisablePrecompiledShaders = false;
        public int UpscaleFilter = 1;
        public int FullscreenScaling = 0;   
    }
}
