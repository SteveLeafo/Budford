using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using Budford.Properties;
using Budford.Utilities;
using Budford.View;

namespace Budford.Control
{
    internal class Unpacker
    {
        // The parent form
        readonly Form owner;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ownerIn"></param>
        internal Unpacker(Form ownerIn)
        {
            owner = ownerIn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="uri"></param>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        internal void DownloadAndUnpack(string fileName, string uri, string folder, string name)
        {
            if (File.Exists(fileName))
            {
                MessageBox.Show(owner, Resources.Unpacker_DownloadAndUnpack_The_latest_version_of_ + name + Resources.Unpacker_DownloadAndUnpack__has_already_been_installed, Resources.Unpacker_DownloadAndUnpack_Info);
            }
            else
            {
                Logger.Log("Downloading " + uri);
                using (FormFileDownload fileDownload = new FormFileDownload(uri, fileName, folder))
                {
                    fileDownload.ShowDialog(owner);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="folder"></param>
        internal void Unpack(string fileName, string folder)
        {
            FileManager.SafeCreateDirectory(folder);

            if (File.Exists(fileName))
            {
                ExtractToDirectory(fileName, folder, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="destinationDirectoryName"></param>
        /// <param name="overwrite"></param>
        static public void ExtractToDirectory(string archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                ZipFile.ExtractToDirectory(archive, destinationDirectoryName);
                return;
            }

            if (!File.Exists(archive))
            {
                return;
            }

            if (destinationDirectoryName == null)
            {
                return;
            }

            if (archive == null)
            {
                return;
            }

            ZipArchive zipArchive = ZipFile.OpenRead(archive);
            foreach (ZipArchiveEntry file in zipArchive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                string directory = Path.GetDirectoryName(completeFileName);

                FileManager.SafeCreateDirectory(directory);

                if (file.Name != "")
                {
                    ExtractFile(file, completeFileName);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="completeFileName"></param>
        internal static void ExtractFile(ZipArchiveEntry file, string completeFileName)
        {
            try
            {
                CurrentUserSecurity cs = new CurrentUserSecurity();
                if (File.Exists(completeFileName))
                {
                    if (cs.HasAccess(new FileInfo(completeFileName), System.Security.AccessControl.FileSystemRights.Write))
                    {
                        file.ExtractToFile(completeFileName, true);
                    }
                }
                else
                {
                    file.ExtractToFile(completeFileName, true);
                }
            }
            catch (System.Exception)
            {
                // ignored
            }
        }
    }
}
