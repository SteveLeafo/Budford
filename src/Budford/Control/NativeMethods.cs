using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Budford.Control
{
    internal class NativeMethods
    {
        public enum KeyModifiers
        {
            None = 0,
            // ReSharper disable once UnusedMember.Local
            Alt = 1,
            // ReSharper disable once UnusedMember.Local
            Control = 2,
            // ReSharper disable once UnusedMember.Local
            Shift = 4,
            // ReSharper disable once UnusedMember.Local
            WinKey = 8
        }

        [Flags]
        public enum ErrorModes : uint
        {
            None = 0x0,
            FailCriticalErrors = 0x0001,
            NoAlignmentException = 0x0004,
            NoGpFaultErrorBox = 0x0002,
            NoOpenFileErrorBox = 0x8000
        }

        public enum WindowLongIndex
        {
            ExtendedStyle = -20,
            HandleInstance = -6,
            HandleParent = -8,
            Identifier = -12,
            Style = -16,
            UserData = -21,
            WindowProc = -4
        }

        [Flags]
        public enum WindowStyles : uint
        {
            None = 0x00000000,
            Popup = 0x80000000,
            Child = 0x40000000,
            Minimize = 0x20000000,
            Visible = 0x10000000,
            Disabled = 0x08000000,
            ClipSiblings = 0x04000000,
            ClipChildren = 0x02000000,
            Maximize = 0x01000000,
            Border = 0x00800000,
            DialogFrame = 0x00400000,
            Vscroll = 0x00200000,
            Hscroll = 0x00100000,
            SystemMenu = 0x00080000,
            ThickFrame = 0x00040000,
            Group = 0x00020000,
            Tabstop = 0x00010000,

            MinimizeBox = 0x00020000,
            MaximizeBox = 0x00010000,

            Caption = Border | DialogFrame,
            Tiled = None,
            Iconic = Minimize,
            SizeBox = ThickFrame,
            TiledWindow = None,

            OverlappedWindow = None | Caption | SystemMenu | ThickFrame | MinimizeBox | MaximizeBox,
            ChildWindow = Child,

            ExtendedDlgModalFrame = 0x00000001,
            ExtendedNoParentNotify = 0x00000004,
            ExtendedTopmost = 0x00000008,
            ExtendedAcceptFiles = 0x00000010,
            ExtendedTransparent = 0x00000020,
            ExtendedMdiChild = 0x00000040,
            ExtendedToolWindow = 0x00000080,
            ExtendedWindowEdge = 0x00000100,
            ExtendedClientEdge = 0x00000200,
            ExtendedContextHelp = 0x00000400,
            ExtendedRight = 0x00001000,
            ExtendedLeft = 0x00000000,
            ExtendedRtlReading = 0x00002000,
            ExtendedLtrReading = 0x00000000,
            ExtendedLeftScrollbar = 0x00004000,
            ExtendedRightScrollbar = 0x00000000,
            ExtendedControlParent = 0x00010000,
            ExtendedStaticEdge = 0x00020000,
            ExtendedAppWindow = 0x00040000,
            ExtendedOverlappedWindow = ExtendedWindowEdge | ExtendedClientEdge,
            ExtendedPaletteWindow = ExtendedWindowEdge | ExtendedToolWindow | ExtendedTopmost,
            ExtendedLayered = 0x00080000,
            ExtendedNoinheritLayout = 0x00100000,
            ExtendedLayoutRtl = 0x00400000,
            ExtendedComposited = 0x02000000,
            ExtendedNoActivate = 0x08000000
        }

        [Flags]
        public enum SetWindowPosTypes
        {
            AsyncWindowPos = 0x4000,
            DeferBase = 0x2000,
            DrawFrame = 0x0020,
            FrameChanged = 0x0020,
            HideWindow = 0x0080,
            NoActivate = 0x0010,
            NoCopyBits = 0x0100,
            NoMove = 0x0002,
            NoOwnerZOrder = 0x0200,
            NoReDraw = 0x0008,
            NoRePosition = 0x0200,
            NoSendChanging = 0x0400,
            NoSize = 0x0001,
            NoZOrder = 0x0004,
            ShowWindow = 0x0040
        }

        [Flags]
        public enum Menus
        {
            ByPosition = 0x00000400,
            Remove = 0x00001000
        }

        [DllImport("user32.dll")]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("kernel32.dll")]
        static extern ErrorModes SetErrorMode(ErrorModes uMode);

        [DllImport("user32.dll")]
        internal static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("User32.dll")]
        internal static extern int SetForegroundWindow(IntPtr point);

        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        /// <summary>
        /// 
        /// </summary>
        internal static void SurpressOsErrors()
        {
            SetErrorMode(ErrorModes.FailCriticalErrors | ErrorModes.NoGpFaultErrorBox | ErrorModes.NoOpenFileErrorBox);
        }

        /// <summary>
        /// The file or directory is not a reparse point.
        /// </summary>
        internal const int ErrorNotAReparsePoint = 4390;

        /// <summary>
        /// The reparse point attribute cannot be set because it conflicts with an existing attribute.
        /// </summary>
        internal const int ErrorReparseAttributeConflict = 4391;

        /// <summary>
        /// The data present in the reparse point buffer is invalid.
        /// </summary>
        internal const int ErrorInvalidReparseData = 4392;

        /// <summary>
        /// The tag present in the reparse point buffer is invalid.
        /// </summary>
        internal const int ErrorReparseTagInvalid = 4393;

        /// <summary>
        /// There is a mismatch between the tag specified in the request and the tag present in the reparse point.
        /// </summary>
        internal const int ErrorReparseTagMismatch = 4394;

        /// <summary>
        /// Command to set the reparse point data block.
        /// </summary>
        internal const int FsctlSetReparsePoint = 0x000900A4;

        /// <summary>
        /// Command to get the reparse point data block.
        /// </summary>
        internal const int FsctlGetReparsePoint = 0x000900A8;

        /// <summary>
        /// Command to delete the reparse point data base.
        /// </summary>
        internal const int FsctlDeleteReparsePoint = 0x000900AC;

        /// <summary>
        /// Reparse point tag used to identify mount points and junction points.
        /// </summary>
        internal const uint IoReparseTagMountPoint = 0xA0000003;

        /// <summary>
        /// This prefix indicates to NTFS that the path is to be treated as a non-interpreted
        /// path in the virtual file system.
        /// </summary>
        internal const string NonInterpretedPathPrefix = @"\??\";

        [Flags]
        internal enum FileAccess : uint
        {
            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000,
        }

        [Flags]
        internal enum FileShares : uint
        {
            None = 0x00000000,
            Read = 0x00000001,
            Write = 0x00000002,
            Delete = 0x00000004,
        }

        internal enum CreationDisposition : uint
        {
            New = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5,
        }

        [Flags]
        internal enum FileAttributes : uint
        {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            WriteThrough = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ReparseDataBuffer
        {
            /// <summary>
            /// Reparse point tag. Must be a Microsoft reparse point tag.
            /// </summary>
            public uint ReparseTag;

            /// <summary>
            /// Size, in bytes, of the data after the Reserved member. This can be calculated by:
            /// (4 * sizeof(ushort)) + SubstituteNameLength + PrintNameLength + 
            /// (namesAreNullTerminated ? 2 * sizeof(char) : 0);
            /// </summary>
            public ushort ReparseDataLength;

            /// <summary>
            /// Reserved; do not use. 
            /// </summary>
            public ushort Reserved;

            /// <summary>
            /// Offset, in bytes, of the substitute name string in the PathBuffer array.
            /// </summary>
            public ushort SubstituteNameOffset;

            /// <summary>
            /// Length, in bytes, of the substitute name string. If this string is null-terminated,
            /// SubstituteNameLength does not include space for the null character.
            /// </summary>
            public ushort SubstituteNameLength;

            /// <summary>
            /// Offset, in bytes, of the print name string in the PathBuffer array.
            /// </summary>
            public ushort PrintNameOffset;

            /// <summary>
            /// Length, in bytes, of the print name string. If this string is null-terminated,
            /// PrintNameLength does not include space for the null character. 
            /// </summary>
            public ushort PrintNameLength;

            /// <summary>
            /// A buffer containing the unicode-encoded path string. The path string contains
            /// the substitute name string and print name string.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)]
            public byte[] PathBuffer;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
            IntPtr inBuffer, int nInBufferSize,
            IntPtr outBuffer, int nOutBufferSize,
            out int pBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateFile(
            string lpFileName,
            FileAccess dwDesiredAccess,
            FileShares dwShareMode,
            IntPtr lpSecurityAttributes,
            CreationDisposition dwCreationDisposition,
            FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPosTypes uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetMenu(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetMenuItemCount(IntPtr hMenu);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DrawMenuBar(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, Menus uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPosTypes wFlags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern WindowStyles GetWindowLong32(IntPtr hWnd, WindowLongIndex nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern WindowStyles GetWindowLong64(IntPtr hWnd, WindowLongIndex nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern WindowStyles SetWindowLong32(IntPtr hWnd, WindowLongIndex nIndex, WindowStyles dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern WindowStyles SetWindowLong64(IntPtr hWnd, WindowLongIndex nIndex, WindowStyles dwNewLong);

        public static WindowStyles SetWindowLong(IntPtr hWnd, WindowLongIndex nIndex, WindowStyles dwNewLong)
        {
            return IntPtr.Size == 8 ? SetWindowLong64(hWnd, nIndex, dwNewLong) : SetWindowLong32(hWnd, nIndex, dwNewLong);
        }

        public static WindowStyles GetWindowLong(IntPtr hWnd, WindowLongIndex nIndex)
        {
            return IntPtr.Size == 8 ? GetWindowLong64(hWnd, nIndex) : GetWindowLong32(hWnd, nIndex);
        }

        /// <summary> Get the text for the window pointed to by hWnd </summary>
        public static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size > 0)
            {
                var builder = new StringBuilder(size + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return String.Empty;
        }

        /// <summary> Find all windows that match the given filter </summary>
        /// <param name="filter"> A delegate that returns true for windows
        ///    that should be returned and false for windows that should
        ///    not be returned </param>
        public static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
        {
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate(IntPtr wnd, IntPtr param)
            {
                if (filter(wnd, param))
                {
                    // only add the windows that pass the filter
                    windows.Add(wnd);
                }

                // but return true here so that we iterate all windows
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary> Find all windows that contain the given title text </summary>
        /// <param name="titleText"> The text that the window title must contain. </param>
        public static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
        {
            return FindWindows(delegate(IntPtr wnd, IntPtr param)
            {
                return GetWindowText(wnd).Contains(titleText);
            });
        } 
    }
}
