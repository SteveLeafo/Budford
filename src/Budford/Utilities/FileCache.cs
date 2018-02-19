using System;
using System.IO;

namespace Budford.Utilities
{
    internal class FileCache
    {
        const uint FilecacheMagic = 0x8371b694;
        const uint FilecacheMagicV2 = 0x8371b695;
        const uint FilecacheHeaderResv = 128; // number of bytes reserved for the header
        const UInt64 FilecacheFiletableName1 = 0xEFEFEFEFEFEFEFEF;
        const UInt64 FilecacheFiletableName2 = 0xFEFEFEFEFEFEFEFE;
        const uint FilecacheFiletableFreeName = 0;

        const int SizeofFileCacheEntryT = 32;

        public BinaryReader StreamFile;
        public BinaryWriter StreamFile2;
        public UInt64 DataOffset;
        public UInt32 ExtraVersion;
        // file table
        public FileCacheEntry[] FileTableEntries;
        public UInt32 FileTableEntryCount;
        // file table (as stored in file)
        public UInt64 FileTableOffset;
        public UInt32 FileTableSize;
        // threadsafe
        public UInt32 Cs = 0;

        public static FileCache fileCache_create(string path, UInt32 extraVersion)
        {
            FileCache fileCache = new FileCache();
            BinaryWriter streamFile = new BinaryWriter(File.Open(path, FileMode.Create));
            if (streamFile == null)
            {
                //forceLog_printf("Failed to create cache file \"%ls\"", path);
                return null;
            }
            // init file cache
            //memset(fileCache, 0x00, sizeof(fileCache_t));
            //InitializeCriticalSection(&fileCache.cs);
            fileCache.StreamFile2 = streamFile;
            fileCache.DataOffset = FilecacheHeaderResv;
            fileCache.FileTableEntryCount = 32;
            fileCache.FileTableOffset = 0;
            fileCache.FileTableSize = SizeofFileCacheEntryT * fileCache.FileTableEntryCount;
            fileCache.FileTableEntries = new FileCacheEntry[fileCache.FileTableEntryCount];
            for (Int32 f = 0; f < (Int32)fileCache.FileTableEntryCount; f++)
            {
                fileCache.FileTableEntries[f] = new FileCacheEntry();
            }
            //fileCache.fileTableEntries = new FileCacheEntry[fileCache.fileTableEntries];
            //memset(fileCache->fileTableEntries, 0, fileCache->fileTableSize);
            fileCache.ExtraVersion = extraVersion;
            // file table stores info about itself
            fileCache.FileTableEntries[0].Name1 = FilecacheFiletableName1;
            fileCache.FileTableEntries[0].Name2 = FilecacheFiletableName2;
            fileCache.FileTableEntries[0].FileOffset = fileCache.FileTableOffset;
            fileCache.FileTableEntries[0].FileSize = fileCache.FileTableSize;
            // write header
            stream_writeU32(streamFile, FilecacheMagicV2);
            stream_writeU32(fileCache.StreamFile2, fileCache.ExtraVersion);
            stream_writeU64(fileCache.StreamFile2, fileCache.DataOffset);
            stream_writeU64(fileCache.StreamFile2, fileCache.FileTableOffset);
            stream_writeU32(fileCache.StreamFile2, fileCache.FileTableSize);
            // write file table
            stream_setSeek64(fileCache.StreamFile2, fileCache.DataOffset + fileCache.FileTableOffset);
            stream_writeData(fileCache.StreamFile2, fileCache.FileTableEntries, fileCache.FileTableEntryCount);

            // done
            return fileCache;
        }

        public static FileCache fileCache_openExisting(string path, UInt32 extraVersion)
        {
            FileCache fileCache = new FileCache();

            BinaryReader streamFile = new BinaryReader(File.Open(path, FileMode.Open));
            UInt32 headerMagic = stream_readU32(streamFile);
            bool isV1 = false;
            if (headerMagic != FilecacheMagic && headerMagic != FilecacheMagicV2)
            {
                stream_destroy(streamFile);
                return null;
            }
            if (headerMagic == FilecacheMagic)
            {
                isV1 = true;
            }

            UInt32 headerExtraVersion = stream_readU32(streamFile);
            if (headerExtraVersion != extraVersion)
            {
                stream_destroy(streamFile);
                return null;
            }

            UInt64 headerDataOffset = 0;
            if (isV1)
            {
                headerDataOffset = stream_readU32(streamFile);
            }
            else
            {
                headerDataOffset = stream_readU64(streamFile);
            }
            UInt64 headerFileTableOffset;
            if (isV1)
            {
                headerFileTableOffset = stream_readU32(streamFile);
            }
            else
            {
                headerFileTableOffset = stream_readU64(streamFile);
            }

            UInt32 headerFileTableSize = stream_readU32(streamFile);
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
                fileTableEntryCount = headerFileTableSize / SizeofFileCacheEntryT;
                invalidFileTableSize = (headerFileTableSize % SizeofFileCacheEntryT) != 0;
            }

