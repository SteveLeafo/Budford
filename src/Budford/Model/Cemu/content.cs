using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budford.Model.Cemu
{
    public class content
    {
        //public content()
        //{
        //    GraphicPack.Add(new Entry() { filename = "Steve", preset = "Lever" });
        //    GraphicPack.Add(new Entry() { filename = "Karyn", preset = "Halls" });
        //}

        public string mlc_path = string.Empty;
        public int language = 0;
        public bool use_discord_presence = false;
        public bool fullscreen_menubar = false;
        public List<Entry> GraphicPack = new List<Entry>();
        public Graphic Graphic = new Graphic();
        public Audio Audio = new Audio();
    }
}
