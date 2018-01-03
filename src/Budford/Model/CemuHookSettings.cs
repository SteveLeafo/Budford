using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budford.Model
{
    public class CemuHookSettings
    {
        public int CustomTimerMode = 0;
        public int CustomTimerMultiplier = 0;
        public bool DisableLZCNT = false;
        public bool DisableMOVBE = false;
        public bool DisableAVX = false;
        public int MotionSource = 0;
        public int MMTimerAccuracy = 0;
    }
}
