using Budford.Model;
using Budford.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Budford.Control
{
    internal class CemuSettings
    {
        int packIndex;

        readonly GameSettings settings;

        readonly GameInformation information;

        readonly Dictionary<string, Tuple<int[], int[]>> settingsByVersion;
        readonly Dictionary<int, int> graphicPackOffset;

        GraphicsPack resolutionPack;

        readonly string[] supportedResolutions = new[]{
            "360",
            "540",
            "1080",
            "1440",
            "1800",
            "2160",
            "2880",
            "4320",
            "5760" };

        enum Settings : byte
        {
            RenderUpsideDownOffset = 0,     // 1 = Upside Down
            GpuBufferAccuractOffset = 1,    // 0 = High, 1 = Medium, 2 = Low
            UpscaleFilterOffset = 2,        // 1 = Bicubic
            FullScreenScalingOffset = 3,    // 1 = Stretch
            VSyncOffset = 4,                // 1 = On
            DisableAudioOffset = 5,         // 1 = Disabled,
            ConsoleRegionOffset = 6,        // Auto = FF, 02 = USA, 04 = EUR, 01 = JAP, 10 = China, Korea = 20, Taiwan = 40
            ConsoleLanguageOffset = 7,      // 0x00 - English07, 0x01 - Jap, 0x02 = French, 0x03 = German, 0x04 = Italian, 0x05 = Spanish, 0x06 = Chinese, 0x07 = Korean, 0x08 = Dutch, 0x09 = Portugese, 0x0A = Russian, 0x0B = Taiwaneseb
            BoTwWorkAroundOffset = 8,       // 1 = On
            FullSyncAtGx2Offset = 9,        // 1 = On
            SeparateGamepadViewOffset = 10,       // 1 = On
            UseRtdscOffset = 11,                  // 3 = On / 2 = Off
            EnableOnLineModeOffset = 12,          // 3 = On / 2 = Off
        }

        enum CoreSettings : byte
        {
            DebugGx2ApiOffset = 0,
            DebugUnsupportedApiCallsOffset = 1,
            DebugThreadSynchronisationApiOffset = 2,
            DebugAudioApiOffset = 3,
            DebugInputApiOffset = 4,
            EnableDebugOffset = 5,          // 1 = Enabled
            VolumeOffset = 6,               // Volume 0 -> 0x64
            CpuModeOffset = 7,              // 1 = Fast
            CpuTimerOffset = 8              // 1 = Host
        }

        int[] settingsOffsets;
        int[] coreSettingsOffsets;
        int[] settingsFile;

        readonly int[] coreSettingsV1115 = new[] { 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1e, 0x28, 0x2c, 0x2d, 0x00, 0x00, 0x00, 0x00 };
        readonly int[] coreSettingsV1113 = new[] { 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1e, 0x28, 0x2c, 0x2d, 0x00, 0x00, 0x00, 0x00 };
        readonly int[] coreSettings = new[] { 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1e, 0x28, 0x2b, 0x2c, 0x00, 0x00, 0x00, 0x00 };


        readonly int[] v1115Settings = new[] { 0x30, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x52, 0x53, 0x6a };
        readonly int[] v1114Settings = new[] { 0x30, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x52, 0x53, 0x6a };
        readonly int[] v1113Settings = new[] { 0x30, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x52, 0x53, 0x6a };
        readonly int[] v1112Settings = new[] { 0x30, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x52, 0x53, 0x6a };
        readonly int[] v1110Settings = new[] { 0x2f, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x52, 0x6a };
        readonly int[] v1100Settings = new[] { 0x2f, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x52, 0x6a };
        readonly int[] v191Settings = new[] { 0x2f, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x00, 0x00 };
        readonly int[] v190Settings = new[] { 0x2f, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x00, 0x00, 0x00 };
        readonly int[] v182Settings = new[] { 0x30, 0x49, 0x4a, 0x4b, 0x4d, 0x4e, 0x4f, 0x50, 0x00, 0x00, 0x00, 0x00, 0x00 };
        readonly int[] v174Settings = new[] { 0x30, 0x49, 0x4a, 0x4b, 0x4d, 0x4e, 0x4f, 0x50, 0x00, 0x00, 0x00, 0x00, 0x00 };
        readonly int[] v173Settings = new[] { 0x30, 0x31, 0x32, 0x33, 0x35, 0x36, 0x37, 0x38, 0x00, 0x00, 0x00, 0x00, 0x00 };
        readonly int[] v171Settings = new[] { 0x30, 0x31, 0x32, 0x00, 0x34, 0x35, 0x36, 0x37, 0x00, 0x00, 0x00, 0x00, 0x00 };
        readonly int[] v163Settings = new[] { 0x00, 0x30, 0x00, 0x00, 0x32, 0x33, 0x34, 0x35, 0x00, 0x00, 0x00, 0x00, 0x00 };
        readonly int[] v160Settings = new[] { 0x00, 0x00, 0x00, 0x00, 0x31, 0x32, 0x33, 0x34, 0x00, 0x00, 0x00, 0x00, 0x00 };

        readonly Model.Model model;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelIn"></param>
        /// <param name="settingsIn"></param>
        /// <param name="informationIn"></param>
        internal CemuSettings(Model.Model modelIn, GameSettings settingsIn, GameInformation informationIn)
        {
            model = modelIn;
            settings = settingsIn;
            information = informationIn;

            settingsByVersion = new Dictionary<string, Tuple<int[], int[]>>
            {
                {"1.0.0", new Tuple<int[], int[]>(CemuSettingsFiles.Settings100Bin, v160Settings) },
                {"1.0.1", new Tuple<int[], int[]>(CemuSettingsFiles.Settings101Bin, v160Settings) },
                {"1.0.2", new Tuple<int[], int[]>(CemuSettingsFiles.Settings102Bin, v160Settings) },
                {"1.1.0", new Tuple<int[], int[]>(CemuSettingsFiles.Settings110Bin, v160Settings) },
                {"1.1.1", new Tuple<int[], int[]>(CemuSettingsFiles.Settings111Bin, v160Settings) },
                {"1.1.2", new Tuple<int[], int[]>(CemuSettingsFiles.Settings112Bin, v160Settings) },
                {"1.2.0", new Tuple<int[], int[]>(CemuSettingsFiles.Settings120Bin, v160Settings) },
                {"1.3.0", new Tuple<int[], int[]>(CemuSettingsFiles.Settings130Bin, v160Settings) },
                {"1.3.1", new Tuple<int[], int[]>(CemuSettingsFiles.Settings131Bin, v160Settings) },
                {"1.3.2", new Tuple<int[], int[]>(CemuSettingsFiles.Settings132Bin, v160Settings) },
                {"1.3.3", new Tuple<int[], int[]>(CemuSettingsFiles.Settings133Bin, v160Settings) },
                {"1.4.0", new Tuple<int[], int[]>(CemuSettingsFiles.Settings140Bin, v160Settings) },
                {"1.4.1", new Tuple<int[], int[]>(CemuSettingsFiles.Settings141Bin, v160Settings) },
                {"1.4.2", new Tuple<int[], int[]>(CemuSettingsFiles.Settings142Bin, v160Settings) },
                {"1.5.0", new Tuple<int[], int[]>(CemuSettingsFiles.Settings150Bin, v160Settings) },
                {"1.5.1", new Tuple<int[], int[]>(CemuSettingsFiles.Settings151Bin, v160Settings) },
                {"1.5.2", new Tuple<int[], int[]>(CemuSettingsFiles.Settings152Bin, v160Settings) },
                {"1.5.3", new Tuple<int[], int[]>(CemuSettingsFiles.Settings153Bin, v160Settings) },
                {"1.5.4", new Tuple<int[], int[]>(CemuSettingsFiles.Settings154Bin, v160Settings) },
                {"1.5.5", new Tuple<int[], int[]>(CemuSettingsFiles.Settings155Bin, v160Settings) },
                {"1.5.6", new Tuple<int[], int[]>(CemuSettingsFiles.Settings156Bin, v160Settings) },
                {"1.6.0", new Tuple<int[], int[]>(CemuSettingsFiles.Settings160Bin, v160Settings) },
                {"1.6.1", new Tuple<int[], int[]>(CemuSettingsFiles.Settings161Bin, v160Settings) },
                {"1.6.2", new Tuple<int[], int[]>(CemuSettingsFiles.Settings162Bin, v160Settings) },
                {"1.6.3", new Tuple<int[], int[]>(CemuSettingsFiles.Settings163Bin, v163Settings) },
                {"1.6.4", new Tuple<int[], int[]>(CemuSettingsFiles.Settings164Bin, v163Settings) },
                {"1.7.0", new Tuple<int[], int[]>(CemuSettingsFiles.Settings170Bin, v163Settings) },
                {"1.7.1", new Tuple<int[], int[]>(CemuSettingsFiles.Settings171Bin, v171Settings) },
                {"1.7.2", new Tuple<int[], int[]>(CemuSettingsFiles.Settings172Bin, v171Settings) },
                {"1.7.3d",new Tuple<int[], int[]>(CemuSettingsFiles.Settings173Bin, v173Settings) },
                {"1.7.3", new Tuple<int[], int[]>(CemuSettingsFiles.Settings173Bin, v173Settings) },
                {"1.7.4", new Tuple<int[], int[]>(CemuSettingsFiles.Settings174Bin, v173Settings) },
                {"1.7.5", new Tuple<int[], int[]>(CemuSettingsFiles.Settings175Bin, v174Settings) },
                {"1.8.0", new Tuple<int[], int[]>(CemuSettingsFiles.Settings180Bin, v174Settings) },
                {"1.8.0b",new Tuple<int[], int[]>(CemuSettingsFiles.Settings180Bin, v174Settings) },
                {"1.8.1", new Tuple<int[], int[]>(CemuSettingsFiles.Settings181Bin, v174Settings) },
                {"1.8.2", new Tuple<int[], int[]>(CemuSettingsFiles.Settings182Bin, v182Settings) },
                {"1.8.2b",new Tuple<int[], int[]>(CemuSettingsFiles.Settings182Bin, v182Settings) },
                {"1.9.0", new Tuple<int[], int[]>(CemuSettingsFiles.Settings190Bin, v190Settings) },
                {"1.9.1", new Tuple<int[], int[]>(CemuSettingsFiles.Settings191Bin, v191Settings) },
                {"1.10.0", new Tuple<int[], int[]>(CemuSettingsFiles.Settings1100Bin, v1100Settings) },
                {"1.11.0", new Tuple<int[], int[]>(CemuSettingsFiles.Settings1110Bin, v1110Settings) },
                {"1.11.2", new Tuple<int[], int[]>(CemuSettingsFiles.Settings1112Bin, v1112Settings) },
                {"1.11.3", new Tuple<int[], int[]>(CemuSettingsFiles.Settings1113Bin, v1113Settings) },
                {"1.11.4", new Tuple<int[], int[]>(CemuSettingsFiles.Settings1114Bin, v1114Settings) },
                {"1.11.5", new Tuple<int[], int[]>(CemuSettingsFiles.Settings1114Bin, v1115Settings) },
                {"1.11.6", new Tuple<int[], int[]>(CemuSettingsFiles.Settings1114Bin, v1114Settings) },
                {"1.12.0", new Tuple<int[], int[]>(CemuSettingsFiles.Settings1114Bin, v1114Settings) },
                {"2.0.0", new Tuple<int[], int[]>(CemuSettingsFiles.Settings1114Bin, v1114Settings) }
            };

            graphicPackOffset = new Dictionary<int, int>
            {
                {100, 0x7c },
                {101, 0x7c },
                {102, 0x7c },
                {110, 0x7c },
                {111, 0x7c },
                {112, 0x7c },
                {120, 0x7c },
                {130, 0x7c },
                {131, 0x7c },
                {132, 0x7c },
                {133, 0x7c },
                {140, 0x7c },
                {141, 0x7c },
                {142, 0x7c },
                {150, 0x7c },
                {151, 0x7c },
                {152, 0x7c },
                {153, 0x7c },
                {154, 0x7c },
                {155, 0x7c },
                {156, 0x7c },
                {160, 0x7c },
                {161, 0x7c },
                {162, 0x7c },
                {163, 0x7c },
                {164, 0x7c },
                {170, 0x7c },
                {171, 0x7c },
                {172, 0x7c },
                {173, 0x7c },
                {174, 0x7c },
                {175, 0x7c },
                {180, 0x7c },
                {181, 0x7c },
                {182, 0x7c },
                {190, 0x7c },
                {191, 0x7c },
                {1100, 0x7c },
                {1110, 0x7c },
                {1112, 0X6f },
                {1113, 0x7c },
                {1114, 0x7c }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentCemuVersion"></param>
        private void SetOffsets(string currentCemuVersion)
        {
            Tuple<int[], int[]> settingsOffsetsLocal;
            currentCemuVersion = Regex.Replace(currentCemuVersion, "[A-Za-z ]", "").Replace("_","");
            if (settingsByVersion.TryGetValue(currentCemuVersion.Replace("-", "").Replace("a", "").Replace("b", "").Replace("c", "").Replace("d", "").Replace("e", "").Replace("f", "").Replace("g", ""), out settingsOffsetsLocal))
            {
                settingsFile = settingsOffsetsLocal.Item1;
                settingsOffsets = settingsOffsetsLocal.Item2;
                int version;
                if (int.TryParse(currentCemuVersion.Replace(".", "").Replace("Cemu_", ""), out version))
                {
                    if (version >= 1113)
                    {
                        coreSettingsOffsets = coreSettingsV1113;
                        if (version >= 1115)
                        {
                            coreSettingsOffsets = coreSettingsV1115;
                        }
                    }
                    else
                    {
                        coreSettingsOffsets = coreSettings;
                    }
                }
            }
            else
            {
                int version;
                if (int.TryParse(currentCemuVersion.Replace(".", "").Replace("Cemu_",""), out version))
                {
                    if (version >= 1113)
                    {
                        coreSettingsOffsets = coreSettingsV1113;
                        if (version >= 1115)
                        {
                            coreSettingsOffsets = coreSettingsV1115;
                        }
                    }
                    else
                    {
                        coreSettingsOffsets = coreSettings;
                    }
                    if (version > 191)
                    {
                         if (version >= 1114)
                         {
                            settingsFile = CemuSettingsFiles.Settings1114Bin;
                            settingsOffsets = v1114Settings;
                         }
                         else
                         {
                            settingsFile = CemuSettingsFiles.Settings1100Bin;
                            settingsOffsets = v1100Settings;
                         }
                    }
                    else
                    {
                        settingsFile = CemuSettingsFiles.Settings160Bin;
                        settingsOffsets = v160Settings;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        InstalledVersion GetVersion()
        {
            InstalledVersion version = null;
            if (settings != null)
            {
                if (settings.PreferedVersion == "Latest")
                {
                    version = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
                }
                else
                {
                    version = model.Settings.InstalledVersions.FirstOrDefault(v => v.Name == settings.PreferedVersion);
                }
            }
            return version;
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WriteSettingsBinFile()
        {
            InstalledVersion version = GetVersion();

            if (version != null)
            {
                if (version.Folder != null)
                {
                    SetOffsets(version.Version);

                    try
                    {
                        if (File.Exists(Path.Combine(version.Folder, "settings.bin")))
                        {
                            File.Delete(Path.Combine(version.Folder, "settings.bin"));
                        }
                    }
                    catch (Exception)
                    {
                    }

                    // No settings file, so lets build our own..
                    if (!File.Exists(Path.Combine(version.Folder, "settings.bin")))
                    {
                        using (FileStream fn = new FileStream(Path.Combine(version.Folder, "settings.bin"), FileMode.Create, FileAccess.ReadWrite))
                        {
                            foreach (int file in settingsFile)
                            {
                                fn.WriteByte((byte)file);
                            }
                        }
                    }

                    FileManager.GrantAccess(Path.Combine(version.Folder, "settings.bin"));


                    if (information != null)
                    {
                        WriteGraphicsPacks(version);
                        WriteSettings(version.Folder);
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        void WriteSettings(string folder)
        {
            if (!Directory.Exists(Path.Combine(folder, "gameProfiles")))
            {
                Directory.CreateDirectory(Path.Combine(folder, "gameProfiles"));
            }

            CurrentUserSecurity cs = new CurrentUserSecurity();

            if (cs.HasAccess(new FileInfo(Path.Combine(folder, "gameProfiles", information.TitleId + ".ini")), System.Security.AccessControl.FileSystemRights.Write))
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(folder, "gameProfiles",  information.TitleId + ".ini")))
                {
                    writer.WriteLine("[Graphics]");
                    writer.WriteLine("accurateShaderMul = " + (settings.AccaccurateShaderMul == 1 ? "true" :  (settings.AccaccurateShaderMul == 0) ? "false" : "min"));
                    writer.WriteLine("disableGPUFence =  " + (settings.DisableGpuFence == 1 ? "true" : "false"));
                    writer.WriteLine("[CPU]");
                    writer.WriteLine("emulateSinglePrecision = " + (settings.EmulateSinglePrecision == 1 ? "true" : "false"));
                }
            }

            FileManager.GrantAccess(Path.Combine(folder, "gameProfiles", information.TitleId + ".ini"));

            if (cs.HasAccess(new FileInfo(Path.Combine(folder, "settings.bin")), System.Security.AccessControl.FileSystemRights.Write))
            {
                using (FileStream fn = new FileStream(Path.Combine(folder, "settings.bin"), FileMode.Open, FileAccess.ReadWrite))
                {
                    WriteByte(fn, coreSettingsOffsets[(int)CoreSettings.DebugGx2ApiOffset], settings.DebugGx2ApiOffset);
                    WriteByte(fn, coreSettingsOffsets[(int)CoreSettings.DebugUnsupportedApiCallsOffset], settings.DebugUnsupportedApiCallsOffset);
                    WriteByte(fn, coreSettingsOffsets[(int)CoreSettings.DebugThreadSynchronisationApiOffset], settings.DebugThreadSynchronisationApiOffset);
                    WriteByte(fn, coreSettingsOffsets[(int)CoreSettings.DebugAudioApiOffset], settings.DebugAudioApiOffset);
                    WriteByte(fn, coreSettingsOffsets[(int)CoreSettings.DebugInputApiOffset], settings.DebugInputApiOffset);

                    byte volume = settings.Volume;
                    if (model.Settings.UseGlobalVolumeSettings)
                    {
                        volume = (byte)model.Settings.GlobalVolume;
                    }

                    WriteByte(fn, coreSettingsOffsets[(int)CoreSettings.VolumeOffset], volume);
                    WriteByte(fn, coreSettingsOffsets[(int)CoreSettings.EnableDebugOffset], settings.EnableDebugOffset);
                    WriteByte(fn, coreSettingsOffsets[(int)CoreSettings.CpuModeOffset], (byte)settings.CpuMode);
                    WriteByte(fn, coreSettingsOffsets[(int)CoreSettings.CpuTimerOffset], (byte)settings.CpuTimer);

                    WriteByte(fn, settingsOffsets[(int)Settings.GpuBufferAccuractOffset], (byte)settings.GpuBufferCacheAccuracy);
                    WriteByte(fn, settingsOffsets[(int)Settings.UpscaleFilterOffset], (byte)settings.UpscaleFilter);
                    WriteByte(fn, settingsOffsets[(int)Settings.FullScreenScalingOffset], (byte)settings.FullScreenScaling);
                    WriteByte(fn, settingsOffsets[(int)Settings.VSyncOffset], settings.EnableVSync);

                    WriteByte(fn, settingsOffsets[(int)Settings.RenderUpsideDownOffset], settings.RenderUpsideDown);
                    WriteByte(fn, settingsOffsets[(int)Settings.DisableAudioOffset], settings.DisableAudio);
                    WriteByte(fn, settingsOffsets[(int)Settings.ConsoleRegionOffset], (byte)model.Settings.ConsoleRegion);
                    WriteByte(fn, settingsOffsets[(int)Settings.ConsoleLanguageOffset], (byte)model.Settings.ConsoleLanguage);
                    WriteByte(fn, settingsOffsets[(int)Settings.BoTwWorkAroundOffset], settings.EnableBoTwCrashWorkaround);
                    WriteByte(fn, settingsOffsets[(int)Settings.FullSyncAtGx2Offset], settings.FullSyncAtGx2DrawDone);
                    WriteByte(fn, settingsOffsets[(int)Settings.SeparateGamepadViewOffset], settings.SeparateGamePadView);
                    WriteByte(fn, settingsOffsets[(int)Settings.UseRtdscOffset], settings.UseRtdsc);
                    WriteByte(fn, settingsOffsets[(int)Settings.EnableOnLineModeOffset], settings.Online != 0 ? 3 : 2);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="position"></param>
        /// <param name="value"></param>
        private static void WriteByte(FileStream fn, int position, int value)
        {
            if (position != 0)
            {
                fn.Seek(position, SeekOrigin.Begin);
                fn.Flush();
                fn.WriteByte((byte)value);
                fn.Flush();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        void WriteGraphicsPacks(InstalledVersion version)
        {
            EnableDefaultGraphicsPack();
            for (int i = 0; i < 100; ++i)
            {
                try
                {
                    if (Directory.Exists(Path.Combine(version.Folder, "graphicPacks", "Budford_" + i)))
                    {
                        Directory.Delete(Path.Combine(version.Folder, "graphicPacks", "Budford_" + i), true);
                    }
                }
                catch (Exception)
                {
                    // May be in use 
                }
            }

            SetClarityPreset();
            SetFps();


            int packs = 0;
            if (resolutionPack != null)
            {
                resolutionPack.PackId = packs;
                packs++;
            }

            foreach (var pack in settings.graphicsPacks)
            {
                if (pack.Active)
                {
                    FileManager.CopyFilesRecursively(new DirectoryInfo(Path.Combine("graphicsPacks", "graphicPacks_2-" + model.Settings.GraphicsPackRevision, pack.Folder)), 
                        new DirectoryInfo(Path.Combine(version.Folder,  "graphicPacks", "Budford_" + packs)), false, true);
                    pack.PackId = packs;
                    packs++;
                }
            }

            if (packs > 0)
            {
                int gfxPackStartOffset;
                using (FileStream fn = new FileStream(Path.Combine(version.Folder, "settings.bin"), FileMode.Open, FileAccess.ReadWrite))
                {
                    
                    if (!graphicPackOffset.TryGetValue(version.VersionNumber, out gfxPackStartOffset))
                    {
                        if (version.VersionNumber >= 1112)
                        {
                            gfxPackStartOffset = 0x7c;
                        }
                        else
                        {
                            gfxPackStartOffset = 0x23;
                        }
                    }

                    if (version.VersionNumber >= 1112)
                    {
                        fn.Seek(gfxPackStartOffset, SeekOrigin.Begin);
                        fn.WriteByte((byte)packs);
                        gfxPackStartOffset += 4;
                    }
                    else
                    {
                        fn.Seek(0x23, SeekOrigin.Begin);
                        gfxPackStartOffset = fn.ReadByte();
                        fn.Seek(0x23, SeekOrigin.Begin);
                        fn.WriteByte((byte)(gfxPackStartOffset + (9 * packs)));

                        gfxPackStartOffset += 0x23 + 4;
                        fn.Seek(gfxPackStartOffset - 4, SeekOrigin.Begin);
                        fn.WriteByte((byte)packs);
                    }
                }

                packIndex = 0;
                foreach (var pack in settings.graphicsPacks)
                {
                    AppendGraphicsPack(version.Folder, gfxPackStartOffset, pack);
                }

                if (resolutionPack != null)
                {
                    AppendGraphicsPack(version.Folder, gfxPackStartOffset, resolutionPack);
                }

                using (FileStream fn = new FileStream(Path.Combine(version.Folder, "settings.bin"), FileMode.Open, FileAccess.ReadWrite))
                {
                    fn.Seek(gfxPackStartOffset + (9 * packs), SeekOrigin.Begin);
                    for (int i = 0; i < 137; ++i)
                    {
                        fn.WriteByte(0);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetClarityPreset()
        {
            string clarityShader = Path.Combine("graphicsPacks", "graphicPacks_2-" + model.Settings.GraphicsPackRevision, "BreathOfTheWild_Clarity", "37040a485a29d54e_00000000000003c9_ps.txt");
            if (File.Exists(clarityShader))
            {
                string text = File.ReadAllText(clarityShader);
                string preset = "#define Preset " + settings.ClarityPreset;
                text = text.Replace("#define Preset 0", preset);
                text = text.Replace("#define Preset 1", preset);
                text = text.Replace("#define Preset 2", preset);
                text = text.Replace("#define Preset 3", preset);
                text = text.Replace("#define Preset 4", preset);
                text = text.Replace("#define Preset 5", preset);
                text = text.Replace("#define Preset 6", preset);
                text = text.Replace("#define Preset 7", preset);
                text = text.Replace("#define Preset 8", preset);
                text = text.Replace("#define Preset 9", preset);
                File.WriteAllText(clarityShader, text);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetFps()
        {
            if (settings.OverrideFps)
            {
                float fps = settings.Fps;
                string[] fpsPatches = new[] { "BreathOfTheWild_StaticFPS_30", "BreathOfTheWild_StaticFPS_30", "BreathOfTheWild_StaticFPS_30" };
                foreach (var fpsPatchSpeed in fpsPatches)
                {
                    string fpsPatch = Path.Combine("graphicsPacks", "graphicPacks_2-" + model.Settings.GraphicsPackRevision, fpsPatchSpeed, "patches.txt");
                    if (!File.Exists(fpsPatch + ".bak"))
                    {
                        File.Copy(fpsPatch, fpsPatch + ".bak");
                    }
                    if (File.Exists(fpsPatch + ".bak"))
                    {
                        string text = File.ReadAllText(fpsPatch + ".bak");
                        if (text.Contains("0x00000000 = .float 1 # = 30FPS / TARGET FPS, e.g. 30FPS / 18FPS = 1.66667"))
                        {
                            text = text.Replace("0x00000000 = .float 1 # = 30FPS / TARGET FPS, e.g. 30FPS / 18FPS = 1.66667", "0x00000000 = .float " + (30.0f / fps));
                        }
                        text = text.Replace("0x18 = .float 30", "0x18 = .float " + (fps));
                        File.WriteAllText(fpsPatch, text);
                    }
                }

                string fpsRules = Path.Combine("graphicsPacks", "graphicPacks_2-" + model.Settings.GraphicsPackRevision, "BreathOfTheWild_StaticFPS_30", "rules.txt");
                if (!File.Exists(fpsRules + ".bak"))
                {
                    File.Copy(fpsRules, fpsRules + ".bak");
                }
                if (File.Exists(fpsRules + ".bak"))
                {
                    string text = File.ReadAllText(fpsRules + ".bak");
                    text = text.Replace("vsyncFrequency = 30", "vsyncFrequency = " + (fps));
                    File.WriteAllText(fpsRules, text);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void EnableDefaultGraphicsPack()
        {
            resolutionPack = null;
            foreach (var pack in settings.graphicsPacks)
            {
                if (IsResolutionPack(pack.Title))
                {
                    if (pack.Title.Contains(model.Settings.DefaultResolution))
                    {
                        resolutionPack = new GraphicsPack()
                        {
                            Active = true,
                            File = pack.File,
                            Folder = pack.Folder,
                            Title = pack.Title,
                        };
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private bool IsResolutionPack(string title)
        {
            if (title != null)
            {
                foreach (var resolution in supportedResolutions)
                {
                    if (title.Contains(resolution))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(String hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gfxPackStartOffset"></param>
        /// <param name="pack"></param>
        /// <param name="folder"></param>
        void AppendGraphicsPack(string folder, int gfxPackStartOffset, GraphicsPack pack)
        {
            if (resolutionPack != null)
            {
                if (pack.Title != resolutionPack.Title)
                {
                    if (IsResolutionPack(pack.Title))
                    {
                        return;
                    }
                }
            }

            using (FileStream fn = new FileStream(Path.Combine(folder, "settings.bin"), FileMode.Open, FileAccess.ReadWrite))
            {
                if (pack.Active)
                {
                    fn.Seek(gfxPackStartOffset + (packIndex++ * 9), SeekOrigin.Begin);
                    string gui = GraphicsPack.GraphicPackHashes[pack.PackId][1];
                    fn.Write(StringToByteArray(gui), 0, 8);
                    fn.WriteByte(1);
                }
            }
        }
    }
}
