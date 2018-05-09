using System.Collections.Generic;

namespace Budford.Model
{
    public class GameSettings
    {
        public string PreferedVersion  = "Latest";

        public EmulationStateType EmulationState = EmulationStateType.NotSet;
        public EmulationStateType OfficialEmulationState = EmulationStateType.NotSet;
        internal EmulationStateType PreviousOfficialEmulationState;
        public string CompatibilityUrl = "";
        // ReSharper disable once InconsistentNaming
        public HashSet<GraphicsPack> graphicsPacks = new HashSet<GraphicsPack>();
        internal HashSet<string> GraphicsPacksFolders = new HashSet<string>();

        // CEMU Options
        public byte FullScreen  = 1;
        public byte EnableVSync  = 0;
        public UpscaleFilterType UpscaleFilter  = UpscaleFilterType.Bicubic;
        public byte UseSeperableShaders = 1;
        public FullScreenScalingType FullScreenScaling  = FullScreenScalingType.KeepAspectRatio;
        public GpuBufferCacheAccuracyType GpuBufferCacheAccuracy  = GpuBufferCacheAccuracyType.High;

        // CPU
        public CpuModeType CpuMode  = CpuModeType.SingleCoreCompiler;
        public CpuTimerType CpuTimer  = CpuTimerType.HostBasedTimer;
        
        // Graphic Pack Settings
        public int ClarityPreset = 3;
        public int UseCafeLibs = 0;

        // Steve settings

        // 0 = Do nothing, 1 = Wii U GamePad, 2 = Wii U Pro Controller, 3 = Wii U Classic Controller, 4 = Wiimote, 5 = Disable
        public int ControllerOverride1 = 0; 
        public int ControllerOverride2 = 0; 
        public int ControllerOverride3 = 0; 
        public int ControllerOverride4 = 0; 
        public int ControllerOverride5 = 0; 
        public int ControllerOverride6 = 0; 
        public int ControllerOverride7 = 0; 
        public int ControllerOverride8 = 0;

        // 0 = Don't Swap, 1 = Swap A+B, 2 = Swap X+Y, 3 = Swap Both
        public int SwapButtons1 = 0;
        public int SwapButtons2 = 0;
        public int SwapButtons3 = 0;
        public int SwapButtons4 = 0;
        public int SwapButtons5 = 0;
        public int SwapButtons6 = 0;
        public int SwapButtons7 = 0;
        public int SwapButtons8 = 0; 

        public int Fps = 30;
        public bool OverrideFps = false;
        public bool DeleteShaderCache = false;

        public byte DebugGx2ApiOffset;
        public byte DebugUnsupportedApiCallsOffset ;
        public byte DebugThreadSynchronisationApiOffset ;
        public byte DebugAudioApiOffset ;
        public byte DebugInputApiOffset ;

        public byte EnableDebugOffset;

        public byte Volume = 0x1E;


        // CEMU Hook
        public CpuAffinityType CpuAffinity  = CpuAffinityType.AllLogicalCores;

        public byte RenderUpsideDown ;
        public byte DisableAudio ;
        public byte EnableBoTwCrashWorkaround ;
        public byte FullSyncAtGx2DrawDone ;
        public byte SeparateGamePadView  = 0;
        public byte AccaccurateShaderMul = 1;
        public byte DisableGpuFence ;
        public byte EmulateSinglePrecision = 1;
        public byte UseRtdsc = 1;
        public byte Online = 0;
        public byte DefaultView = 0;

        public enum GpuBufferCacheAccuracyType : byte
        {
            Low = 2,
            Medium = 1,
            High = 0
        }

        public enum FullScreenScalingType : byte
        {
            KeepAspectRatio = 0,
            Stretch = 1
        }

        public enum UpscaleFilterType : byte
        {
            Bilinear = 0,
            Bicubic = 1
        }

        public enum CpuModeType : byte
        {
            SingleCoreInterpreter = 0,
            SingleCoreCompiler = 1,
            DualCoreCompiler = 2,
            TripleCoreCompiler = 3
        }

        public enum CpuTimerType : byte
        {
            CycleBasedTimer = 0,
            HostBasedTimer = 1
        }

        public enum CpuAffinityType : byte
        {
            AllLogicalCores = 0,
            FirstLogicalCodePerPhysicaCore = 1,
            LastLogicalCodePerPhysicaCore = 2
        }

        public enum EmulationStateType : byte
        {
            NotSet = 0,
            Perfect = 1,
            Playable = 2,
            Runs = 3,
            Loads = 4,
            Unplayable = 5
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        internal static int GetStatusImageIndex(string status)
        {
            int imageIndex = 5;
            switch (status)
            {
                case "Perfect":
                    imageIndex = 0;
                    break;
                case "Playable":
                    imageIndex = 1;
                    break;
                case "Runs":
                    imageIndex = 2;
                    break;
                case "Loads":
                    imageIndex = 3;
                    break;
                case "Unplayable":
                    imageIndex = 4;
                    break;
            }

            return imageIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static int GetRegionImageIndex(KeyValuePair<string, GameInformation> game)
        {
            switch (game.Value.Region)
            {
                case "EUR": return 0;
                case "JPN": return 1;
                case "USA": return 2;
                default: return 4;
            }
        }

        internal static bool IsPlayable(EmulationStateType emulationState)
        {
            if (emulationState != EmulationStateType.NotSet)
            {
                if (emulationState != EmulationStateType.Loads)
                {
                    if (emulationState != EmulationStateType.Unplayable)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
