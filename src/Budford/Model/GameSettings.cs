using System.Collections.Generic;

namespace Budford.Model
{
    public class GameSettings
    {
        public string PreferedVersion  = "Latest";

        public EmulationStateType EmulationState = EmulationStateType.NotSet;
        public EmulationStateType OfficialEmulationState = EmulationStateType.NotSet;
        public HashSet<GraphicsPack> graphicsPacks  = new HashSet<GraphicsPack>();
        internal HashSet<string> GraphicsPacksFolders = new HashSet<string>();

        // CEMU Options
        public byte FullScreen  = 1;
        public byte EnableVSync  = 0;
        public UpscaleFilterType UpscaleFilter  = UpscaleFilterType.Bicubic;
        public FullScreenScalingType FullScreenScaling  = FullScreenScalingType.KeepAspectRatio;
        public GpuBufferCacheAccuracyType GpuBufferCacheAccuracy  = GpuBufferCacheAccuracyType.High;

        // CPU
        public CpuModeType CpuMode  = CpuModeType.SingleCoreCompiler;
        public CpuTimerType CpuTimer  = CpuTimerType.HostBasedTimer;

        // Graphic Pack Settings
        public int ClarityPreset = 3;

        public int Fps = 30;

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

      
    }
}
