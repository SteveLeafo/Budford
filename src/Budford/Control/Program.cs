using Budford.Control;
using Budford.View;
using System;
using System.IO;
using System.Windows.Forms;

namespace Budford
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!Directory.Exists("C:\\ProgramData\\Budford"))
            {
                Directory.CreateDirectory("C:\\ProgramData\\Budford");
            }
            Directory.SetCurrentDirectory("C:\\ProgramData\\Budford");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var arguments = Environment.GetCommandLineArgs();

            bool cmdLineLaunch = false;
            bool cmdLineFullScreen = false;
            string cmdLineFileName = string.Empty;
            ParseCommandLineArguments(arguments, ref cmdLineLaunch, ref cmdLineFullScreen, ref cmdLineFileName);

            if (cmdLineFileName == string.Empty || cmdLineLaunch == false)
            {
                Application.Run(new FormMainWindow());
            }
            else
            {
                new Launcher(null).LaunchRpx(Persistence.Load(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Model.xml"), cmdLineFileName, cmdLineFullScreen);
            }
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
                    if (Path.GetExtension(arguments[i]).ToLower() == ".rpx")
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
