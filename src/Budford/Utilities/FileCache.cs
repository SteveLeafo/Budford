using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Budford.Utilities
{
    internal class FileCache
    {
        const uint FILECACHE_MAGIC = 0x8371b694;
        const uint FILECACHE_MAGIC_V2 = 0x8371b695;
        const uint FILECACHE_HEADER_RESV = 128; // number of bytes reserved for the header
        const UInt64 FILECACHE_FILETABLE_NAME1 = 0xEFEFEFEFEFEFEFEF;
        const UInt64 FILECACHE_FILETABLE_NAME2 = 0xFEFEFEFEFEFEFEFE;
        const uint FILECACHE_FILETABLE_FREE_NAME = 0;

        const int sizeof_fileCacheEntry_t = 32;

        public BinaryReader stream_file;
        public BinaryWriter stream_file2;
        public UInt64 dataOffset;
        public UInt32 extraVersion;
        // file table
        public FileCacheEntry[] fileTableEntries;
        public UInt32 fileTableEntryCount;
        // file table (as stored in file)
        public UInt64 fileTableOffset;
        public UInt32 fileTableSize;
        // threadsafe
        public UInt32 cs = 0;

        public static FileCache fileCache_create(string path, UInt32 extraVersion)
        {
            FileCache fileCache = new FileCache();
            BinaryWriter stream_file = new BinaryWriter(File.Open(path, FileMode.Create));
            if (stream_file == null)
            {
                //forceLog_printf("Failed to create cache file \"%ls\"", path);
                return null;
            }
            // init file cache
            //memset(fileCache, 0x00, sizeof(fileCache_t));
            //InitializeCriticalSection(&fileCache.cs);
            fileCache.stream_file2 = stream_file;
            fileCache.dataOffset = FILECACHE_HEADER_RESV;
            fileCache.fileTableEntryCount = 32;
            fileCache.fileTableOffset = 0;
            fileCache.fileTableSize = sizeof_fileCacheEntry_t * fileCache.fileTableEntryCount;
            fileCache.fileTableEntries = new FileCacheEntry[fileCache.fileTableEntryCount];
            for (Int32 f = 0; f < (Int32)fileCache.fileTableEntryCount; f++)
            {
                fileCache.fileTableEntries[f] = new FileCacheEntry();
            }
            //fileCache.fileTableEntries = new FileCacheEntry[fileCache.fileTableEntries];
            //memset(fileCache->fileTableEntries, 0, fileCache->fileTableSize);
            fileCache.extraVersion = extraVersion;
            // file table stores info about itself
            fileCache.fileTableEntries[0].name1 = FILECACHE_FILETABLE_NAME1;
            fileCache.fileTableEntries[0].name2 = FILECACHE_FILETABLE_NAME2;
            fileCache.fileTableEntries[0].fileOffset = fileCache.fileTableOffset;
            fileCache.fileTableEntries[0].fileSize = fileCache.fileTableSize;
            // write header
            stream_writeU32(stream_file, FILECACHE_MAGIC_V2);
            stream_writeU32(fileCache.stream_file2, fileCache.extraVersion);
            stream_writeU64(fileCache.stream_file2, fileCache.dataOffset);
            stream_writeU64(fileCache.stream_file2, fileCache.fileTableOffset);
            stream_writeU32(fileCache.stream_file2, fileCache.fileTableSize);
            // write file table
            stream_setSeek64(fileCache.stream_file2, fileCache.dataOffset + fileCache.fileTableOffset);
            stream_writeData(fileCache.stream_file2, fileCache.fileTableEntries, fileCache.fileTableEntryCount);

            // done
            return fileCache;
        }

        public static FileCache fileCache_openExisting(string path, UInt32 extraVersion)
        {
            FileCache fileCache = new FileCache();

            BinaryReader stream_file = new BinaryReader(File.Open(path, FileMode.Open));
            UInt32 headerMagic = stream_readU32(stream_file);
            bool isV1 = false;
            if (headerMagic != FILECACHE_MAGIC && headerMagic != FILECACHE_MAGIC_V2)
            {
                stream_destroy(stream_file);
                return null;
            }
            if (headerMagic == FILECACHE_MAGIC)
            {
                isV1 = true;
            }

            UInt32 headerExtraVersion = stream_readU32(stream_file);
            if (headerExtraVersion != extraVersion)
            {
                stream_destroy(stream_file);
                return null;
            }

            UInt64 headerDataOffset = 0;
            if (isV1)
            {
                headerDataOffset = stream_readU32(stream_file);
            }
            else
            {
                headerDataOffset = stream_readU64(stream_file);
            }
            UInt64 headerFileTableOffset;
            if (isV1)
            {
                headerFileTableOffset = stream_readU32(stream_file);
            }
            else
            {
                headerFileTableOffset = stream_readU64(stream_file);
            }

            UInt32 headerFileTableSize = stream_readU32(stream_file);
            //if( (headerFileTableSize%sizeof(fileCacheEntry_t)) != 0 )
            //  return NULL;
            UInt32 fileTableEntryCount = 0;
            bool invalidFileTableSize = false;
            if (isV1)
            {
                fileTableEntryCount = headerFileTableSize / 24;
                invalidFileTableSize = (headerFileTableSize % 24) != 0;
            }
            else
            {
                fileTableEntryCount = headerFileTableSize / sizeof_fileCacheEntry_t;
                invalidFileTableSize = (headerFileTableSize % sizeof_fileCacheEntry_t) != 0;
            }

            if (invalidFileTableSize)
            {
                Console.WriteLine("\"{0}\" is corrupted", path);
                stream_destroy(stream_file);
                return null;
            }

            InitializeCriticalSection(fileCache.cs);
            fileCache.stream_file = stream_file;
            fileCache.extraVersion = extraVersion;
            fileCache.dataOffset = headerDataOffset;
            fileCache.fileTableEntryCount = fileTableEntryCount;
            fileCache.fileTableOffset = headerFileTableOffset;
            fileCache.fileTableSize = fileTableEntryCount * sizeof_fileCacheEntry_t;
            fileCache.fileTableEntries = new FileCacheEntry[fileTableEntryCount];

            stream_setSeek64(stream_file, fileCache.dataOffset + fileCache.fileTableOffset);

            if (isV1)
            {
                // read file table entries in old format
                for (UInt32 i = 0; i < fileTableEntryCount; i++)
                {
                    UInt64 name1 = stream_readU64(stream_file);
                    UInt64 name2 = stream_readU64(stream_file);
                    UInt32 fileOffset = stream_readU32(stream_file);
                    UInt32 fileSize = stream_readU32(stream_file);
                    fileCache.fileTableEntries[i].name1 = name1;
                    fileCache.fileTableEntries[i].name2 = name2;
                    fileCache.fileTableEntries[i].fileOffset = fileOffset;
                    fileCache.fileTableEntries[i].fileSize = fileSize;
                    fileCache.fileTableEntries[i].extraReserved = 0;
                }
            }
            else
            {
                stream_readData(stream_file, fileCache.fileTableEntries, fileTableEntryCount);
            }
            // upgrade file table and header if V1
            if (isV1)
            {
                fileCache_updateFiletable(fileCache, 0);
            }

            return fileCache;
        }

        void fileCache_close(FileCache fileCache)
        {
            //free(fileCache->fileTableEntries);
            fileCache.stream_file.Dispose();// stream_destroy(fileCache->stream_file);
            fileCache.stream_file2.Dispose();
            //free(fileCache);
        }

        private static void fileCache_updateFiletable(FileCache fileCache, Int32 extraEntriesToAllocate)
        {
            // recreate file table with bigger size (optional)
            fileCache.fileTableEntries[0].name1 = FILECACHE_FILETABLE_FREE_NAME;
            fileCache.fileTableEntries[0].name2 = FILECACHE_FILETABLE_FREE_NAME;
            Int32 newFileTableEntryCount = (Int32)fileCache.fileTableEntryCount + extraEntriesToAllocate;
            //fileCache.fileTableEntries = (fileCacheEntry_t*)realloc(fileCache.fileTableEntries, sizeof_fileCacheEntry_t * newFileTableEntryCount);
            //fileCache.fileTableEntries = new FileCacheEntry[fileCache.fileTableSize];
            Array.Resize<FileCacheEntry>(ref fileCache.fileTableEntries, newFileTableEntryCount);
            for (Int32 f = (Int32)fileCache.fileTableEntryCount; f < newFileTableEntryCount; f++)
            {
                fileCache.fileTableEntries[f] = new FileCacheEntry();
                fileCache.fileTableEntries[f].name1 = FILECACHE_FILETABLE_FREE_NAME;
                fileCache.fileTableEntries[f].name2 = FILECACHE_FILETABLE_FREE_NAME;
                fileCache.fileTableEntries[f].fileOffset = 0;
                fileCache.fileTableEntries[f].fileSize = 0;
            }
            fileCache.fileTableEntryCount = (UInt32)newFileTableEntryCount;

            byte[] fileTable = AsByteArray(fileCache.fileTableEntries);
            fileCache_addFile(fileCache, FILECACHE_FILETABLE_NAME1, FILECACHE_FILETABLE_NAME2, fileTable, (int)(sizeof_fileCacheEntry_t * newFileTableEntryCount));

            // update file table info in struct
            if (fileCache.fileTableEntries[0].name1 != FILECACHE_FILETABLE_NAME1 || fileCache.fileTableEntries[0].name2 != FILECACHE_FILETABLE_NAME2)
            {
                __debugbreak();
            }
            fileCache.fileTableOffset = fileCache.fileTableEntries[0].fileOffset;
            fileCache.fileTableSize = fileCache.fileTableEntries[0].fileSize;
            // update header
            stream_setSeek64(fileCache.stream_file2, 0);
            stream_writeU32(fileCache.stream_file2, FILECACHE_MAGIC_V2);
            stream_writeU32(fileCache.stream_file2, fileCache.extraVersion);
            stream_writeU64(fileCache.stream_file2, fileCache.dataOffset);
            stream_writeU64(fileCache.stream_file2, fileCache.fileTableOffset);
            stream_writeU32(fileCache.stream_file2, fileCache.fileTableSize);
        }

        private static byte[] AsByteArray(FileCacheEntry[] fileCacheEntry)
        {
            byte[] bytes = new byte[fileCacheEntry.Length * 32];
            for (int i = 0; i < fileCacheEntry.Length; ++i)
            {
                fileCacheEntry[i].GetBytes().CopyTo(bytes, i * 32);
            }
            return bytes;
        }

        public static void fileCache_addFile(FileCache fileCache, UInt64 name1, UInt64 name2, byte[] fileData, Int32 fileSize, UInt32 extraReserved = 0)
        {
            if (fileCache == null)
            {
                return;
            }
            EnterCriticalSection(fileCache.cs);
            // find free entry in file table
            Int32 entryIndex = -1;
            // scan for already existing entry
            for (Int32 i = 0; i < fileCache.fileTableEntryCount; i++)
            {
                if (fileCache.fileTableEntries[i].name1 == name1 && fileCache.fileTableEntries[i].name2 == name2)
                {
                    entryIndex = i;
                    // note: We don't delete the entry right here to avoid it being overwritten before the new file is added
                    break;
                }
            }
            if (entryIndex == -1)
            {
                while (true)
                {
                    // if no entry exists, search for empty one
                    for (Int32 i = 0; i < fileCache.fileTableEntryCount; i++)
                    {
                        if (fileCache.fileTableEntries[i].name1 == FILECACHE_FILETABLE_FREE_NAME && fileCache.fileTableEntries[i].name2 == FILECACHE_FILETABLE_FREE_NAME)
                        {
                            entryIndex = i;
                            break;
                        }
                    }
                    if (entryIndex == -1)
                    {
                        if (name1 == FILECACHE_FILETABLE_NAME1 && name2 == FILECACHE_FILETABLE_NAME2)
                        {
                            __debugbreak();
                        }
                        // no free entry, recreate file table with bigger size
                        fileCache_updateFiletable(fileCache, 64);
                        // try again
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // find free space
            UInt64 currentStartOffset = 0;
            int s0 = 0;
            while (true)
            {
                bool hasCollision = false;
                UInt64 currentEndOffset = currentStartOffset + (UInt64)fileSize;
                //FileCacheEntry_t* entry = fileCache.fileTableEntries;
                //FileCacheEntry_t* entryLast = fileCache.fileTableEntries + fileCache.fileTableEntryCount;
                int entry = s0;
                int entryLast = (int)fileCache.fileTableEntryCount;
                while (entry < entryLast)
                {
                    if (fileCache.fileTableEntries[entry].name1 == FILECACHE_FILETABLE_FREE_NAME && fileCache.fileTableEntries[entry].name2 == FILECACHE_FILETABLE_FREE_NAME)
                    {
                        entry++;
                        continue;
                    }
                    if (currentEndOffset >= fileCache.fileTableEntries[entry].fileOffset && currentStartOffset < fileCache.fileTableEntries[entry].fileOffset + fileCache.fileTableEntries[entry].fileSize)
                    {
                        currentStartOffset = fileCache.fileTableEntries[entry].fileOffset + (UInt64)fileCache.fileTableEntries[entry].fileSize;
                        hasCollision = true;
                        s0 = 0;
                        break;
                    }
                    s0 = entry;
                    entry++;
                }
                // special logic to speed up scanning for free offsets
                // assumes that most of the time entries are stored in direct succession (unless entries are frequently deleted)
                if (hasCollision && (entry + 1) < entryLast)
                {
                    entry++;
                    while (entry < entryLast)
                    {
                        if (fileCache.fileTableEntries[entry].name1 == FILECACHE_FILETABLE_FREE_NAME && fileCache.fileTableEntries[entry].name2 == FILECACHE_FILETABLE_FREE_NAME)
                        {
                            entry++;
                            continue;
                        }
                        if ((UInt64)fileCache.fileTableEntries[entry].fileOffset == currentStartOffset)
                        {
                            currentStartOffset = (UInt64)fileCache.fileTableEntries[entry].fileOffset + fileCache.fileTableEntries[entry].fileSize;
                            entry++;
                            continue;
                        }
                        break;
                    }
                }
                // retry in case of collision
                if (hasCollision == false)
                {
                    s0 = 0;
                    break;
                }
                //if (entry > 128)
                //{
                //    s0 = entry - 128;
                //}
            }
            // update file table entry
            fileCache.fileTableEntries[entryIndex].name1 = name1;
            fileCache.fileTableEntries[entryIndex].name2 = name2;
            fileCache.fileTableEntries[entryIndex].extraReserved = extraReserved;
            fileCache.fileTableEntries[entryIndex].fileOffset = (UInt64)currentStartOffset;
            fileCache.fileTableEntries[entryIndex].fileSize = (UInt32)fileSize;
            // write file data
            stream_setSeek64(fileCache.stream_file2, fileCache.dataOffset + (UInt64)currentStartOffset);
            stream_writeData(fileCache.stream_file2, fileData, fileSize);
            // write file table entry
            stream_setSeek64(fileCache.stream_file2, fileCache.dataOffset + fileCache.fileTableOffset + (UInt64)(sizeof_fileCacheEntry_t * entryIndex));

            stream_writeData(fileCache.stream_file2, fileCache.fileTableEntries, fileCache.fileTableEntryCount);
            LeaveCriticalSection(fileCache.cs);
        }

        public static bool fileCache_deleteFile(FileCache fileCache, UInt64 name1, UInt64 name2)
        {
            if (fileCache == null)
            {
                return false;
            }
            if (name1 == FILECACHE_FILETABLE_NAME1 && name2 == FILECACHE_FILETABLE_NAME2)
            {
                return false; // make sure the filetable is not accidentally deleted
            }
            EnterCriticalSection(fileCache.cs);
            //fileCacheEntry_t* entry = fileCache->fileTableEntries;
            //fileCacheEntry_t* entryLast = fileCache->fileTableEntries + fileCache->fileTableEntryCount;
            int entry = 0;
            uint entryLast = fileCache.fileTableEntryCount;
            while (entry < entryLast)
            {
                if (fileCache.fileTableEntries[entry].name1 == name1 && fileCache.fileTableEntries[entry].name2 == name2)
                {
                    fileCache.fileTableEntries[entry].name1 = FILECACHE_FILETABLE_FREE_NAME;
                    fileCache.fileTableEntries[entry].name2 = FILECACHE_FILETABLE_FREE_NAME;
                    fileCache.fileTableEntries[entry].fileOffset = 0;
                    fileCache.fileTableEntries[entry].fileSize = 0;
                    // store updated entry to file cache
                    Int32 entryIndex = entry; ;
                    stream_setSeek64(fileCache.stream_file, fileCache.dataOffset + fileCache.fileTableOffset + (UInt64)(sizeof_fileCacheEntry_t * entryIndex));
                    //TODOstream_writeData(fileCache.stream_file, fileCache.fileTableEntries[entryIndex], sizeof_fileCacheEntry_t);
                    LeaveCriticalSection(fileCache.cs);
                    return true;
                }
                entry++;
            }
            LeaveCriticalSection(fileCache.cs);
            return false;
        }

        public static byte[] fileCache_getFile(FileCache fileCache, UInt64 name1, UInt64 name2, ref Int32 fileSize)
        {
            if (fileCache == null)
            {
                return null;
            }
            EnterCriticalSection(fileCache.cs);
            //fileCacheEntry_t* entry = fileCache.fileTableEntries;
            //fileCacheEntry_t* entryLast = fileCache.fileTableEntries + fileCache.fileTableEntryCount;
            int entry = 0;
            uint entryLast = fileCache.fileTableEntryCount;
            while (entry < entryLast)
            {
                if (fileCache.fileTableEntries[entry].name1 == name1 && fileCache.fileTableEntries[entry].name2 == name2)
                {
                    fileSize = (Int32)fileCache.fileTableEntries[entry].fileSize;
                    //uint8* fileData = (uint8*)malloc(fileCache.fileTableEntries[entry].fileSize);
                    byte[] fileData = new byte[fileCache.fileTableEntries[entry].fileSize];
                    stream_setSeek64(fileCache.stream_file, fileCache.dataOffset + fileCache.fileTableEntries[entry].fileOffset);
                    stream_readData(fileCache.stream_file, fileData, fileCache.fileTableEntries[entry].fileSize);
                    LeaveCriticalSection(fileCache.cs);
                    return fileData;
                }
                entry++;
            }
            LeaveCriticalSection(fileCache.cs);
            return null;
        }

        private static void stream_readData(BinaryReader binaryReader, byte[] fileData, uint p)
        {
            binaryReader.Read(fileData, 0, (int)p);
        }

        public static Int32 fileCache_getFileEntryCount(FileCache fileCache)
        {
            if (fileCache == null)
            {
                return 0;
            }
            return (Int32)fileCache.fileTableEntryCount;
        }

        public static Int32 fileCache_countFileEntries(FileCache fileCache)
        {
            if (fileCache == null)
            {
                return 0;
            }
            EnterCriticalSection(fileCache.cs);
            Int32 fileCount = 0;
            //fileCacheEntry_t* entry = fileCache->fileTableEntries;
            //fileCacheEntry_t* entryLast = fileCache->fileTableEntries + fileCache->fileTableEntryCount;
            int entry = 0;
            int entryLast = (int)fileCache.fileTableEntryCount;
            while (entry < entryLast)
            {
                if (fileCache.fileTableEntries[entry].name1 == FILECACHE_FILETABLE_FREE_NAME && fileCache.fileTableEntries[entry].name2 == FILECACHE_FILETABLE_FREE_NAME)
                {
                    entry++;
                    continue;
                }
                if (fileCache.fileTableEntries[entry].name1 == FILECACHE_FILETABLE_NAME1 && fileCache.fileTableEntries[entry].name2 == FILECACHE_FILETABLE_NAME2)
                {
                    entry++;
                    continue;
                }
                fileCount++;
                entry++;
            }
            LeaveCriticalSection(fileCache.cs);
            return fileCount;
        }

        private static void LeaveCriticalSection(uint p)
        {

        }

        private static void stream_writeData(BinaryWriter binaryWriter, byte[] fileData, int fileSize)
        {
            binaryWriter.Write(fileData);
        }

        private static void __debugbreak()
        {
            //throw new NotImplementedException();
        }

        private static void EnterCriticalSection(uint p)
        {

        }
        private static void stream_writeData(BinaryWriter stream_file2, FileCacheEntry[] fileCacheEntry, uint p)
        {
            for (UInt32 i = 0; i < p; i++)
            {
                stream_writeU64(stream_file2, fileCacheEntry[i].name1);
                stream_writeU64(stream_file2, fileCacheEntry[i].name2);
                stream_writeU64(stream_file2, fileCacheEntry[i].fileOffset);
                stream_writeU32(stream_file2, fileCacheEntry[i].fileSize);
                stream_writeU32(stream_file2, fileCacheEntry[i].extraReserved);
            }
        }

        private static void stream_setSeek64(BinaryWriter stream_file2, ulong p)
        {
            stream_file2.BaseStream.Position = (long)p;
        }

        private static void stream_writeU64(BinaryWriter binaryWriter, ulong p)
        {
            binaryWriter.Write(p);
        }

        private static void stream_writeU32(BinaryWriter stream_file, uint FILECACHE_MAGIC_V2)
        {
            stream_file.Write(FILECACHE_MAGIC_V2);
        }



        private static void stream_readData(BinaryReader stream_file, FileCacheEntry[] entries, uint p)
        {
            for (UInt32 i = 0; i < p; i++)
            {
                UInt64 name1 = stream_readU64(stream_file);
                UInt64 name2 = stream_readU64(stream_file);
                UInt64 fileOffset = stream_readU64(stream_file);
                UInt32 fileSize = stream_readU32(stream_file);
                UInt32 extraReserved = stream_readU32(stream_file);
                entries[i] = new FileCacheEntry();
                entries[i].name1 = name1;
                entries[i].name2 = name2;
                entries[i].fileOffset = fileOffset;
                entries[i].fileSize = fileSize;
                entries[i].extraReserved = extraReserved;
            }
        }


        private static void stream_setSeek64(BinaryReader stream_file, ulong p)
        {
            stream_file.BaseStream.Position = (long)p;
        }

        private static void InitializeCriticalSection(uint p)
        {
        }

        private static void stream_destroy(BinaryReader stream_file)
        {
        }

        private static uint stream_readU32(BinaryReader stream_file)
        {
            return stream_file.ReadUInt32();
        }

        private static UInt64 stream_readU64(BinaryReader stream_file)
        {
            return stream_file.ReadUInt64();
        }
    }
}
