using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budford.Tools
{
    public abstract class Tool
    {
        string Name { get; set; }

        protected Tool(Model.Model model)
        {
            Model = model;
        }

        protected Model.Model Model { get; set; }

        public abstract bool Execute();
    }
}
