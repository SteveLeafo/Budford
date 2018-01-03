using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using Budford.Control;

namespace Budford.Utilities
{
    /// <summary>
    /// Provides access to NTFS junction points in .Net.
    /// </summary>
    public static class JunctionPoint
    {
        /// <summary>
        /// Creates a junction point from the specified directory to the specified target directory.
        /// </summary>
        /// <remarks>
        /// Only works on NTFS.
        /// </remarks>
        /// <param name="targetDir">The target directory to create</param>
        /// <param name="sourceDir">The source directory to alias</param>
        /// <param name="overwrite">If true overwrites an existing reparse point or empty directory</param>
        /// <exception cref="IOException">Thrown when the junction point could not be created or when
        /// an existing directory was found and <paramref name="overwrite" /> if false</exception>
        public static void Create(string sourceDir, string targetDir, bool overwrite)
        {
            sourceDir = Path.GetFullPath(sourceDir);

            if (!Directory.Exists(sourceDir))
            {
                throw new IOException("Source path does not exist or is not a directory.");
            }

            if (Directory.Exists(targetDir))
            {
                if (!overwrite)
                {
                    throw new IOException("Directory '{targetDir}' already exists.");
                }
            }
            else
            {
                Directory.CreateDirectory(targetDir);
            }

            using (SafeFileHandle handle = OpenReparsePoint(targetDir, NativeMethods.FileAccess.GenericWrite))
            {
                byte[] sourceDirBytes = Encoding.Unicode.GetBytes(NativeMethods.NonInterpretedPathPrefix + Path.GetFullPath(sourceDir));

                NativeMethods.ReparseDataBuffer reparseDataBuffer = new NativeMethods.ReparseDataBuffer();

                reparseDataBuffer.ReparseTag = NativeMethods.IO_REPARSE_TAG_MOUNT_POINT;
                reparseDataBuffer.ReparseDataLength = (ushort)(sourceDirBytes.Length + 12);
                reparseDataBuffer.SubstituteNameOffset = 0;
                reparseDataBuffer.SubstituteNameLength = (ushort)sourceDirBytes.Length;
                reparseDataBuffer.PrintNameOffset = (ushort)(sourceDirBytes.Length + 2);
                reparseDataBuffer.PrintNameLength = 0;
                reparseDataBuffer.PathBuffer = new byte[0x3ff0];
                Array.Copy(sourceDirBytes, reparseDataBuffer.PathBuffer, sourceDirBytes.Length);

                int inBufferSize = Marshal.SizeOf(reparseDataBuffer);
                IntPtr inBuffer = Marshal.AllocHGlobal(inBufferSize);

                try
                {
                    Marshal.StructureToPtr(reparseDataBuffer, inBuffer, false);

                    int bytesReturned;
                    bool result = NativeMethods.DeviceIoControl(handle.DangerousGetHandle(), NativeMethods.FSCTL_SET_REPARSE_POINT,
                        inBuffer, sourceDirBytes.Length + 20, IntPtr.Zero, 0, out bytesReturned, IntPtr.Zero);

                    if (!result)
                    {
                        ThrowLastWin32Error("Unable to create junction point '{sourceDir}' -> '{targetDir}'.");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(inBuffer);
                }
            }
        }

        /// <summary>
        /// Deletes a junction point at the specified source directory along with the directory itself.
        /// Does nothing if the junction point does not exist.
        /// </summary>
        /// <remarks>
        /// Only works on NTFS.
        /// </remarks>
        /// <param name="junctionPoint">The junction point path</param>
        public static void Delete(string junctionPoint)
        {
            if (!Directory.Exists(junctionPoint))
            {
                if (File.Exists(junctionPoint))
                {
                    throw new IOException("Path is not a junction point.");
                }

                return;
            }

            using (SafeFileHandle handle = OpenReparsePoint(junctionPoint, NativeMethods.FileAccess.GenericWrite))
            {
                NativeMethods.ReparseDataBuffer reparseDataBuffer = new NativeMethods.ReparseDataBuffer();

                reparseDataBuffer.ReparseTag = NativeMethods.IO_REPARSE_TAG_MOUNT_POINT;
                reparseDataBuffer.ReparseDataLength = 0;
                reparseDataBuffer.PathBuffer = new byte[0x3ff0];

                int inBufferSize = Marshal.SizeOf(reparseDataBuffer);
                IntPtr inBuffer = Marshal.AllocHGlobal(inBufferSize);
                try
                {
                    Marshal.StructureToPtr(reparseDataBuffer, inBuffer, false);

                    int bytesReturned;
                    bool result = NativeMethods.DeviceIoControl(handle.DangerousGetHandle(), NativeMethods.FSCTL_DELETE_REPARSE_POINT,
                        inBuffer, 8, IntPtr.Zero, 0, out bytesReturned, IntPtr.Zero);

                    if (!result)
                    {
                        ThrowLastWin32Error("Unable to delete junction point.");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(inBuffer);
                }

                try
                {
                    Directory.Delete(junctionPoint);
                }
                catch (IOException ex)
                {
                    throw new IOException("Unable to delete junction point.", ex);
                }
            }
        }

        /// <summary>
        /// Determines whether the specified path exists and refers to a junction point.
        /// </summary>
        /// <param name="path">The junction point path</param>
        /// <returns>True if the specified path represents a junction point</returns>
        /// <exception cref="IOException">Thrown if the specified path is invalid
        /// or some other error occurs</exception>
        public static bool Exists(string path)
        {
            if (!Directory.Exists(path))
            {
                return false;
            }

            using (SafeFileHandle handle = OpenReparsePoint(path, NativeMethods.FileAccess.GenericRead))
            {
                string target = InternalGetTarget(handle);
                return target != null;
            }
        }

        /// <summary>
        /// Gets the target of the specified junction point.
        /// </summary>
        /// <remarks>
        /// Only works on NTFS.
        /// </remarks>
        /// <param name="junctionPoint">The junction point path</param>
        /// <returns>The target of the junction point</returns>
        /// <exception cref="IOException">Thrown when the specified path does not
        /// exist, is invalid, is not a junction point, or some other error occurs</exception>
        public static string GetTarget(string junctionPoint)
        {
            using (SafeFileHandle handle = OpenReparsePoint(junctionPoint, NativeMethods.FileAccess.GenericRead))
            {
                return InternalGetTarget(handle);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        private static string InternalGetTarget(SafeFileHandle handle)
        {
            int outBufferSize = Marshal.SizeOf(typeof(NativeMethods.ReparseDataBuffer));
            IntPtr outBuffer = Marshal.AllocHGlobal(outBufferSize);

            try
            {
                int bytesReturned;
                bool result = NativeMethods.DeviceIoControl(handle.DangerousGetHandle(), NativeMethods.FSCTL_GET_REPARSE_POINT,
                    IntPtr.Zero, 0, outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero);

                if (!result)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == NativeMethods.ERROR_NOT_A_REPARSE_POINT)
                    {
                        return null;
                    }

                    ThrowLastWin32Error("Unable to get information about junction point.");
                }

                NativeMethods.ReparseDataBuffer reparseDataBuffer = (NativeMethods.ReparseDataBuffer)
                    Marshal.PtrToStructure(outBuffer, typeof(NativeMethods.ReparseDataBuffer));

                if (reparseDataBuffer.ReparseTag != NativeMethods.IO_REPARSE_TAG_MOUNT_POINT)
                {
                    return null;
                }

                string targetDir = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer, 
                    reparseDataBuffer.SubstituteNameOffset, reparseDataBuffer.SubstituteNameLength);

                if (targetDir.StartsWith(NativeMethods.NonInterpretedPathPrefix))
                {
                    targetDir = targetDir.Substring(NativeMethods.NonInterpretedPathPrefix.Length);
                }

                return targetDir;
            }
            finally
            {
                Marshal.FreeHGlobal(outBuffer);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reparsePoint"></param>
        /// <param name="accessMode"></param>
        /// <returns></returns>
        private static SafeFileHandle OpenReparsePoint(string reparsePoint, NativeMethods.FileAccess accessMode)
        {
            var handle = NativeMethods.CreateFile(reparsePoint, accessMode,
                NativeMethods.FileShares.Read | NativeMethods.FileShares.Write | NativeMethods.FileShares.Delete,
                IntPtr.Zero, NativeMethods.CreationDisposition.OpenExisting,
                NativeMethods.FileAttributes.BackupSemantics | NativeMethods.FileAttributes.OpenReparsePoint, IntPtr.Zero);

            if (Marshal.GetLastWin32Error() != 0)
            {
                ThrowLastWin32Error("Unable to open reparse point.");
            }

            SafeFileHandle reparsePointHandle = new SafeFileHandle(handle, true);

            return reparsePointHandle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private static void ThrowLastWin32Error(string message)
        {
            throw new IOException(message, Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
        }
    }
}