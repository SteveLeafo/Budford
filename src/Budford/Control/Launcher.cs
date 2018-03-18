using Budford.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Budford.Properties;
using System.Threading;
using System.Runtime.InteropServices;
using Budford.View;
using System.Drawing;
using SharpPresence;
using System.Text;

namespace Budford.Control
{
    internal class Launcher
    {
        // import the function in your class
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        /// <summary>
        /// 
        /// </summary>
        internal Model.Model Model;

        /// <summary>
        /// 
        /// </summary>
        readonly FormMainWindow parent;

        /// <summary>
        /// 
        /// </summary>
        Process runningProcess;

        /// <summary>
        /// 
        /// </summary>
        DateTime startTime = DateTime.MinValue;

        /// <summary>
        /// 
        /// </summary>
        GameInformation runningGame;

        /// <summary>
        /// 
        /// </summary>
        static InstalledVersion runningVersion;

        static string cemu = "";
        static string logfile = "";

        static readonly string[][] ContollerFileNames = 
        {
            new[] { "controller0.txt", "controller0.bfb" },
            new[] { "controller1.txt", "controller1.bfb" },
            new[] { "controller2.txt", "controller2.bfb" },
            new[] { "controller3.txt", "controller3.bfb" },
            new[] { "controller4.txt", "controller4.bfb" },
            new[] { "controller5.txt", "controller5.bfb" },
            new[] { "controller6.txt", "controller6.bfb" },
            new[] { "controller7.txt", "controller7.bfb" }
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentIn"></param>
        internal Launcher(FormMainWindow parentIn)
        {
            parent = parentIn;
        }

        /// <summary>
        /// 
        /// </summary>
        internal bool Open(Model.Model model)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = Resources.fMainWindow_toolStripButton1_Click_Nintendo_Launch_Files_____rpx_;

                // Show open file dialog box 
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LaunchRpx(model, dlg.FileName);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentIn"></param>
        /// <param name="modelIn"></param>
        /// <param name="game"></param>
        /// <param name="getSaveDir"></param>
        /// <param name="cemuOnly"></param>
        /// <param name="shiftUp"></param>
        /// <param name="forceFullScreen"></param>
        internal void LaunchCemu(FormMainWindow parentIn, Model.Model modelIn, GameInformation game, bool getSaveDir = false, bool cemuOnly = false, bool shiftUp = true, bool forceFullScreen = false)
        {
            makeBorderLess = false;
            SetCemuVersion(modelIn, game);

            if (!GameIsPlayable(parentIn, modelIn, game))
            {
                TriggerEarlyExit();
                return;
            }

            if (File.Exists(cemu) || File.Exists(modelIn.Settings.WineExe))
            {
                if (modelIn.Settings.UpdateDiscordPresence)
                {
                    DiscordRichPresence.StartGame(game);
                }

                DeleteLogFile(modelIn, logfile);
                SetupCafeLibs(modelIn, game);
                if (game != null)
                {
                    CopyLargestShaderCacheToCemu(game);
                    CopyLatestSaveToCemu(game);
                }

                if (modelIn.Settings.DisableShaderCache)
                {
                    FileManager.DeleteShaderCache(runningVersion);
                }

                if (!getSaveDir)
                {
                    CreateDefaultSettingsFile(runningVersion, game);

                    DeleteShaderCacheIfRequired(modelIn, runningVersion);

                    CreateDefaultSettingsFile(modelIn, game);
                }
                // Prepare the process to run
                ProcessStartInfo start = new ProcessStartInfo();

                PopulateStartInfo(game, getSaveDir, cemuOnly, CemuFeatures.Cemu, start, shiftUp, forceFullScreen);

                // Required since 1.11.2
                if (runningVersion != null)
                {
                    start.WorkingDirectory = runningVersion.Folder;
                }

                // Run the external process & wait for it to finish
                var parentProcess = Process.GetCurrentProcess();
                var original = parentProcess.PriorityClass;

                parentProcess.PriorityClass = GetProcessPriority(modelIn.Settings.ShaderPriority);

                startTime = DateTime.Now;
                if (File.Exists(Path.Combine(runningVersion.Folder, start.FileName)))
                {
                    StartCemuProcess(parentIn, modelIn, game, getSaveDir, cemuOnly, start, parentProcess, original);
                }
                else
                {
                    TriggerEarlyExit();
                }
            }
            else
            {
                MessageBox.Show(parentIn, Resources.Launcher_LaunchCemu_Please_install_CEMU, Resources.Launcher_LaunchCemu_CEMU_is_not_installed);
                TriggerEarlyExit();
            }
        }

        public static string ToReadableString(TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:2} : ", span.Days) : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:2} : ", span.Hours) : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:2} : ", span.Minutes) : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:2} :", span.Seconds) : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }

        private void StartCemuProcess(FormMainWindow parentIn, Model.Model modelIn, GameInformation game, bool getSaveDir, bool cemuOnly, ProcessStartInfo start, Process parentProcess, ProcessPriorityClass original)
        {
            StartCemuProcess(start);
            runningGame = game;

            if (parentIn != null)
            {
                try
                {
                    parentIn.SetParent(runningProcess);
                }
                catch (Exception)
                {
                    // Dies in Linux
                }
            }
            parentProcess.PriorityClass = original;

            // Allow the process to finish starting.
            try
            {
                runningProcess.PriorityClass = GetProcessPriority(modelIn.Settings.ShaderPriority);
            }
            catch (Exception)
            {
                // Probably not enough permissions...
            }
            WaitForProcess(modelIn, game, getSaveDir, cemuOnly);
        }

        private void TriggerEarlyExit()
        {
            if (parent != null)
            {
                parent.ProcessExited();
            }
        }

        private bool GameIsPlayable(FormMainWindow parentIn, Model.Model modelIn, GameInformation game)
        {

            if (runningVersion == null)
            {
                if (parent != null)
                {
                    parent.ProcessExited();
                }
                return false;
            }

            Model = modelIn;

            if (game != null && !game.Exists)
            {
                if (parentIn != null)
                {
                    if (!parentIn.InvokeRequired)
                    {
                        MessageBox.Show(parentIn, Resources.Launcher_LaunchCemu_If_you_are_using_a_removable_storage_device_check_it_is_plugged_in_and_try_again, Resources.Launcher_LaunchCemu_Can_not_find_file);
                        if (parent != null)
                        {
                            parent.ProcessExited();
                        }
                    }
                }
                return false;
            }
            return true;
        }

        private static void SetupCafeLibs(Model.Model model, GameInformation game)
        {
            string cafeLibs = Path.Combine(runningVersion.Folder, "cafeLibs");

            if (!Directory.Exists(cafeLibs))
            {
                Directory.CreateDirectory(cafeLibs);
            }
            FileManager.ClearFolder(cafeLibs);

            if (game != null)
            {
                if (game.GameSetting.UseCafeLibs == 1)
                {
                    FileManager.SafeCopy(Path.Combine(SpecialFolders.CafeLibDirBudford(model), "snd_user.rpl"), Path.Combine(cafeLibs, "snd_user.rpl"));
                    FileManager.SafeCopy(Path.Combine(SpecialFolders.CafeLibDirBudford(model), "snduser2.rpl"), Path.Combine(cafeLibs, "snduser2.rpl"));
                }
            }
        }

        ProcessPriorityClass GetProcessPriority(int priority)
        {
            switch (priority)
            {
                case 0: return ProcessPriorityClass.High;
                case 1: return ProcessPriorityClass.AboveNormal;
                case 2: return ProcessPriorityClass.Normal;
                case 3: return ProcessPriorityClass.BelowNormal;
                case 4: return ProcessPriorityClass.Idle;
            }
            return ProcessPriorityClass.Normal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        internal void CopyLargestShaderCacheToCemu(GameInformation game)
        {
            if (runningVersion == null)
            {
                SetCemuVersion(Model, game);
            }

            if (runningVersion != null && runningVersion.VersionNumber >= 170)
            {
                if (!game.SaveDir.StartsWith("??"))
                {
                    FileInfo src = new FileInfo(SpecialFolders.ShaderCacheBudford(Model, game));
                    if (File.Exists(src.FullName))
                    {
                        FileInfo dest = new FileInfo(SpecialFolders.ShaderCacheCemu(runningVersion, game));
                        if (!File.Exists(dest.FullName) || dest.Length < src.Length)
                        {
                            FileManager.SafeCopy(src.FullName, dest.FullName, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        private void CopyLatestSaveToCemu(GameInformation game)
        {
            if (!game.SaveDir.StartsWith("??"))
            {
                DirectoryInfo src = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirBudford(Model, Model.CurrentUser, game, ""));
                DirectoryInfo dest = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirCemu(runningVersion, game));

                string lockFileName = Path.Combine(dest.FullName, "Budford.lck");
                if (File.Exists(lockFileName))
                {
                    // This save has been locked, which means budford launched the game, but wasn't running with it exitted.
                    // In this case we won't overwrite the save as it is sure to be newer
                    return;
                }

                UpdateFolder(src, dest, true);

                src = new DirectoryInfo(SpecialFolders.CommonSaveDirBudford(Model, game, ""));
                dest = new DirectoryInfo(SpecialFolders.CommonUserFolderCemu(runningVersion, game));
                UpdateFolder(src, dest);

                CreateLockFile(lockFileName);
            }
        }

        private static void CreateLockFile(string lockFileName)
        {
            try
            {
                File.Create(lockFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.Launcher_CreateLockFile_Failed_to_create_lock_file);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="smashIt"></param>    
        internal static void UpdateFolder(DirectoryInfo src, DirectoryInfo dest, bool smashIt = false)
        {
            if (smashIt)
            {
                if (Directory.Exists(dest.FullName))
                {
                    dest.Delete(true);
                }
                dest.Create();
            }

            if (Directory.Exists(src.FullName))
            {
                if (src.GetFiles().Any() || src.GetDirectories().Any())
                {
                    FileManager.CopyFilesRecursively(src, dest, false, true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="game"></param>
        /// <param name="getSaveDir"></param>
        /// <param name="cemuOnly"></param>
        private void WaitForProcess(Model.Model model, GameInformation game, bool getSaveDir, bool cemuOnly)
        {
            try
            {
                runningProcess.WaitForInputIdle();
            }
            catch (Exception ex)
            {
                model.Errors.Add(ex.Message);
            }

            try
            {
                runningProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
            }
            catch (Exception)
            {
                // Probably don't have enough permissions.
            }

            if (getSaveDir)
            {
                ExtractSaveDirName(game);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    ExtractSaveDirName(game);
                });
            }

            if (getSaveDir && !cemuOnly)
            {
                if (!runningProcess.HasExited)
                {
                    runningProcess.Kill();
                }
            }

            if (getSaveDir)
            {
                runningProcess.WaitForExit();
            }
            else
            {
                if (game != null)
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        runningProcess.WaitForExit();
                    });
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void proc_Exited(object sender, EventArgs e)
        {
            if (Model != null)
            {
                if (Model.Settings.UpdateDiscordPresence)
                {
                    DiscordRichPresence.EndGame();
                }
            }

            if (runningVersion != null)
            {
                if (runningGame != null)
                {
                    foreach (var controller in ContollerFileNames)
                    {
                        SafeCopy(runningVersion, controller[1], controller[0]);
                    }

                    if (!runningGame.SaveDir.StartsWith("??"))
                    {
                        CopyShaderCaches();

                        CopySaves();
                    }
                }
            }

            if (runningGame != null)
            {
                runningGame.PlayTime += (long)(DateTime.Now - startTime).TotalSeconds;
            }
            runningGame = null;
            ClearRunningVersion();

            if (parent != null)
            {
                parent.ProcessExited();
            }
        }

        private void CopySaves()
        {
            // Copy saves
            DirectoryInfo src = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirCemu(runningVersion, runningGame));
            DirectoryInfo dest = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirBudford(Model, Model.CurrentUser, runningGame, ""));
            DirectoryInfo src255 = new DirectoryInfo(SpecialFolders.CommonUserFolderCemu(runningVersion, runningGame));
            DirectoryInfo dest255 = new DirectoryInfo(SpecialFolders.CommonSaveDirBudford(Model, runningGame, ""));
            if (Directory.Exists(src.FullName))
            {
                if (File.Exists(Path.Combine(src.FullName, "Budford.lck")))
                {
                    // Delete the lock file, to allow Budford to overwrite the Cemu save in future.
                    try
                    {
                        FileManager.SafeDelete("Budford.lck");
                    }
                    catch (Exception)
                    {
                        // No code
                    }
                }

                if (src.GetDirectories().Any() || src.GetFiles().Any() || (Directory.Exists(src255.FullName) && (src255.GetFiles().Any() || src255.GetDirectories().Any())))
                {
                    if (!Directory.Exists(dest.FullName))
                    {
                        dest.Create();
                    }
                    if (!Directory.Exists(dest255.FullName))
                    {
                        dest255.Create();
                    }

                    FileManager.CopyFilesRecursively(src, dest, false, true);
                    FileManager.CopyFilesRecursively(src255, dest255, false, true);
                }
            }
        }

        private void CopyShaderCaches()
        {
            // Copy shader caches...
            FileInfo srcFile = new FileInfo(SpecialFolders.ShaderCacheCemu(runningVersion, runningGame));
            if (File.Exists(srcFile.FullName))
            {
                FileInfo destFile = new FileInfo(SpecialFolders.ShaderCacheBudford(Model, runningGame));
                if (!File.Exists(destFile.FullName) || destFile.Length < srcFile.Length)
                {
                    string folder = Path.GetDirectoryName(destFile.FullName);
                    if (!Directory.Exists(folder))
                    {
                        if (folder != null) Directory.CreateDirectory(folder);
                    }
                    FileManager.SafeCopy(srcFile.FullName, destFile.FullName, true);
                }
            }
        }

        private static void ClearRunningVersion()
        {
            runningVersion = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="getSaveDir"></param>
        /// <param name="cemuOnly"></param>
        /// <param name="cemuIn"></param>
        /// <param name="start"></param>
        /// <param name="shiftUp"></param>
        /// <param name="forceFullScreen"></param>
        private void PopulateStartInfo(GameInformation game, bool getSaveDir, bool cemuOnly, string cemuIn, ProcessStartInfo start, bool shiftUp, bool forceFullScreen = false)
        {
            // Enter in the command line arguments, everything you would enter after the executable name itself
            if (!cemuOnly)
            {
                SetGameLaunchParameters(game, getSaveDir, start, shiftUp, forceFullScreen);
            }

            // Enter the executable to run, including the complete path
            if (Model.Settings.WineExe.Length > 1)
            {
                start.FileName = Model.Settings.WineExe;
                start.Arguments = Path.Combine(Directory.GetCurrentDirectory(), runningVersion.Folder, cemuIn) + " " + start.Arguments;
            }
            else
            {
                start.FileName = cemuIn;
            }

            // Do you want to show a console window?
            start.CreateNoWindow = true;

            if (game != null)
            {
                game.PlayCount++;

                game.LastPlayed = DateTime.Now;

                if (parent != null)
                {
                    parent.RefreshList(game);
                }
            }
        }

        string GetNoLegacyOption()
        {
            if (Model.Settings.LegacyIntelGpuMode)
            {
                return "-nolegacy";
            }
            return "";
        }

        bool makeBorderLess;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="getSaveDir"></param>
        /// <param name="start"></param>
        /// <param name="shiftUp"></param>
        /// <param name="forceFullScreen"></param>
        private void SetGameLaunchParameters(GameInformation game, bool getSaveDir, ProcessStartInfo start, bool shiftUp = true, bool forceFullScreen = false)
        {
            if (getSaveDir)
            {
                if (game != null)
                {
                    start.Arguments = GetNoLegacyOption() + " -g \"" + game.LaunchFile + "\"" + GetMlcOption();
                }
                if (Model.Settings.HideWindowWhenCaching)
                {
                    start.WindowStyle = ProcessWindowStyle.Minimized;
                }
            }
            else
            {
                if (game != null && game.GameSetting.FullScreen == 1 && !shiftUp)
                {
                    if (Model.Settings.BorderlessFullScreen)
                    {
                        start.Arguments = GetNoLegacyOption() + "-g \"" + game.LaunchFile + "\"" + GetMlcOption();
                        start.WindowStyle =  ProcessWindowStyle.Maximized;
                        makeBorderLess = true;
                    }
                    else
                    {
                        start.Arguments = GetNoLegacyOption() + " -f -g \"" + game.LaunchFile + "\"" + GetMlcOption();
                    }
                }
                else if (game != null && game.GameSetting.FullScreen == 4 && !shiftUp)
                {
                    start.Arguments = GetNoLegacyOption() + "-g \"" + game.LaunchFile + "\"" + GetMlcOption();
                    start.WindowStyle = ProcessWindowStyle.Maximized;
                    makeBorderLess = true;
                }
                else if (game != null)
                {
                    start.WindowStyle = (ProcessWindowStyle)game.GameSetting.FullScreen;
                    if (forceFullScreen)
                    {
                        if (Model.Settings.BorderlessFullScreen)
                        {
                            start.Arguments = GetNoLegacyOption() + " -g \"" + game.LaunchFile + "\"" + GetMlcOption();
                            start.WindowStyle = ProcessWindowStyle.Maximized;
                        }
                        else
                        {
                            start.Arguments = GetNoLegacyOption() + " -f -g \"" + game.LaunchFile + "\"" + GetMlcOption();
                        }

                    }
                    else
                    {
                        start.Arguments = GetNoLegacyOption() + " -g \"" + game.LaunchFile + "\"" + GetMlcOption();
                    }
                }
            }
        }

        private string GetMlcOption()
        {
            if (Model != null)
            {
                if (Model.Settings.MlcFolder != "")
                {
                    if (Directory.Exists(Model.Settings.MlcFolder))
                    {
                        return " -mlc \"" + Model.Settings.MlcFolder + "\"";
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="game"></param>
        private static void CreateDefaultSettingsFile(Model.Model model, GameInformation game)
        {
            if (game != null)
            {
                CemuSettings cs = new CemuSettings(model, game.GameSetting, game);
                cs.WriteSettingsBinFile();
                WriteCemuHookIniFile.WriteIni(model, game);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <param name="game"></param>
        private static void CreateDefaultSettingsFile(InstalledVersion version, GameInformation game)
        {
            if (game != null)
            {
                if (version != null)
                {
                    foreach (var controller in ContollerFileNames)
                    {
                        SafeCopy(version, controller[0], controller[1]);
                    }

                    UpdateControllerProfiles(version, game.GameSetting.ControllerOverride1, game.GameSetting.SwapButtons1, "controller0.txt");
                    UpdateControllerProfiles(version, game.GameSetting.ControllerOverride2, game.GameSetting.SwapButtons2, "controller1.txt");
                    UpdateControllerProfiles(version, game.GameSetting.ControllerOverride3, game.GameSetting.SwapButtons3, "controller2.txt");
                    UpdateControllerProfiles(version, game.GameSetting.ControllerOverride4, game.GameSetting.SwapButtons4, "controller3.txt");
                    UpdateControllerProfiles(version, game.GameSetting.ControllerOverride5, game.GameSetting.SwapButtons5, "controller4.txt");
                    UpdateControllerProfiles(version, game.GameSetting.ControllerOverride6, game.GameSetting.SwapButtons6, "controller5.txt");
                    UpdateControllerProfiles(version, game.GameSetting.ControllerOverride7, game.GameSetting.SwapButtons7, "controller6.txt");
                    UpdateControllerProfiles(version, game.GameSetting.ControllerOverride8, game.GameSetting.SwapButtons8, "controller7.txt");
                }
            }
        }

        private static void UpdateControllerProfiles(InstalledVersion version, int controllerOverride, int swapButtons, string profileName)
        {
            string fileName = Path.Combine(version.Folder, "controllerProfiles", profileName);
            if (File.Exists(fileName))
            {
                string text = File.ReadAllText(fileName);
                text = SwapControllerButtons(swapButtons, text);

                switch (controllerOverride)
                {
                    case 1:
                        text = text.Replace("Wii U Pro Controller", "Wii U GamePad");
                        text = text.Replace("Wii U Classic Controller", "Wii U GamePad");
                        break;
                    case 2:
                        text = text.Replace("Wii U GamePad", "Wii U Pro Controller");
                        text = text.Replace("Wii U Classic Controller", "Wii U Pro Controller");
                        break;
                    case 3:
                        text = text.Replace("Wii U GamePad", "Wii U Classic Controller");
                        text = text.Replace("Wii U Pro Controller", "Wii U Classic Controller");
                        break;
                    case 5:
                        FileManager.SafeDelete(fileName);
                        return;
                }
                File.WriteAllText(fileName, text);
            }
        }

        private static string SwapControllerButtons(int swapButtons, string text)
        {
            string[] buttons = new string[4];
            foreach (var line in text.Split('\n'))
            {
                if (line.StartsWith("1 = "))
                {
                    buttons[0] = line;
                }
                if (line.StartsWith("2 = "))
                {
                    buttons[1] = line;
                }
                if (line.StartsWith("3 = "))
                {
                    buttons[2] = line;
                }
                if (line.StartsWith("4 = "))
                {
                    buttons[3] = line;
                }
            }

            if (swapButtons == 1 || swapButtons == 3)
            {
                text = text.Replace(buttons[0], "ctrl1");
                text = text.Replace(buttons[1], "ctrl2");
                text = text.Replace("ctrl1", buttons[1].Replace("2 = ", "1 = "));
                text = text.Replace("ctrl2", buttons[0].Replace("1 = ", "2 = "));
            }
            if (swapButtons == 2 || swapButtons == 3)
            {
                text = text.Replace(buttons[2], "ctrl3");
                text = text.Replace(buttons[3], "ctrl4");
                text = text.Replace("ctrl3", buttons[3].Replace("4 = ", "3 = "));
                text = text.Replace("ctrl4", buttons[2].Replace("3 = ", "4 = "));
            }
            return text;
        }

        private static void SafeCopy(InstalledVersion version, string source, string destination)
        {
            string fileName = Path.Combine(version.Folder, "controllerProfiles", source);
            if (File.Exists(fileName))
            {
                string backUpFileName = Path.Combine(version.Folder, "controllerProfiles", destination);
                FileManager.SafeCopy(fileName, backUpFileName, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="version"></param>
        private static void DeleteShaderCacheIfRequired(Model.Model model, InstalledVersion version)
        {
            if (model.Settings.DisableShaderCache)
            {
                DirectoryInfo di1 = new DirectoryInfo(Path.Combine(SpecialFolders.ShaderCacheFolderCemu(version), "transferable"));
                foreach (FileInfo file in di1.GetFiles())
                {
                    FileManager.SafeDelete(file.FullName);
                }
                DirectoryInfo di2 = new DirectoryInfo(Path.Combine(SpecialFolders.ShaderCacheFolderCemu(version), "shaderCache", "precompiled"));
                foreach (FileInfo file in di2.GetFiles())
                {
                    FileManager.SafeDelete(file.FullName);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="logfileIn"></param>
        private static void DeleteLogFile(Model.Model model, string logfileIn)
        {
            try
            {
                FileManager.SafeDelete(logfileIn);
            }
            catch (Exception ex)
            {
                model.Errors.Add(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="game"></param>
        internal static void SetCemuVersion(Model.Model model, GameInformation game)
        {
            if (model != null)
            {
                if (game != null)
                {
                    if (game.GameSetting.PreferedVersion != "Latest")
                    {
                        runningVersion = model.Settings.InstalledVersions.FirstOrDefault(v => v.Name == game.GameSetting.PreferedVersion);
                        PopulateFromVersion(ref cemu, ref logfile, runningVersion);
                    }
                    else
                    {
                        runningVersion = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
                        PopulateFromVersion(ref cemu, ref logfile, runningVersion);
                    }
                }
                else
                {
                    runningVersion = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
                    PopulateFromVersion(ref cemu, ref logfile, runningVersion);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cemuIn"></param>
        /// <param name="logfileIn"></param>
        /// <param name="version"></param>
        private static void PopulateFromVersion(ref string cemuIn, ref string logfileIn, InstalledVersion version)
        {
            if (version != null)
            {
                cemuIn = Path.Combine(version.Folder, CemuFeatures.Cemu);
                logfileIn = Path.Combine(version.Folder, "log.txt");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fileName"></param>
        /// <param name="forceFullScreen"></param>
        internal void LaunchRpx(Model.Model model, string fileName, bool forceFullScreen = false)
        {
            makeBorderLess = false;
            string cemuExe = "";
            var latest = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
            if (latest != null)
            {
                cemuExe = Path.Combine(latest.Folder, CemuFeatures.Cemu);
            }

            if (File.Exists(cemuExe))
            {
                if (model.Settings.DisableShaderCache)
                {
                    FileManager.DeleteShaderCache(latest);
                }

                CemuSettings cs = new CemuSettings(model, null, null);
                cs.WriteSettingsBinFile();

                // Prepare the process to run
                ProcessStartInfo start = new ProcessStartInfo
                {
                    Arguments = forceFullScreen ? "-f -g \"" + fileName + "\"" : "-g \"" + fileName + "\"",
                    FileName = cemuExe,
                    CreateNoWindow = true
                };

                // Run the external process & wait for it to finish
                StartCemuProcess(start);
            }
            else
            {
                MessageBox.Show(parent, Resources.Launcher_LaunchCemu_Please_install_CEMU, Resources.Launcher_LaunchCemu_CEMU_is_not_installed);
                if (parent != null)
                {
                    parent.ProcessExited();
                }
            }
        }

        private void StartCemuProcess(ProcessStartInfo start)
        {
            startTime = DateTime.Now;
            runningProcess = Process.Start(start);
            runningProcess.EnableRaisingEvents = true;
            runningProcess.Exited += proc_Exited;
        }


        /// <summary>
        /// Launches CEMU and tries to extract the SaveDir from the windows title
        /// </summary>
        /// <param name="game"></param>
        private void ExtractSaveDirName(GameInformation game)
        {
            try
            {
                if (Model.Settings.Monitor == -1)
                {
                    MoveToMonitor(runningProcess.MainWindowHandle, Model.Settings.Monitor);
                }

                WaitForWindowTitleToAppear();

                if (makeBorderLess)
                {
                    MakeBorderlessFullScreen();
                }
                SetCemuCpuPrioty(game);

                SetGamePadViewIfDesired(game);

                if (parent != null)
                {
                    parent.ProcessRunning();
                }
            }
            catch (Exception)
            {
                // Nothing
            }
        }

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);


        public void MoveToMonitor(IntPtr windowHandle, int numberMonitor)
        {
            if (numberMonitor >= 1)
            {
                if (Screen.AllScreens.Length >= numberMonitor)
                {
                    numberMonitor--;
                    //Get the data of the monitor
                    var monitor = Screen.AllScreens[numberMonitor].WorkingArea;
                    //change the window to the second monitor
                    SetWindowPos(windowHandle, IntPtr.Zero,
                    monitor.Left, monitor.Top, monitor.Width,
                    monitor.Height, 0);
                }
            }
        }

        private void SetGamePadViewIfDesired(GameInformation game)
        {
            try
            {
                if (game != null)
                {
                    if (game.GameSetting.DefaultView == 1)
                    {
                        IntPtr h = runningProcess.MainWindowHandle;
                        SetForegroundWindow(h);
                        SendKeys.SendWait("^{TAB}");
                    }
                }
            }
            catch (Exception ex)
            {
                parent.Model.Errors.Add(ex.Message);
            }
        }

        private void SetCemuCpuPrioty(GameInformation game)
        {
            if (runningProcess.HasExited)
            {
                return;
            }
            try
            {
                if (game != null)
                {
                    switch (game.GameSetting.CpuMode)
                    {
                        case GameSettings.CpuModeType.DualCoreCompiler: runningProcess.PriorityClass = GetProcessPriority(Model.Settings.DualCorePriority);
                            break;
                        case GameSettings.CpuModeType.TripleCoreCompiler: runningProcess.PriorityClass = GetProcessPriority(Model.Settings.TripleCorePriority);
                            break;
                        default: runningProcess.PriorityClass = GetProcessPriority(Model.Settings.SingleCorePriority);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                parent.Model.Errors.Add(ex.Message);
            }
        }

        private void WaitForWindowTitleToAppear()
        {
            int i = runningProcess.MainWindowTitle.IndexOf("SaveDir", StringComparison.Ordinal);
            int c = 0;
            while (i == -1 && c < 50000)
            {
                try
                {
                    Thread.Sleep(100);
                    Thread.Sleep(100);
                    if (!runningProcess.HasExited)
                    {
                        runningProcess.Refresh();
                        i = runningProcess.MainWindowTitle.IndexOf("Title", StringComparison.Ordinal);
                    }
                    else
                    {
                        break;
                    }
                    c++;
                }
                catch (Exception ex)
                {
                    parent.Model.Errors.Add(ex.Message);
                    break;
                }
            }
        }

        /// <summary>
        /// Kills the current process
        /// </summary>
        internal void KillCurrentProcess()
        {
            if (runningProcess != null)
            {
                if (!runningProcess.HasExited)
                {
                    runningProcess.Kill();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="game"></param>
        internal void CreateSaveSnapshot(Model.Model model, GameInformation game)
        {
            if (!game.SaveDir.StartsWith("??"))
            {
                if (runningVersion == null)
                {
                    SetCemuVersion(model, game);
                }
                DirectoryInfo saveDir = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirBudford(model, "", game, ""));
                string snapShotDir = "S_" + saveDir.EnumerateDirectories().Count();

                DirectoryInfo src = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirCemu(runningVersion, game));
                DirectoryInfo dest = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirBudford(model, model.CurrentUser, game, snapShotDir));
                UpdateFolder(src, dest);

                src = new DirectoryInfo(SpecialFolders.CommonUserFolderCemu(runningVersion, game));
                dest = new DirectoryInfo(SpecialFolders.CommonSaveDirBudford(model, game, snapShotDir));

                UpdateFolder(src, dest);
            }
        }

        internal void FullScreen()
        {
            if (runningProcess != null)
            {
                IntPtr h = runningProcess.MainWindowHandle;
                SetForegroundWindow(h);
                SendKeys.SendWait("%{ENTER}");
            }
        }

        internal void ScreenShot()
        {
            if (runningProcess != null)
            {
                IntPtr h = runningProcess.MainWindowHandle;
                SetForegroundWindow(h);
                Thread.Sleep(100);
                SendKeys.SendWait("+{PRTSC}");
                Thread.Sleep(100);
                SetForegroundWindow(parent.Handle);
            }
        }

        internal void MakeBorderlessFullScreen()
        {
            var styleNewWindowStandard = NativeMethods.GetWindowLong(runningProcess.MainWindowHandle, NativeMethods.WindowLongIndex.Style) & ~(NativeMethods.WindowStyles.Caption | NativeMethods.WindowStyles.ThickFrame | NativeMethods.WindowStyles.SystemMenu | NativeMethods.WindowStyles.MaximizeBox | NativeMethods.WindowStyles.MinimizeBox);

            HideMenu();

            NativeMethods.SetWindowLong(runningProcess.MainWindowHandle, NativeMethods.WindowLongIndex.Style, styleNewWindowStandard);

            Rectangle rect = Screen.FromHandle(runningProcess.MainWindowHandle).Bounds;
            NativeMethods.SetWindowPos(runningProcess.MainWindowHandle, 0, rect.X, rect.Y, rect.Width, rect.Height, NativeMethods.SetWindowPosTypes.ShowWindow | NativeMethods.SetWindowPosTypes.NoOwnerZOrder | NativeMethods.SetWindowPosTypes.NoSendChanging);
        }

        private void HideMenu()
        {
            var menuHandle = NativeMethods.GetMenu(runningProcess.MainWindowHandle);
            if (menuHandle != IntPtr.Zero)
            {
                var menuItemCount = NativeMethods.GetMenuItemCount(menuHandle);

                for (var i = 0; i < menuItemCount; i++)
                {
                    NativeMethods.RemoveMenu(menuHandle, 0, NativeMethods.Menus.ByPosition | NativeMethods.Menus.Remove);
                }

                NativeMethods.DrawMenuBar(runningProcess.MainWindowHandle);
            }
        }
    }
}
