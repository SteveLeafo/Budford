using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budford.Utilities
{
    class FileCacheEntry
    {
        public UInt64 name1;
        public UInt64 name2;
        public UInt64 fileOffset;
        public UInt32 fileSize;
        public UInt32 extraReserved; // currently unused, but in the future may be used to extend fileSize or add flags (for compression)

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[32];
            BitConverter.GetBytes(name1).CopyTo(bytes, 0);
            BitConverter.GetBytes(name2).CopyTo(bytes, 8);
            BitConverter.GetBytes(fileOffset).CopyTo(bytes, 16);
            BitConverter.GetBytes(fileSize).CopyTo(bytes, 24);
            BitConverter.GetBytes(extraReserved).CopyTo(bytes, 28);

            return bytes;
        }
    }
}
