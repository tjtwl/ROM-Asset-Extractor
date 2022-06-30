using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using RomAssetExtractor.Pokemon.Entities;

// Source: Nintenlord.GBA @ https://www.romhacking.net/community/975/
namespace RomAssetExtractor.GbaSystem
{
    public unsafe static class GbaGraphics
    {
        public static Bitmap ToBitmap(byte[] pixels, Color[] colorPalette, int width = 64, int height = 64, int bitsPerPixel = 8)
        {
            var length = Math.Min(bitsPerPixel * width * height / 8, pixels.Length);

            // Making first color have no alpha for transparent background:
            colorPalette[0] = Color.FromArgb(0, 0, 0, 0);

            return GbaGraphics.ToBitmap(pixels, length, 0, colorPalette, width);
        }

        // Creates a bitmap from an byte array containing pixels (each pixel takes 8 bits in size)
        static public Bitmap ToBitmap(byte[] GBAGraphics, int length, int index, Color[] palette, int width)
        {
            fixed (byte* pointer = &GBAGraphics[index])
            {
                return FromBitmapIndexed(pointer, length, palette, width);
            }
        }

        static private Bitmap FromBitmapIndexed(byte* GBAGraphics, int length, Color[] palette, int width)
        {
            int heigth = length / width;
            while (heigth * width < length)
                heigth++;

            Bitmap result = new Bitmap(width, heigth, PixelFormat.Format8bppIndexed);
            result.Palette = BufferFillPalette(palette, result.Palette);

            BitmapData bData = result.LockBits(new Rectangle(new Point(), result.Size), ImageLockMode.WriteOnly, result.PixelFormat);

            byte* bitmap = (byte*)bData.Scan0;
            for (int i = 0; i < length; i++)
                bitmap[i] = GBAGraphics[i];

            result.UnlockBits(bData);
            return result;

        }

        static private ColorPalette BufferFillPalette(Color[] palette, ColorPalette original)
        {
            if (palette == null)
                return original;

            for (int i = 0; i < original.Entries.Length; i++)
            {
                if(i < palette.Length)
                    original.Entries[i] = palette[i];
                else
                    original.Entries[i] = Color.FromArgb(0, 0, 0);
            }

            return original;
        }
    }
}
