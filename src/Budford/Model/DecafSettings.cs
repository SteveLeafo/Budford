﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budford.Model
{
    public class DecafSettings
    {
        public bool Enable = false;
        public bool Sound = true;
        public bool Logging = false;

        public int WindowMode = 0;
        public int Layout = 0;

        public int Input = 0;
        public int Input0 = 2;

        public int Input1 = 2;
        public int Input2 = 2;
        public int Input3 = 2;
        public int Input4 = 2;

        public int Backend = 1;

        public string Executable = "";
        public string MlcPath = "mlc";
        public string SlcPath = "slc";
        public string ResourcesPath = "resources";
    }
}