            if (invalidFileTableSize)
            {
                Console.WriteLine("\"{0}\" is corrupted", path);
                stream_destroy(streamFile);
                return null;
            }

            InitializeCriticalSection(fileCache.Cs);
            fileCache.StreamFile = streamFile;
            fileCache.ExtraVersion = extraVersion;
            fileCache.DataOffset = headerDataOffset;
            fileCache.FileTableEntryCount = fileTableEntryCount;
            fileCache.FileTableOffset = headerFileTableOffset;
            fileCache.FileTableSize = fileTableEntryCount * SizeofFileCacheEntryT;
            fileCache.FileTableEntries = new FileCacheEntry[fileTableEntryCount];

            stream_setSeek64(streamFile, fileCache.DataOffset + fileCache.FileTableOffset);

            if (isV1)
            {
                // read file table entries in old format
                for (UInt32 i = 0; i < fileTableEntryCount; i++)
                {
                    UInt64 name1 = stream_readU64(streamFile);
                    UInt64 name2 = stream_readU64(streamFile);
                    UInt32 fileOffset = stream_readU32(streamFile);
                    UInt32 fileSize = stream_readU32(streamFile);
                    fileCache.FileTableEntries[i].Name1 = name1;
                    fileCache.FileTableEntries[i].Name2 = name2;
                    fileCache.FileTableEntries[i].FileOffset = fileOffset;
                    fileCache.FileTableEntries[i].FileSize = fileSize;
                    fileCache.FileTableEntries[i].ExtraReserved = 0;
                }
            }
            else
            {
                stream_readData(streamFile, fileCache.FileTableEntries, fileTableEntryCount);
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
            fileCache.StreamFile.Dispose();// stream_destroy(fileCache->stream_file);
            fileCache.StreamFile2.Dispose();
            //free(fileCache);
        }

