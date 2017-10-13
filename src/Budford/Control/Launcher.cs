using Budford.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Budford.Properties;

namespace Budford.Control
{
    internal class Launcher
    {
        readonly FormMainWindow parent;

        Process proc;

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
        /// <param name="model"></param>
        /// <param name="game"></param>
        /// <param name="getSaveDir"></param>
        /// <param name="cemu_only"></param>
        internal void LaunchCemu(Model.Model model, GameInformation game, bool getSaveDir = false, bool cemu_only = false, bool shiftUp = true)
        {
            if (game != null &&  !game.Exists)
            {
                MessageBox.Show(parent, "If you are using a removable storage device check it is plugged in and try again", "Can not find file");
                return;
            }            
            string cemu = "";
            string logfile = "";

            SetCemuVersion(model, game, ref cemu, ref logfile);

            if (File.Exists(cemu))
            {
                DeleteLogFile(model, logfile);

                if (!getSaveDir)
                {
                    DeleteShaderCacheIfRequired(model);

                    CreateDefaultSettingsFile(model, game);
                }
                // Prepare the process to run
                ProcessStartInfo start = new ProcessStartInfo();

                PopulateStartInfo(game, getSaveDir, cemu_only, cemu, start, shiftUp);

                // Run the external process & wait for it to finish
                var parentProcess = Process.GetCurrentProcess();
                var original = parentProcess.PriorityClass;

                parentProcess.PriorityClass = ProcessPriorityClass.BelowNormal;

                proc = Process.Start(start);

                parentProcess.PriorityClass = original;

                // Allow the process to finish starting.
                if (proc != null)
                {
                    proc.PriorityClass = ProcessPriorityClass.BelowNormal;
                    WaitForProcess(model, game, getSaveDir, cemu_only, logfile);
                }
            }
            else
            {
                MessageBox.Show(parent, Resources.Launcher_LaunchCemu_Please_install_CEMU, Resources.Launcher_LaunchCemu_CEMU_is_not_installed);
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
                proc.WaitForInputIdle();
            }
            catch (Exception ex)
            {
                model.Errors.Add(ex.Message);
            }

            proc.PriorityClass = ProcessPriorityClass.BelowNormal;

            if (getSaveDir && !cemu_only)
            {
                ExtractSaveDirName(game, logfile);
            }

            if (getSaveDir)
            {
                proc.WaitForExit();
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
                    start.Arguments = "-g \"" + game.LaunchFile + "\"";
                }
                start.WindowStyle = ProcessWindowStyle.Minimized;
            }
            else
            {
                if (game != null && game.GameSetting.FullScreen == 1 && !shiftUp)
                {
                    start.Arguments = "-f -g \"" + game.LaunchFile + "\"";
                }
                else if (game != null)
                {
                    start.Arguments = "-g \"" + game.LaunchFile + "\"";
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
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        private static void DeleteShaderCacheIfRequired(Model.Model model)
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
        private static void SetCemuVersion(Model.Model model, GameInformation game, ref string cemu, ref string logfile)
        {
            if (game != null)
            {
                if (game.GameSetting.PreferedVersion != "Latest")
                {
                    var version = model.Settings.InstalledVersions.FirstOrDefault(v => v.Name == game.GameSetting.PreferedVersion);
                    PopulateFromVersion(ref cemu, ref logfile, version);
                }
                else
                {
                    var latest = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
                    PopulateFromVersion(ref cemu, ref logfile, latest);
                }
            }
            else
            {
                var latest = model.Settings.InstalledVersions.FirstOrDefault(v => v.IsLatest);
                PopulateFromVersion(ref cemu, ref logfile, latest);
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

                //foreach (var gd in model.GameData)
                //{
                //    if (gd.Value.LaunchFile == fileName)
                //    {
                //        setting = gd.Value.GameSetting;
                //        information = gd.Value;
                //    }
                //}

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
                proc = Process.Start(start);               
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
            int i = proc.MainWindowTitle.IndexOf("SaveDir", StringComparison.Ordinal);
            int c = 0;
            while (i == -1 && c < 300)
            {
                try
                {
                    System.Threading.Thread.Sleep(100);
                    proc.Refresh();
                    i = proc.MainWindowTitle.IndexOf("SaveDir", StringComparison.Ordinal);
                    if (File.Exists(logFile))
                    {
                        game.SaveDir = ExtractSaveDirFromLogfile(logFile);
                        parent.UpdateSaveDir(game.SaveDir);
                        i = -1;
                        break;
                    }
                    c++;
                }
                catch (Exception ex)
                {
                    parent.model.Errors.Add(ex.Message);
                    break;
                }
            }

            if (i != -1)
            {
                game.SaveDir = proc.MainWindowTitle.Substring(proc.MainWindowTitle.IndexOf("SaveDir", StringComparison.Ordinal) + 9, 8);
                parent.UpdateSaveDir(game.SaveDir);
            }

            if (!proc.HasExited)
            {
                proc.Kill();
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
            if (proc != null)
            {
                if (!proc.HasExited)
                {
                    proc.Kill();
                }
            }
        }
    }
}
