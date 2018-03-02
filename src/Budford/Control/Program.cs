using System;
using System.IO;
using System.Windows.Forms;
using Budford.View;
using Budford.Utilities;

namespace Budford.Control
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var model = FormMainWindow.TransferLegacyModel();

            SetDefaultFolder(model);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var arguments = Environment.GetCommandLineArgs();

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

                if (model.Settings.StopHotkey != "None")
                {
                    mainForm.launchGame = cmdLineFileName;
                    mainForm.launchFull = cmdLineFullScreen;

                    Application.Run(mainForm);
                }
                else
                {
                    if (model.Settings.AutomaticallyDownloadGraphicsPackOnStart)
                    {
                        CemuFeatures.DownloadLatestGraphicsPack(null, model, false);
                    }
                    foreach (var game in model.GameData)
                    {
                        if (game.Value.LaunchFile.ToLower() == cmdLineFileName.ToLower())
                        {
                            game.Value.Exists = true;
                            new Launcher(null).LaunchCemu(null, model, game.Value, false, false, true, cmdLineFullScreen);
                            return;
                        }
                    }
                    new Launcher(null).LaunchRpx(model, cmdLineFileName, cmdLineFullScreen);
                }
            }
        }

        /// <summary>
        /// This is the Budford folder where all the downloaded files will be saved
        /// </summary>
        private static void SetDefaultFolder(Model.Model model)
        {
            if (model.Settings.DownloadsFolder == "")
            {
                if (!CurrentOS.IsWindows)
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
            Persistence.Save(model, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Budford", "Model.xml"));
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
