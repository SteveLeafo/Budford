using System;

namespace Budford.Utilities
{
    class FileCacheEntry
    {
        public UInt64 Name1;
        public UInt64 Name2;
        public UInt64 FileOffset;
        public UInt32 FileSize;
        public UInt32 ExtraReserved; // currently unused, but in the future may be used to extend fileSize or add flags (for compression)

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[32];
            BitConverter.GetBytes(Name1).CopyTo(bytes, 0);
            BitConverter.GetBytes(Name2).CopyTo(bytes, 8);
            BitConverter.GetBytes(FileOffset).CopyTo(bytes, 16);
            BitConverter.GetBytes(FileSize).CopyTo(bytes, 24);
            BitConverter.GetBytes(ExtraReserved).CopyTo(bytes, 28);

            return bytes;
        }
    }
}
