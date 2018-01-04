using Budford.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budford.Control
{
    public class WriteCemuHookIniFile
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="gameInfo"></param>
        public static void WriteIni(Model.Model model, GameInformation gameInfo)
        {
            using (StreamWriter sw = new StreamWriter(GetVersion(model, gameInfo).Folder + "\\cemuhook.ini"))
            {
                sw.WriteLine("[CPU]");
                sw.WriteLine("customTimerMode = " + GetCustomTimerMode(gameInfo.CemuHookSetting.CustomTimerMode));
                sw.WriteLine("customTimerMultiplier = " + GetCustomTimerMultiplier(gameInfo.CemuHookSetting.CustomTimerMultiplier));
                sw.WriteLine("disableLZCNT = " + (gameInfo.CemuHookSetting.DisableLZCNT ? "true" : "false"));
                sw.WriteLine("disableMOVBE =  " + (gameInfo.CemuHookSetting.DisableLZCNT ? "true" : "false"));
                sw.WriteLine("disableAVX =  " + (gameInfo.CemuHookSetting.DisableLZCNT ? "true" : "false"));
                sw.WriteLine("[Input]");
                sw.WriteLine("motionSource = " + GetMotionSource(gameInfo.CemuHookSetting.MotionSource));
                sw.WriteLine("[Debug]");
                sw.WriteLine("mmTimerAccuracy = " + GetMMTimerAccuracy(gameInfo.CemuHookSetting.MotionSource));
                sw.WriteLine("[Graphics]");
                sw.WriteLine("ignorePrecompiledShaderCache = true");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        static string GetCustomTimerMode(int mode)
        {
            switch (mode)
            {
                case 0: return "none";
                case 1: return "QPC";
                case 2: return "RDTSC";
            }
            return "none";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        static string GetCustomTimerMultiplier(int mode)
        {
            switch (mode)
            {
                case 0: return "1";
                case 1: return "2";
                case 2: return "4";
                case 3: return "8";
                case 4: return "0.5";
                case 5: return "0.25";
                case 6: return "0.125";
            }
            return "1";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        static string GetMotionSource(int mode)
        {
            switch (mode)
            {
                case 0: return "DSU1";
                case 1: return "DSU2";
                case 2: return "DSU3";
                case 3: return "DSU4";
            }
            return "DSU1";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        static string GetMMTimerAccuracy(int mode)
        {
            switch (mode)
            {
                case 0: return "default";
                case 1: return "max";
                case 2: return "1";
            }
            return "default";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static InstalledVersion GetVersion(Model.Model model, GameInformation information)
        {
            InstalledVersion version = null;
            if (information.GameSetting != null)
            {
                if (information.GameSetting.PreferedVersion == "Latest")
                {
                    version = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
                }
                else
                {
                    version = model.Settings.InstalledVersions.FirstOrDefault(v => v.Name == information.GameSetting.PreferedVersion);
                }
            }
            return version;
        }
    }
}
