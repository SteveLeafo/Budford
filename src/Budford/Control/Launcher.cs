using Budford.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Budford.Properties;
using System.Threading;
using System.Runtime.InteropServices;

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
        internal Model.Model model;

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
        GameInformation runningGame = null;

        /// <summary>
        /// 
        /// </summary>
        static InstalledVersion runningVersion = null;

        static string cemu = "";
        static string logfile = "";


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
        internal void Open(Model.Model model)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = Resources.fMainWindow_toolStripButton1_Click_Nintendo_Launch_Files_____rpx_;

                // Show open file dialog box 
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LaunchRpx(model, dlg.FileName);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelIn"></param>
        /// <param name="game"></param>
        /// <param name="getSaveDir"></param>
        /// <param name="cemu_only"></param>
        internal void LaunchCemu(Form parent, Model.Model modelIn, GameInformation game, bool getSaveDir = false, bool cemu_only = false, bool shiftUp = true)
        {
            if (runningVersion != null)
            {
                return;
            }

            model = modelIn;

            if (game != null &&  !game.Exists)
            {
                if (!parent.InvokeRequired)
                {
                    MessageBox.Show(parent, "If you are using a removable storage device check it is plugged in and try again", "Can not find file");
                }
                return;
            }            

            SetCemuVersion(modelIn, game);

            if (File.Exists(cemu))
            {
                DeleteLogFile(modelIn, logfile);

                if (game != null)
                {
                    CopyLargestShaderCacheToCemu(game);
                    CopyLatestSaveToCemu(game);
                }

                if (!getSaveDir)
                {
                    DeleteShaderCacheIfRequired(modelIn, runningVersion);

                    CreateDefaultSettingsFile(modelIn, game);
                }
                // Prepare the process to run
                ProcessStartInfo start = new ProcessStartInfo();

                PopulateStartInfo(game, getSaveDir, cemu_only, "CEMU.EXE", start, shiftUp);

                // Required since 1.11.2
                start.WorkingDirectory = runningVersion.Folder;

                // Run the external process & wait for it to finish
                var parentProcess = Process.GetCurrentProcess();
                var original = parentProcess.PriorityClass;

                parentProcess.PriorityClass = GetProcessPriority(modelIn.Settings.ShaderPriority);


                string path = Directory.GetCurrentDirectory();
                
                runningProcess = Process.Start(start);
                runningProcess.EnableRaisingEvents = true;
                runningProcess.Exited += new EventHandler(proc_Exited);

                runningGame = game;

                if (parent != null)
                {
                    NativeMethods.SetParent(runningProcess.Handle, parent.Handle);
                }
                parentProcess.PriorityClass = original;

                // Allow the process to finish starting.
                if (runningProcess != null)
                {
                    try
                    {
                        runningProcess.PriorityClass = GetProcessPriority(modelIn.Settings.ShaderPriority);
                    }
                    catch (Exception)
                    {
                        // Probably not enough permissions...
                    }
                    WaitForProcess(modelIn, game, getSaveDir, cemu_only, logfile);
                }
            }
            else
            {
                MessageBox.Show(parent, Resources.Launcher_LaunchCemu_Please_install_CEMU, Resources.Launcher_LaunchCemu_CEMU_is_not_installed);
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
                SetCemuVersion(model, game);
            }

            if (runningVersion.VersionNumber >= 170)
            {
                if (!game.SaveDir.StartsWith("??"))
                {
                    FileInfo src = new FileInfo(SpecialFolders.ShaderCacheBudford(game));
                    if (File.Exists(src.FullName))
                    {
                        FileInfo dest = new FileInfo(SpecialFolders.ShaderCacheCemu(runningVersion, game));
                        if (!File.Exists(dest.FullName) || dest.Length < src.Length)
                        {
                            File.Copy(src.FullName, dest.FullName, true);
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
                DirectoryInfo src = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirBudford(model.CurrentUser, game, ""));
                DirectoryInfo dest = new DirectoryInfo(SpecialFolders.CurrenUserSaveDirCemu(runningVersion,game));
                UpdateFolder(src, dest, true);

                src = new DirectoryInfo(SpecialFolders.CommonSaveDirBudford(game, ""));
                dest = new DirectoryInfo(SpecialFolders.CommonUserFolderCemu(runningVersion, game));
                UpdateFolder(src, dest);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>    
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
        /// <param name="cemu_only"></param>
        /// <param name="logfile"></param>
        private void WaitForProcess(Model.Model model, GameInformation game, bool getSaveDir, bool cemu_only, string logfile)
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
                ExtractSaveDirName(game, logfile);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    ExtractSaveDirName(game, logfile);
                });
            }

            if (getSaveDir && !cemu_only)
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
            if (runningGame != null)
            {
                if (runningVersion != null)
                {
                    if (!runningGame.SaveDir.StartsWith("??"))
                    {
                        // Copy shader caches...
                        FileInfo srcFile = new FileInfo(SpecialFolders.ShaderCacheCemu(runningVersion, runningGame));
                        if (File.Exists(srcFile.FullName))
                        {
                            FileInfo destFile = new FileInfo(SpecialFolders.ShaderCacheBudford(runningGame));
                            if (!File.Exists(destFile.FullName) || destFile.Length < srcFile.Length)
                            {
                                string folder = Path.GetDirectoryName(destFile.FullName);
                                if (!Directory.Exists(folder))
                                {
                                    Directory.CreateDirectory(folder);
                                }
                                File.Copy(srcFile.FullName, destFile.FullName, true);
                            }
                        }

                        // Copy saves
                        DirectoryInfo src = new DirectoryInfo(SpecialFolders.CurrenUserSaveDirCemu(runningVersion, runningGame));
                        DirectoryInfo dest = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirBudford(model.CurrentUser, runningGame, ""));
                        DirectoryInfo src_255 = new DirectoryInfo(SpecialFolders.CommonUserFolderCemu(runningVersion, runningGame));
                        DirectoryInfo dest_255 = new DirectoryInfo(SpecialFolders.CommonSaveDirBudford(runningGame, ""));
                        if (Directory.Exists(src.FullName))
                        {
                            if (src.GetDirectories().Any() || src.GetFiles().Any() || (Directory.Exists(src_255.FullName) && (src_255.GetFiles().Any() || src_255.GetDirectories().Any())))
                            {
                                if (!Directory.Exists(dest.FullName))
                                {
                                    dest.Create();
                                }
                                if (!Directory.Exists(dest_255.FullName))
                                {
                                    dest_255.Create();
                                }

                                FileManager.CopyFilesRecursively(src, dest, false, true);
                                FileManager.CopyFilesRecursively(src_255, dest_255, false, true);
                            }
                        }
                    }
                }
            }

            runningGame = null;
            runningVersion = null;

            if (parent != null)
            {
                parent.ProcessExited();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="getSaveDir"></param>
        /// <param name="cemu_only"></param>
        /// <param name="cemu"></param>
        /// <param name="start"></param>
        private void PopulateStartInfo(GameInformation game, bool getSaveDir, bool cemu_only, string cemu, ProcessStartInfo start, bool shiftUp)
        {
            // Enter in the command line arguments, everything you would enter after the executable name itself
            if (!cemu_only)
            {
                SetGameLaunchParameters(game, getSaveDir, start, shiftUp);
            }

            // Enter the executable to run, including the complete path
            start.FileName = cemu;

            // Do you want to show a console window?
            start.CreateNoWindow = true;

            if (game != null)
            {
                game.PlayCount++;

                game.LastPlayed = DateTime.Now;

                parent.RefreshList(game);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="getSaveDir"></param>
        /// <param name="start"></param>
        private static void SetGameLaunchParameters(GameInformation game, bool getSaveDir, ProcessStartInfo start, bool shiftUp = true)
        {
            if (getSaveDir)
            {
                if (game != null)
                {
                    start.Arguments = "-nolegacy -g \"" + game.LaunchFile + "\"";
                }
                //start.WindowStyle = ProcessWindowStyle.Minimized;
            }
            else
            {
                if (game != null && game.GameSetting.FullScreen == 1 && !shiftUp)
                {
                    start.Arguments = "-nolegacy -f -g \"" + game.LaunchFile + "\"";
                }
                else if (game != null)
                {
                    start.Arguments = "-nolegacy -g \"" + game.LaunchFile + "\"";
                }
            }
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
        /// <param name="model"></param>
        private static void DeleteShaderCacheIfRequired(Model.Model model, InstalledVersion version)
        {
            if (model.Settings.DisableShaderCache)
            {
                DirectoryInfo di1 = new DirectoryInfo(SpecialFolders.ShaderCacheFolderCemu(version) + "\\transferable");
                foreach (FileInfo file in di1.GetFiles())
                {
                    file.Delete();
                }
                DirectoryInfo di2 = new DirectoryInfo(SpecialFolders.ShaderCacheFolderCemu(version) + "\\shaderCache\\precompiled");
                foreach (FileInfo file in di2.GetFiles())
                {
                    file.Delete();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="logfile"></param>
        private static void DeleteLogFile(Model.Model model, string logfile)
        {
            try
            {
                if (File.Exists(logfile))
                {
                    File.Delete(logfile);
                }
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
        /// <param name="cemu"></param>
        /// <param name="logfile"></param>
        internal static void SetCemuVersion(Model.Model model, GameInformation game)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cemu"></param>
        /// <param name="logfile"></param>
        /// <param name="version"></param>
        private static void PopulateFromVersion(ref string cemu, ref string logfile, InstalledVersion version)
        {
            if (version != null)
            {
                cemu = version.Folder + "\\cemu.exe";
                logfile = version.Folder + "\\log.txt";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fileName"></param>
        internal void LaunchRpx(Model.Model model, string fileName, bool forceFullScreen = false)
        {
            string cemu = "";
            var latest = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
            if (latest != null)
            {
                cemu = latest.Folder + "\\cemu.exe";
            }

            if (File.Exists(cemu))
            {
                if (model.Settings.DisableShaderCache)
                {
                    DirectoryInfo di1 = new DirectoryInfo("Cemu\\cemu_" + model.Settings.CurrentCemuVersion + "\\shaderCache\\transferable");
                    foreach (FileInfo file in di1.GetFiles())
                    {
                        file.Delete();
                    }
                    DirectoryInfo di2 = new DirectoryInfo("Cemu\\cemu_" + model.Settings.CurrentCemuVersion + "\\shaderCache\\precompiled");
                    foreach (FileInfo file in di2.GetFiles())
                    {
                        file.Delete();
                    }
                }

                GameSettings setting = null;
                GameInformation information = null;

                CemuSettings cs = new CemuSettings(model, setting, information);
                cs.WriteSettingsBinFile();

                // Prepare the process to run
                ProcessStartInfo start = new ProcessStartInfo
                {
                    Arguments = forceFullScreen ? "-f -g \"" + fileName + "\"" : "-g \"" + fileName + "\"",
                    FileName = cemu,
                    CreateNoWindow = true
                };

                // Run the external process & wait for it to finish
                runningProcess = Process.Start(start);               
            }
            else
            {
                MessageBox.Show(parent, Resources.Launcher_LaunchCemu_Please_install_CEMU, Resources.Launcher_LaunchCemu_CEMU_is_not_installed);
            }
        }

        /// <summary>
        /// Launches CEMU and tries to extract the SaveDir from the windows title
        /// </summary>
        /// <param name="game"></param>
        /// <param name="logFile"></param>
        private void ExtractSaveDirName(GameInformation game, string logFile)
        {
            int i = runningProcess.MainWindowTitle.IndexOf("SaveDir", StringComparison.Ordinal);
            int c = 0;
            while (i == -1 && c < 50000)
            {
                try
                {
                    System.Threading.Thread.Sleep(100);
                    System.Threading.Thread.Sleep(100);
                    if (!runningProcess.HasExited)
                    {
                        runningProcess.Refresh();
                        i = runningProcess.MainWindowTitle.IndexOf("Title", StringComparison.Ordinal);
                    }
                    else
                    {
                        return;
                    }
                    c++;
                }
                catch (Exception ex)
                {
                    parent.model.Errors.Add(ex.Message);
                    break;
                }
            }

            try
            {
                switch (game.GameSetting.CpuMode)
                {
                    case GameSettings.CpuModeType.DualCoreCompiler: runningProcess.PriorityClass = GetProcessPriority(model.Settings.DualCorePriority);
                        break;
                    case GameSettings.CpuModeType.TripleCoreCompiler: runningProcess.PriorityClass = GetProcessPriority(model.Settings.TripleCorePriority);
                        break;
                    default: runningProcess.PriorityClass = GetProcessPriority(model.Settings.SingleCorePriority);
                        break;
                }
            }
            catch (Exception ex)
            {
                parent.model.Errors.Add(ex.Message);
            }

            try
            {
                if (game.GameSetting.DefaultView == 1)
                {
                    IntPtr h = runningProcess.MainWindowHandle;
                    SetForegroundWindow(h);
                    SendKeys.SendWait("^{TAB}");
                }
            }
            catch (Exception ex)
            {
                parent.model.Errors.Add(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logFile"></param>
        /// <returns></returns>
        private string ExtractSaveDirFromLogfile(string logFile)
        {
            System.Threading.Thread.Sleep(1200);
            var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (StreamReader sr = new StreamReader(fs))
            {
                string[] lines = sr.ReadToEnd().Replace("\r", "").Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("saveDir and shaderCache name"))
                    {
                        int col = line.LastIndexOf(':');
                        string saveDir = line.Substring(col + 1);
                        return saveDir;
                    }
                }
            }
            return "??";
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
                DirectoryInfo saveDir = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirBudford("", game, ""));
                string snapShotDir = "S_" + saveDir.EnumerateDirectories().Count();

                DirectoryInfo src = new DirectoryInfo(SpecialFolders.CurrenUserSaveDirCemu(runningVersion, game));
                DirectoryInfo dest = new DirectoryInfo(SpecialFolders.CurrentUserSaveDirBudford(model.CurrentUser, game, snapShotDir));
                UpdateFolder(src, dest);

                src = new DirectoryInfo(SpecialFolders.CommonUserFolderCemu(runningVersion, game));
                dest = new DirectoryInfo(SpecialFolders.CommonSaveDirBudford(game, snapShotDir));

                UpdateFolder(src, dest);
            }
        }

        internal void FullScreen()
        {
            if (runningProcess != null)
            {
                IntPtr h = runningProcess.MainWindowHandle;
                SetForegroundWindow(h);
                //SendKeys.SendWait("+{PRTSC}");
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
    }
}
