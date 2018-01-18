using Budford.Control;
using Budford.View;
using System;
using System.IO;
using System.Windows.Forms;
using Budford.Tools;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            //byte[] ba = Encoding.ASCII.GetBytes("@");
            //byte[] b1 = Encoding.ASCII.GetBytes("@");
            //byte[] b2 = Encoding.ASCII.GetBytes("0");

            //UInt32 jx1 = HashGenerator.GenerateHashFromRpxRawData(ba, ba.Length);
            //UInt32 jx2 = HashGenerator.GenerateHashFromRpxRawData(b1, ba.Length);
            //UInt32 jx3 = HashGenerator.GenerateHashFromRpxRawData(b2, ba.Length);
                
            //List<int> xxers = new List<int>();
            //for (int i = 0; i < 256; ++i)
            //{
            //    UInt32 jx = HashGenerator.GenerateHashFromRpxRawData(ba, ba.Length);
            //    UInt64 hasher = HashGenerator.GenerateHashFromRpxRawData2((ulong)i, ba, ba.Length);
            //    if (hasher == 0x713B832AE0DAC43B)
            //    {
            //        xxers.Add(i);
            //    }
            //}
            //Parallel.For(0, long.MaxValue, i =>
            ////for (ulong i = 0; i < ulong.MaxValue; i++)
            //{
            //    UInt64 hasher = HashGenerator.GenerateHashFromRpxRawData2((ulong)i, ba, ba.Length);
            //    if (hasher == 0xeebcd522ec4183d4)
            //    {
            //        MessageBox.Show(i.ToString(), i.ToString());
            //    }
            //});

            //MessageBox.Show("Budford said no");

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
