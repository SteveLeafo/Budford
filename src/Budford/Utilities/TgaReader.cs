using System;
using System.Drawing;
using System.IO;

/*
Decoder for Targa (.TGA) images.
Supports pretty much the full Targa specification (all bit
depths, etc).  At the very least, it decodes all TGA images that
I've found in the wild.  If you find one that it fails to decode,
let me know!
Copyright 2013-2016 Dmitry Brant
http://dmitrybrant.com
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
   http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

namespace Budford.Utilities
{
    /// <summary>
    /// Handles reading Targa (.TGA) images.
    /// </summary>
    public static class TgaReader
    {
        /// <summary>
        /// Reads a Targa (.TGA) image from a file.
        /// </summary>
        /// <param name="fileName">Name of the file to read.</param>
        /// <returns>Bitmap that contains the image that was read.</returns>
        public static Bitmap Load(string fileName)
        {
            Bitmap bmp;
            using (var f = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bmp = Load(f);
            }
            return bmp;
        }

        /// <summary>
        /// Reads a Targa (.TGA) image from a stream.
        /// </summary>
        /// <param name="stream">Stream from which to read the image.</param>
        /// <returns>Bitmap that contains the image that was read.</returns>
        /// 
        public static Bitmap Load(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            UInt32[] palette = null;

            byte idFieldLength, colorMap, imageType, bitsPerColorMap, bitsPerPixel, imgFlags;
            ushort colorMapOffset, colorsUsed, imgWidth, imgHeight;

            idFieldLength = (byte)stream.ReadByte();
            colorMap = (byte)stream.ReadByte();
            imageType = (byte)stream.ReadByte();
            colorMapOffset = LittleEndian(reader.ReadUInt16());
            colorsUsed = LittleEndian(reader.ReadUInt16());
            bitsPerColorMap = (byte)stream.ReadByte();
            LittleEndian(reader.ReadUInt16());
            LittleEndian(reader.ReadUInt16());
            imgWidth = LittleEndian(reader.ReadUInt16());
            imgHeight = LittleEndian(reader.ReadUInt16());
            bitsPerPixel = (byte)stream.ReadByte();
            imgFlags = (byte)stream.ReadByte();

            if (colorMap > 1)
            {
                throw new InvalidDataException("This is not a valid TGA file.");
            }

            if (idFieldLength > 0)
            {
                byte[] idBytes = new byte[idFieldLength];
                stream.Read(idBytes, 0, idFieldLength);
            }

            CheckImageType(colorMap, imageType, bitsPerColorMap, bitsPerPixel);

            byte[] bmpData = new byte[imgWidth * 4 * imgHeight];

            try
            {
                palette = ReadColorMap(stream, palette, colorMap, bitsPerColorMap, colorMapOffset, colorsUsed);

                byte[] scanline;
                if (imageType == 1 || imageType == 2 || imageType == 3)
                {
                    scanline = ReadImageType1To3(stream, palette, imageType, bitsPerPixel, imgWidth, imgHeight, bmpData);

                }
            }
            catch (Exception)
            {
                //give a partial image in case of unexpected end-of-file
            }

            return CreateBitmap(imgFlags, imgWidth, imgHeight, bmpData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colorMap"></param>
        /// <param name="imageType"></param>
        /// <param name="bitsPerColorMap"></param>
        /// <param name="bitsPerPixel"></param>
        private static void CheckImageType(byte colorMap, byte imageType, byte bitsPerColorMap, byte bitsPerPixel)
        {

            //image types:
            //0 - No Image Data Available
            //1 - Uncompressed Color Image
            //2 - Uncompressed RGB Image
            //3 - Uncompressed Black & White Image
            //9 - Compressed Color Image
            //10 - Compressed RGB Image
            //11 - Compressed Black & White Image

            if ((imageType > 11) || ((imageType > 3) && (imageType < 9)))
            {
                throw new InvalidDataException("This image type (" + imageType + ") is not supported.");
            }
            else if (bitsPerPixel != 8 && bitsPerPixel != 15 && bitsPerPixel != 16 && bitsPerPixel != 24 && bitsPerPixel != 32)
            {
                throw new InvalidDataException("Number of bits per pixel (" + bitsPerPixel + ") is not supported.");
            }
            if (colorMap > 0)
            {
                if (bitsPerColorMap != 15 && bitsPerColorMap != 16 && bitsPerColorMap != 24 && bitsPerColorMap != 32)
                {
                    throw new InvalidDataException("Number of bits per color map (" + bitsPerPixel + ") is not supported.");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imgFlags"></param>
        /// <param name="imgWidth"></param>
        /// <param name="imgHeight"></param>
        /// <param name="bmpData"></param>
        /// <returns></returns>
        private static Bitmap CreateBitmap(byte imgFlags, ushort imgWidth, ushort imgHeight, byte[] bmpData)
        {
            var theBitmap = new Bitmap(imgWidth, imgHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Imaging.BitmapData bmpBits = theBitmap.LockBits(new Rectangle(0, 0, theBitmap.Width, theBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Runtime.InteropServices.Marshal.Copy(bmpData, 0, bmpBits.Scan0, imgWidth * 4 * imgHeight);
            theBitmap.UnlockBits(bmpBits);

            int imgOrientation = (imgFlags >> 4) & 0x3;
            if (imgOrientation == 1)
                theBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            else if (imgOrientation == 2)
                theBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            else if (imgOrientation == 3)
                theBitmap.RotateFlip(RotateFlipType.RotateNoneFlipXY);
            return theBitmap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="palette"></param>
        /// <param name="imageType"></param>
        /// <param name="bitsPerPixel"></param>
        /// <param name="imgWidth"></param>
        /// <param name="imgHeight"></param>
        /// <param name="bmpData"></param>
        /// <returns></returns>
        private static byte[] ReadImageType1To3(Stream stream, uint[] palette, byte imageType, byte bitsPerPixel, ushort imgWidth, ushort imgHeight, byte[] bmpData)
        {
            byte[] scanline = new byte[imgWidth * (bitsPerPixel / 8)];
            for (int y = imgHeight - 1; y >= 0; y--)
            {
                switch (bitsPerPixel)
                {
                    case 8:
                        ReadCase8(stream, palette, imageType, imgWidth, bmpData, scanline, y);
                        break;
                    case 15:
                    case 16:
                        for (int x = 0; x < imgWidth; x++)
                        {
                            var hi = stream.ReadByte();
                            var lo = stream.ReadByte();

                            bmpData[4 * (y * imgWidth + x)] = (byte)((hi & 0x1F) << 3);
                            bmpData[4 * (y * imgWidth + x) + 1] = (byte)((((lo & 0x3) << 3) + ((hi & 0xE0) >> 5)) << 3);
                            bmpData[4 * (y * imgWidth + x) + 2] = (byte)(((lo & 0x7F) >> 2) << 3);
                            bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
                        }
                        break;
                    case 24:
                        ReadScanLine(stream, imgWidth, bmpData, scanline, y, 3);
                        break;
                    case 32:
                        ReadScanLine(stream, imgWidth, bmpData, scanline, y, 4);
                        break;
                }
            }

            return scanline;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="imgWidth"></param>
        /// <param name="bmpData"></param>
        /// <param name="scanline"></param>
        /// <param name="y"></param>
        /// <param name="mul"></param>
        private static void ReadScanLine(Stream stream, ushort imgWidth, byte[] bmpData, byte[] scanline, int y, int mul)
        {
            stream.Read(scanline, 0, scanline.Length);
            for (int x = 0; x < imgWidth; x++)
            {
                bmpData[4 * (y * imgWidth + x)] = scanline[x * mul];
                bmpData[4 * (y * imgWidth + x) + 1] = scanline[x * mul + 1];
                bmpData[4 * (y * imgWidth + x) + 2] = scanline[x * mul + 2];
                bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="palette"></param>
        /// <param name="imageType"></param>
        /// <param name="imgWidth"></param>
        /// <param name="bmpData"></param>
        /// <param name="scanline"></param>
        /// <param name="y"></param>
        private static void ReadCase8(Stream stream, uint[] palette, byte imageType, ushort imgWidth, byte[] bmpData, byte[] scanline, int y)
        {
            stream.Read(scanline, 0, scanline.Length);
            if (imageType == 1)
            {
                ReadImageType1(palette, imgWidth, bmpData, scanline, y);
            }
            else if (imageType == 3)
            {
                ReadImageType3(imgWidth, bmpData, scanline, y);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imgWidth"></param>
        /// <param name="bmpData"></param>
        /// <param name="scanline"></param>
        /// <param name="y"></param>
        private static void ReadImageType3(ushort imgWidth, byte[] bmpData, byte[] scanline, int y)
        {
            for (int x = 0; x < imgWidth; x++)
            {
                bmpData[4 * (y * imgWidth + x)] = scanline[x];
                bmpData[4 * (y * imgWidth + x) + 1] = scanline[x];
                bmpData[4 * (y * imgWidth + x) + 2] = scanline[x];
                bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="palette"></param>
        /// <param name="imgWidth"></param>
        /// <param name="bmpData"></param>
        /// <param name="scanline"></param>
        /// <param name="y"></param>
        private static void ReadImageType1(uint[] palette, ushort imgWidth, byte[] bmpData, byte[] scanline, int y)
        {
            for (int x = 0; x < imgWidth; x++)
            {
                if (palette != null)
                {
                    bmpData[4 * (y * imgWidth + x)] = (byte)((palette[scanline[x]] >> 16) & 0XFF);
                    bmpData[4 * (y * imgWidth + x) + 1] = (byte)((palette[scanline[x]] >> 8) & 0XFF);
                    bmpData[4 * (y * imgWidth + x) + 2] = (byte)((palette[scanline[x]]) & 0XFF);
                }
                bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="palette"></param>
        /// <param name="colorMap"></param>
        /// <param name="bitsPerColorMap"></param>
        /// <param name="colorMapOffset"></param>
        /// <param name="colorsUsed"></param>
        /// <returns></returns>
        private static uint[] ReadColorMap(Stream stream, uint[] palette, byte colorMap, byte bitsPerColorMap, ushort colorMapOffset, ushort colorsUsed)
        {
            if (colorMap > 0)
            {
                int paletteEntries = colorMapOffset + colorsUsed;
                palette = new UInt32[paletteEntries];

                if (bitsPerColorMap == 24)
                {
                    for (int i = colorMapOffset; i < paletteEntries; i++)
                    {
                        palette[i] = 0xFF000000;
                        palette[i] |= (UInt32)(stream.ReadByte() << 16);
                        palette[i] |= (UInt32)(stream.ReadByte() << 8);
                        palette[i] |= (UInt32)(stream.ReadByte());
                    }
                }
                else if (bitsPerColorMap == 32)
                {
                    for (int i = colorMapOffset; i < paletteEntries; i++)
                    {
                        palette[i] = 0xFF000000;
                        palette[i] |= (UInt32)(stream.ReadByte() << 16);
                        palette[i] |= (UInt32)(stream.ReadByte() << 8);
                        palette[i] |= (UInt32)(stream.ReadByte());
                        palette[i] |= (UInt32)(stream.ReadByte() << 24);
                    }
                }
                else if ((bitsPerColorMap == 15) || (bitsPerColorMap == 16))
                {
                    for (int i = colorMapOffset; i < paletteEntries; i++)
                    {
                        var hi = stream.ReadByte();
                        var lo = stream.ReadByte();
                        palette[i] = 0xFF000000;
                        palette[i] |= (UInt32)((hi & 0x1F) << 3) << 16;
                        palette[i] |= (UInt32)((((lo & 0x3) << 3) + ((hi & 0xE0) >> 5)) << 3) << 8;
                        palette[i] |= (UInt32)(((lo & 0x7F) >> 2) << 3);
                    }
                }
            }

            return palette;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static UInt16 LittleEndian(UInt16 val)
        {
            if (BitConverter.IsLittleEndian) return val;
            return conv_endian(val);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static UInt16 conv_endian(UInt16 val)
        {
            var temp = (UInt16)(val << 8); temp &= 0xFF00; temp |= (UInt16)((val >> 8) & 0xFF);
            return temp;
        }
    }
}