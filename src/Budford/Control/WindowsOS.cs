using Microsoft.Win32;
using System.Windows.Forms;
using Budford.Properties;
using Budford.View;

namespace Budford.Control
{
    internal static class WindowsOs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool IsVc2015RedistInstalled()
        {
            using (FormFileDownload dl = new FormFileDownload("https://download.microsoft.com/download/6/A/A/6AA4EDFF-645B-48C5-81CC-ED5963AEAD48/vc_redist.x64.exe", "vc_redist.x64.exe"))
            {
                dl.ShowDialog();
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool IsVc2013RedistInstalled()
        {
            RegistryKey winLogonKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Installer\Dependencies\{ca67548a-5ebe-413a-b50c-4b9ceb6d66c6}", false);

            if (winLogonKey != null)
            {
                MessageBox.Show(Resources.WindowsOS_IsVC2013RedistInstalled_Visual_C___2013_Redistributable_is_installed_);
                return true;
            }

            MessageBox.Show(Resources.WindowsOs_IsVc2013RedistInstalled_Visual_C___2013_Redistributable_is_not_installed_);
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool IsVc2012RedistInstalled()
        {
            RegistryKey winLogonKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Installer\Dependencies\{d992c12e-cab2-426f-bde3-fb8c53950b0d}", false);

            if (winLogonKey != null)
            {
                MessageBox.Show(Resources.WindowsOs_IsVc2012RedistInstalled_Visual_C___2012_Redistributable_is_installed_);
                return true;
            }

            MessageBox.Show(Resources.WindowsOs_IsVc2012RedistInstalled_Visual_C___2012_Redistributable_is_not_installed_);
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool IsVc2010RedistInstalled()
        {
            RegistryKey winLogonKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Installer\Products\1926E8D15D0BCE53481466615F760A7F", false);

            if (winLogonKey != null)
            {
                MessageBox.Show(Resources.WindowsOs_IsVc2010RedistInstalled_Visual_C___2010_Redistributable_is_installed_);
                return true;
            }

            MessageBox.Show(Resources.WindowsOs_IsVc2010RedistInstalled_Visual_C___2010_Redistributable_is_not_installed_);
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool IsVc2008RedistInstalled()
        {
            RegistryKey winLogonKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Installer\Products\67D6ECF5CD5FBA732B8B22BAC8DE1B4D", false);

            if (winLogonKey != null)
            {
                MessageBox.Show(Resources.WindowsOs_IsVc2008RedistInstalled_Visual_C___2008_Redistributable_is_installed_);
                return true;
            }

            MessageBox.Show(Resources.WindowsOs_IsVc2008RedistInstalled_Visual_C___2008_Redistributable_is_not_installed_);
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool IsVc2005RedistInstalled()
        {
            RegistryKey winLogonKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Installer\Products\1af2a8da7e60d0b429d7e6453b3d0182", false);

            if (winLogonKey != null)
            {
                MessageBox.Show(Resources.WindowsOs_IsVc2005RedistInstalled_Visual_C___2005_Redistributable_is_installed_);
                return true;
            }

            MessageBox.Show(Resources.WindowsOs_IsVc2005RedistInstalled_Visual_C___2005_Redistributable_is_not_installed_);
            return false;
        }
    }
}
