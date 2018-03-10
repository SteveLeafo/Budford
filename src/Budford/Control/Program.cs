using System;
using System.IO;
using System.Windows.Forms;
using Budford.Properties;
using Budford.View;
using Budford.Utilities;
using Microsoft.Win32;

namespace Budford.Control
{
    internal static class Program
    {
        private static bool isInstalled = true;
        internal static bool IsInstalled()
        {
            return isInstalled;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");

            var arguments = Environment.GetCommandLineArgs();

            if (!arguments[0].Contains(programFiles))
            {
                if (arguments[0].Contains("Budford.exe"))
                {
                    isInstalled = false;
                }
            }

            var model = FormMainWindow.TransferLegacyModel();

            if (!Get45Or451FromRegistry())
            {
                MessageBox.Show(Resources.Program_Main_Needs__NET_4_5_or_higher_to_run, Resources.Program_Main_Please_update_your__NET_run_environment);
            }

            if (IsInstalled())
            {
                SetDefaultFolder(model);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            bool cmdLineLaunch = false;
            bool cmdLineFullScreen = false;
            string cmdLineFileName = string.Empty;
            ParseCommandLineArguments(arguments, ref cmdLineLaunch, ref cmdLineFullScreen, ref cmdLineFileName);

            if (arguments.Length == 2)
            {
                cmdLineLaunch = true;
            }

            FormMainWindow mainForm = new FormMainWindow();

            if (cmdLineFileName == string.Empty || !cmdLineLaunch)
            {
                Application.Run(mainForm);
            }
            else
            {
                mainForm.launchGame = cmdLineFileName;
                mainForm.LaunchFull = cmdLineFullScreen;

                Application.Run(mainForm);
            }
        }

        private static bool Get45Or451FromRegistry()
        {
            try
            {
                using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
                {
                    if (ndpKey != null)
                    {
                        int releaseKey = Convert.ToInt32(ndpKey.GetValue("Release"));
                        return CheckFor45DotVersion(releaseKey);
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing
            }
            return false;
        }

        // Checking the version using >= will enable forward compatibility,  
        // however you should always compile your code on newer versions of 
        // the framework to ensure your app works the same. 
        private static bool CheckFor45DotVersion(int releaseKey)
        {
            if (releaseKey >= 393273)
            {
                return true;
            }
            if ((releaseKey >= 379893))
            {
                return true;
            }
            if ((releaseKey >= 378675))
            {
                return true;
            }
            if ((releaseKey >= 378389))
            {
                return true;
            }
            // This line should never execute. A non-null release key should mean 
            // that 4.5 or later is installed. 
            return false;
        }

        /// <summary>
        /// This is the Budford folder where all the downloaded files will be saved
        /// </summary>
        private static void SetDefaultFolder(Model.Model model)
        {
            if (model.Settings.DownloadsFolder == "")
            {
                if (!CurrentOs.IsWindows)
                {
                    model.Settings.DownloadsFolder = "Budford";
                }
                else
                {
                    model.Settings.DownloadsFolder = "C:\\ProgramData\\Budford";
                }

            }

            if (!Directory.Exists(model.Settings.DownloadsFolder))
            {
                Directory.CreateDirectory(model.Settings.DownloadsFolder);
            }
            Directory.SetCurrentDirectory(model.Settings.DownloadsFolder);
            Persistence.Save(model, FormMainWindow.GetModelFileName());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="cmdLineLaunch"></param>
        /// <param name="cmdLineFullScreen"></param>
        /// <param name="cmdLineFileName"></param>
        private static void ParseCommandLineArguments(string[] arguments, ref bool cmdLineLaunch, ref bool cmdLineFullScreen, ref string cmdLineFileName)
        {
            for (int i = 1; i < arguments.Length; ++i)
            {
                if (arguments[i] == "-f")
                {
                    cmdLineFullScreen = true;
                }
                else if (arguments[i] == "-g")
                {
                    cmdLineLaunch = true;
                }
                else
                {
                    var extension = Path.GetExtension(arguments[i]);
                    if (extension != null && extension.ToLower() == ".rpx")
                    {
                        if (File.Exists(arguments[i]))
                        {
                            cmdLineFileName = arguments[i];
                        }
                    }
                }
            }
        }
    }
}
