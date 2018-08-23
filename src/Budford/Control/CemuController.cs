using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Budford.Control
{
    internal class CemuController
    {
        Dictionary<string, int> menuHandles = new Dictionary<string, int>();

        IntPtr cemuHandle;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        internal CemuController(IntPtr hWnd)
        {
            cemuHandle = hWnd;
            menuHandles = GetMenuLookupTable(NativeMethods.GetMenu(cemuHandle));
        }

        /// <summary>
        /// 
        /// </summary>
        internal void ToggleLogging()
        {
            foreach (var v in menuHandles)
            {
                if (v.Key.Contains("Enable logging"))
                {
                    NativeMethods.SendMessage(cemuHandle, NativeMethods.WM_COMMAND, (IntPtr)v.Value, (IntPtr)0);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sysMenu"></param>
        /// <returns></returns>
        private Dictionary<string, int> GetMenuLookupTable(IntPtr sysMenu)
        {
            Dictionary<string, int> menuHandles = new Dictionary<string, int>();
            int itemCount = NativeMethods.GetMenuItemCount(sysMenu);

            for (int i = 0; i < itemCount; ++i)
            {
                TraverseSubMenus(sysMenu, i, menuHandles);
            }
            return menuHandles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="pos"></param>
        /// <param name="menuHandles"></param>
        void TraverseSubMenus(IntPtr handle, int pos, Dictionary<string, int> menuHandles)
        {
            var menu = NativeMethods.GetSubMenu(handle, pos);
            GetMenuCaption(menuHandles, pos, handle);
            int itemCount = NativeMethods.GetMenuItemCount(menu);

            for (int i = 0; i < itemCount; ++i)
            {
                var subMenu = NativeMethods.GetSubMenu(handle, pos);
                GetMenuCaption(menuHandles, i, subMenu);
                TraverseSubMenus(menu, i, menuHandles);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menuHandles"></param>
        /// <param name="i"></param>
        /// <param name="subMenu"></param>
        /// <returns></returns>
        private static string GetMenuCaption(Dictionary<string, int> menuHandles, int i, IntPtr subMenu)
        {
            string caption = "";
            NativeMethods.MENUITEMINFO mif = new NativeMethods.MENUITEMINFO();
            mif.fMask = NativeMethods.MIIM_STRING;
            mif.fType = NativeMethods.MFT_STRING;

            mif.dwTypeData = IntPtr.Zero;
            bool res = NativeMethods.GetMenuItemInfo(subMenu, i, true, mif);
            if (!res)
            {
                return "";
            }
            mif.cch++;
            mif.dwTypeData = Marshal.AllocHGlobal((IntPtr)(mif.cch * 2));
            try
            {
                res = NativeMethods.GetMenuItemInfo(subMenu, i, true, mif);
                if (!res)
                {
                    return "";
                }
                caption = Marshal.PtrToStringUni(mif.dwTypeData);               
            }
            finally
            {
                Marshal.FreeHGlobal(mif.dwTypeData);
            }
            return caption;
        }
    }
}
