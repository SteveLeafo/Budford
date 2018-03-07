using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Budford.Utilities
{
    public static class CurrentOs
    {
        public static bool IsWindows { get; private set; }
        public static bool IsUnix { get; private set; }
        public static bool IsMac { get; private set; }
        public static bool IsLinux { get; private set; }
        public static bool IsUnknown { get; private set; }
        public static bool Is32Bit { get; private set; }
        public static bool Is64Bit { get; private set; }
        public static bool Is64BitProcess { get { return (IntPtr.Size == 8); } }
        public static bool Is32BitProcess { get { return (IntPtr.Size == 4); } }
        public static string Name { get; private set; }

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool wow64Process);

        private static bool Is64BitWindows
        {
            get
            {
                if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) || Environment.OSVersion.Version.Major >= 6)
                {
                    using (Process p = Process.GetCurrentProcess())
                    {
                        bool retVal;
                        if (!IsWow64Process(p.Handle, out retVal)) return false;
                        return retVal;
                    }
                }
                else return false;
            }
        }

        static CurrentOs()
        {
            IsWindows = Path.DirectorySeparatorChar == '\\';
            if (IsWindows)
            {
                Name = Environment.OSVersion.VersionString;

                Name = Name.Replace("Microsoft ", "");
                Name = Name.Replace("  ", " ");
                Name = Name.Replace(" )", ")");
                Name = Name.Trim();

                Name = Name.Replace("NT 6.2", "8 %bit 6.2");
                Name = Name.Replace("NT 6.1", "7 %bit 6.1");
                Name = Name.Replace("NT 6.0", "Vista %bit 6.0");
                Name = Name.Replace("NT 5.", "XP %bit 5.");
                Name = Name.Replace("%bit", (Is64BitWindows ? "64bit" : "32bit"));

                if (Is64BitWindows)
                    Is64Bit = true;
                else
                    Is32Bit = true;
            }
            else
            {
                string unixName = ReadProcessOutput("uname");
                if (unixName.Contains("Darwin"))
                {
                    IsUnix = true;
                    IsMac = true;

                    Name = "MacOS X " + ReadProcessOutput("sw_vers", "-productVersion");
                    Name = Name.Trim();

                    string machine = ReadProcessOutput("uname", "-m");
                    if (machine.Contains("x86_64"))
                        Is64Bit = true;
                    else
                        Is32Bit = true;

                    Name += " " + (Is32Bit ? "32bit" : "64bit");
                }
                else if (unixName.Contains("Linux"))
                {
                    IsUnix = true;
                    IsLinux = true;

                    Name = ReadProcessOutput("lsb_release", "-d");
                    Name = Name.Substring(Name.IndexOf(":", StringComparison.Ordinal) + 1);
                    Name = Name.Trim();

                    string machine = ReadProcessOutput("uname", "-m");
                    if (machine.Contains("x86_64"))
                        Is64Bit = true;
                    else
                        Is32Bit = true;

                    Name += " " + (Is32Bit ? "32bit" : "64bit");
                }
                else if (unixName != "")
                {
                    IsUnix = true;
                }
                else
                {
                    IsUnknown = true;
                }
            }
        }

        private static string ReadProcessOutput(string name, string args = null)
        {
            try
            {
                Process p = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };
                if (!string.IsNullOrEmpty(args)) p.StartInfo.Arguments = " " + args;
                p.StartInfo.FileName = name;
                p.Start();

                // Read the output stream first and then wait.
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                output = output.Trim();
                return output;
            }
            catch
            {
                return "";
            }
        }
    }
}
