using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Budford.Utilities
{
    /// <summary>
    /// Provides helper methods for imaging
    /// </summary>
    public class IconHelper
    {
        List<Bitmap> images = new List<Bitmap>();

        /// <summary>
        /// Converts a PNG image to an icon (ico)
        /// </summary>
        /// <param name="input">The input stream</param>
        /// <param name="size">Needs to be a factor of 2 (16x16 px by default)</param>
        /// <param name="preserveAspectRatio">Preserve the aspect ratio</param>
        /// <returns>Wether or not the icon was succesfully generated</returns>
        public bool AddImage(Bitmap inputBitmap, int size, bool preserveAspectRatio = false)
        {
            if (inputBitmap == null)
            {
                return false;
            }

            float width = size, height = size;
            if (preserveAspectRatio)
            {
                if (inputBitmap.Width > inputBitmap.Height)
                {
                    height = ((float)inputBitmap.Height / inputBitmap.Width) * size;
                }
                else
                {
                    width = ((float)inputBitmap.Width / inputBitmap.Height) * size;
                }
            }

            var newBitmap = new Bitmap(inputBitmap, new Size((int)width, (int)height));
            if (newBitmap == null)
            {
                return false;
            }
            images.Add(newBitmap);
            return true;
        }


        /// <summary>
        /// Converts a PNG image to an icon (ico)
        /// </summary>
        /// <param name="input">The input stream</param>
        /// <param name="output">The output stream</param>
        /// <param name="size">Needs to be a factor of 2 (16x16 px by default)</param>
        /// <param name="preserveAspectRatio">Preserve the aspect ratio</param>
        /// <returns>Wether or not the icon was succesfully generated</returns>
        public bool ConvertToIcon(Stream output)
        {
            // save the resized png into a memory stream for future use
            using (MemoryStream memoryStream = new MemoryStream())
            {

                var iconWriter = new BinaryWriter(output);
                if (output == null || iconWriter == null)
                    return false;

                
                WriteIconHeader(iconWriter);

                foreach (var inputBitmap in images)
                {
                    inputBitmap.Save(memoryStream, ImageFormat.Png);

                    WriteIconImage(inputBitmap.Width, inputBitmap.Height, memoryStream, iconWriter);
                }
                iconWriter.Flush();
            }

            return true;
        }

        private static void WriteIconImage(float width, float height, MemoryStream memoryStream, BinaryWriter iconWriter)
        {
            // image entry 1
            // 0 image width
            iconWriter.Write((byte)width);
            // 1 image height
            iconWriter.Write((byte)height);

            // 2 number of colors
            iconWriter.Write((byte)0);

            // 3 reserved
            iconWriter.Write((byte)0);

            // 4-5 color planes
            iconWriter.Write((short)0);

            // 6-7 bits per pixel
            iconWriter.Write((short)32);

            // 8-11 size of image data
            iconWriter.Write((int)memoryStream.Length);

            // 12-15 offset of image data(Current position + 4 bytes for this entry
            iconWriter.Write((int)(iconWriter.BaseStream.Position + 4));

            // write image data
            // png data must contain the whole png data file
            iconWriter.Write(memoryStream.ToArray());
        }

        private static void WriteIconHeader(BinaryWriter iconWriter)
        {
            // 0-1 reserved, 0
            iconWriter.Write((byte)0);
            iconWriter.Write((byte)0);

            // 2-3 image type, 1 = icon, 2 = cursor
            iconWriter.Write((short)1);

            // 4-5 number of images
            iconWriter.Write((short)1);
        }
    }
}
