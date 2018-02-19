using System;
using System.IO;


namespace Budford.Tools
{
    internal class HashGenerator
    {
        internal static UInt32 GenerateHashFromRpxRawData(byte[] rpxData, long size)
        {
            UInt32 h = 0x3416DCBF;
            for (Int32 i = 0; i < size; ++i)
            {
                UInt32 c = rpxData[i];
                h = (h << 3) | (h >> 29);
                h += c;
            }
            return h;
        }

        internal static UInt64 GenerateHashFromRpxRawData2(UInt64 xx, byte[] rpxData, long size)
        {
            UInt32 h2 = 0x3416DCBF;
            for (Int32 i = 0; i < size; ++i)
            {
                UInt32 c = rpxData[i];
                UInt32 a = (h2 << 3);
                UInt32 b = (h2 >> 29);
                h2 = a | b;
                h2 += c;
            }

            UInt64 h = 0x0a0770655c1b587f;
            for (Int32 i = 0; i < size; ++i)
            {
                UInt32 c = rpxData[i];
                h = (h << 3) | (h >> 61);
                h += c;
            }
            return h;
        }

        internal static byte[] FileToByteArray(string fileName)
        {
            FileStream fs = new FileStream(fileName,
                                           FileMode.Open,
                                           FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(fileName).Length;
            var buff = br.ReadBytes((int)numBytes);
            return buff;
        }

        internal static UInt32 GetHash(string fileName)
        {
            byte[] ba = FileToByteArray(fileName);
            return GenerateHashFromRpxRawData(ba, ba.Length);
        }
    }
}