        private static void fileCache_updateFiletable(FileCache fileCache, Int32 extraEntriesToAllocate)
        {
            // recreate file table with bigger size (optional)
            fileCache.FileTableEntries[0].Name1 = FilecacheFiletableFreeName;
            fileCache.FileTableEntries[0].Name2 = FilecacheFiletableFreeName;
            Int32 newFileTableEntryCount = (Int32)fileCache.FileTableEntryCount + extraEntriesToAllocate;
            //fileCache.fileTableEntries = (fileCacheEntry_t*)realloc(fileCache.fileTableEntries, sizeof_fileCacheEntry_t * newFileTableEntryCount);
            //fileCache.fileTableEntries = new FileCacheEntry[fileCache.fileTableSize];
            Array.Resize<FileCacheEntry>(ref fileCache.FileTableEntries, newFileTableEntryCount);
            for (Int32 f = (Int32)fileCache.FileTableEntryCount; f < newFileTableEntryCount; f++)
            {
                fileCache.FileTableEntries[f] = new FileCacheEntry();
                fileCache.FileTableEntries[f].Name1 = FilecacheFiletableFreeName;
                fileCache.FileTableEntries[f].Name2 = FilecacheFiletableFreeName;
                fileCache.FileTableEntries[f].FileOffset = 0;
                fileCache.FileTableEntries[f].FileSize = 0;
            }
            fileCache.FileTableEntryCount = (UInt32)newFileTableEntryCount;

            byte[] fileTable = AsByteArray(fileCache.FileTableEntries);
            fileCache_addFile(fileCache, FilecacheFiletableName1, FilecacheFiletableName2, fileTable, (int)(SizeofFileCacheEntryT * newFileTableEntryCount));

            // update file table info in struct
            if (fileCache.FileTableEntries[0].Name1 != FilecacheFiletableName1 || fileCache.FileTableEntries[0].Name2 != FilecacheFiletableName2)
            {
                __debugbreak();
            }
            fileCache.FileTableOffset = fileCache.FileTableEntries[0].FileOffset;
            fileCache.FileTableSize = fileCache.FileTableEntries[0].FileSize;
            // update header
            stream_setSeek64(fileCache.StreamFile2, 0);
            stream_writeU32(fileCache.StreamFile2, FilecacheMagicV2);
            stream_writeU32(fileCache.StreamFile2, fileCache.ExtraVersion);
            stream_writeU64(fileCache.StreamFile2, fileCache.DataOffset);
            stream_writeU64(fileCache.StreamFile2, fileCache.FileTableOffset);
            stream_writeU32(fileCache.StreamFile2, fileCache.FileTableSize);
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
            EnterCriticalSection(fileCache.Cs);
            // find free entry in file table
            Int32 entryIndex = -1;
            // scan for already existing entry
            for (Int32 i = 0; i < fileCache.FileTableEntryCount; i++)
            {
                if (fileCache.FileTableEntries[i].Name1 == name1 && fileCache.FileTableEntries[i].Name2 == name2)
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
                    for (Int32 i = 0; i < fileCache.FileTableEntryCount; i++)
                    {
                        if (fileCache.FileTableEntries[i].Name1 == FilecacheFiletableFreeName && fileCache.FileTableEntries[i].Name2 == FilecacheFiletableFreeName)
                        {
                            entryIndex = i;
                            break;
                        }
                    }
                    if (entryIndex == -1)
                    {
                        if (name1 == FilecacheFiletableName1 && name2 == FilecacheFiletableName2)
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
                int entryLast = (int)fileCache.FileTableEntryCount;
                while (entry < entryLast)
                {
                    if (fileCache.FileTableEntries[entry].Name1 == FilecacheFiletableFreeName && fileCache.FileTableEntries[entry].Name2 == FilecacheFiletableFreeName)
                    {
                        entry++;
                        continue;
                    }
                    if (currentEndOffset >= fileCache.FileTableEntries[entry].FileOffset && currentStartOffset < fileCache.FileTableEntries[entry].FileOffset + fileCache.FileTableEntries[entry].FileSize)
                    {
                        currentStartOffset = fileCache.FileTableEntries[entry].FileOffset + (UInt64)fileCache.FileTableEntries[entry].FileSize;
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
                        if (fileCache.FileTableEntries[entry].Name1 == FilecacheFiletableFreeName && fileCache.FileTableEntries[entry].Name2 == FilecacheFiletableFreeName)
                        {
                            entry++;
                            continue;
                        }
                        if ((UInt64)fileCache.FileTableEntries[entry].FileOffset == currentStartOffset)
                        {
                            currentStartOffset = (UInt64)fileCache.FileTableEntries[entry].FileOffset + fileCache.FileTableEntries[entry].FileSize;
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
            fileCache.FileTableEntries[entryIndex].Name1 = name1;
            fileCache.FileTableEntries[entryIndex].Name2 = name2;
            fileCache.FileTableEntries[entryIndex].ExtraReserved = extraReserved;
            fileCache.FileTableEntries[entryIndex].FileOffset = (UInt64)currentStartOffset;
            fileCache.FileTableEntries[entryIndex].FileSize = (UInt32)fileSize;
            // write file data
            stream_setSeek64(fileCache.StreamFile2, fileCache.DataOffset + (UInt64)currentStartOffset);
            stream_writeData(fileCache.StreamFile2, fileData, fileSize);
            // write file table entry
            stream_setSeek64(fileCache.StreamFile2, fileCache.DataOffset + fileCache.FileTableOffset + (UInt64)(SizeofFileCacheEntryT * entryIndex));

            stream_writeData(fileCache.StreamFile2, fileCache.FileTableEntries, fileCache.FileTableEntryCount);
            LeaveCriticalSection(fileCache.Cs);
        }

        public static bool fileCache_deleteFile(FileCache fileCache, UInt64 name1, UInt64 name2)
        {
            if (fileCache == null)
            {
                return false;
            }
            if (name1 == FilecacheFiletableName1 && name2 == FilecacheFiletableName2)
            {
                return false; // make sure the filetable is not accidentally deleted
            }
            EnterCriticalSection(fileCache.Cs);
            //fileCacheEntry_t* entry = fileCache->fileTableEntries;
            //fileCacheEntry_t* entryLast = fileCache->fileTableEntries + fileCache->fileTableEntryCount;
            int entry = 0;
            uint entryLast = fileCache.FileTableEntryCount;
            while (entry < entryLast)
            {
                if (fileCache.FileTableEntries[entry].Name1 == name1 && fileCache.FileTableEntries[entry].Name2 == name2)
                {
                    fileCache.FileTableEntries[entry].Name1 = FilecacheFiletableFreeName;
                    fileCache.FileTableEntries[entry].Name2 = FilecacheFiletableFreeName;
                    fileCache.FileTableEntries[entry].FileOffset = 0;
                    fileCache.FileTableEntries[entry].FileSize = 0;
                    // store updated entry to file cache
                    Int32 entryIndex = entry; ;
                    stream_setSeek64(fileCache.StreamFile, fileCache.DataOffset + fileCache.FileTableOffset + (UInt64)(SizeofFileCacheEntryT * entryIndex));
                    //TODOstream_writeData(fileCache.stream_file, fileCache.fileTableEntries[entryIndex], sizeof_fileCacheEntry_t);
                    LeaveCriticalSection(fileCache.Cs);
                    return true;
                }
                entry++;
            }
            LeaveCriticalSection(fileCache.Cs);
            return false;
        }

        public static byte[] fileCache_getFile(FileCache fileCache, UInt64 name1, UInt64 name2, ref Int32 fileSize)
        {
            if (fileCache == null)
            {
                return null;
            }
            EnterCriticalSection(fileCache.Cs);
            //fileCacheEntry_t* entry = fileCache.fileTableEntries;
            //fileCacheEntry_t* entryLast = fileCache.fileTableEntries + fileCache.fileTableEntryCount;
            int entry = 0;
            uint entryLast = fileCache.FileTableEntryCount;
            while (entry < entryLast)
            {
                if (fileCache.FileTableEntries[entry].Name1 == name1 && fileCache.FileTableEntries[entry].Name2 == name2)
                {
                    fileSize = (Int32)fileCache.FileTableEntries[entry].FileSize;
                    //uint8* fileData = (uint8*)malloc(fileCache.fileTableEntries[entry].fileSize);
                    byte[] fileData = new byte[fileCache.FileTableEntries[entry].FileSize];
                    stream_setSeek64(fileCache.StreamFile, fileCache.DataOffset + fileCache.FileTableEntries[entry].FileOffset);
                    stream_readData(fileCache.StreamFile, fileData, fileCache.FileTableEntries[entry].FileSize);
                    LeaveCriticalSection(fileCache.Cs);
                    return fileData;
                }
                entry++;
            }
            LeaveCriticalSection(fileCache.Cs);
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
            return (Int32)fileCache.FileTableEntryCount;
        }

        public static Int32 fileCache_countFileEntries(FileCache fileCache)
        {
            if (fileCache == null)
            {
                return 0;
            }
            EnterCriticalSection(fileCache.Cs);
            Int32 fileCount = 0;
            //fileCacheEntry_t* entry = fileCache->fileTableEntries;
            //fileCacheEntry_t* entryLast = fileCache->fileTableEntries + fileCache->fileTableEntryCount;
            int entry = 0;
            int entryLast = (int)fileCache.FileTableEntryCount;
            while (entry < entryLast)
            {
                if (fileCache.FileTableEntries[entry].Name1 == FilecacheFiletableFreeName && fileCache.FileTableEntries[entry].Name2 == FilecacheFiletableFreeName)
                {
                    entry++;
                    continue;
                }
                if (fileCache.FileTableEntries[entry].Name1 == FilecacheFiletableName1 && fileCache.FileTableEntries[entry].Name2 == FilecacheFiletableName2)
                {
                    entry++;
                    continue;
                }
                fileCount++;
                entry++;
            }
            LeaveCriticalSection(fileCache.Cs);
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
        private static void stream_writeData(BinaryWriter streamFile2, FileCacheEntry[] fileCacheEntry, uint p)
        {
            for (UInt32 i = 0; i < p; i++)
            {
                stream_writeU64(streamFile2, fileCacheEntry[i].Name1);
                stream_writeU64(streamFile2, fileCacheEntry[i].Name2);
                stream_writeU64(streamFile2, fileCacheEntry[i].FileOffset);
                stream_writeU32(streamFile2, fileCacheEntry[i].FileSize);
                stream_writeU32(streamFile2, fileCacheEntry[i].ExtraReserved);
            }
        }

        private static void stream_setSeek64(BinaryWriter streamFile2, ulong p)
        {
            streamFile2.BaseStream.Position = (long)p;
        }

        private static void stream_writeU64(BinaryWriter binaryWriter, ulong p)
        {
            binaryWriter.Write(p);
        }

        private static void stream_writeU32(BinaryWriter streamFile, uint filecacheMagicV2)
        {
            streamFile.Write(filecacheMagicV2);
        }



        private static void stream_readData(BinaryReader streamFile, FileCacheEntry[] entries, uint p)
        {
            for (UInt32 i = 0; i < p; i++)
            {
                UInt64 name1 = stream_readU64(streamFile);
                UInt64 name2 = stream_readU64(streamFile);
                UInt64 fileOffset = stream_readU64(streamFile);
                UInt32 fileSize = stream_readU32(streamFile);
                UInt32 extraReserved = stream_readU32(streamFile);
                entries[i] = new FileCacheEntry();
                entries[i].Name1 = name1;
                entries[i].Name2 = name2;
                entries[i].FileOffset = fileOffset;
                entries[i].FileSize = fileSize;
                entries[i].ExtraReserved = extraReserved;
            }
        }


        private static void stream_setSeek64(BinaryReader streamFile, ulong p)
        {
            streamFile.BaseStream.Position = (long)p;
        }

        private static void InitializeCriticalSection(uint p)
        {
        }

        private static void stream_destroy(BinaryReader streamFile)
        {
        }

        private static uint stream_readU32(BinaryReader streamFile)
        {
            return streamFile.ReadUInt32();
        }

        private static UInt64 stream_readU64(BinaryReader streamFile)
        {
            return streamFile.ReadUInt64();
        }
    }
}
