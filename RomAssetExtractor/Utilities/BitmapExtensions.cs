using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Utilities
{
    public static class BitmapExtensions
    {
        public static Bitmap Combine(this List<Bitmap> bitmaps)
        {
            var targetBitmap = new Bitmap(bitmaps.SumWidths(), bitmaps[0]?.Height ?? 0);
            var currentWidth = 0;

            using (var graphics = Graphics.FromImage(targetBitmap))
            {
                foreach (var bitmap in bitmaps)
                {
                    graphics.DrawImage(bitmap, currentWidth, 0);
                    currentWidth += bitmap.Width;
                }
            }

            return targetBitmap;
        }

        public static int SumWidths(this List<Bitmap> bitmaps)
        {
            var widths = 0;

            foreach (var bitmap in bitmaps)
            {
                widths += bitmap.Width;
            }

            return widths;
        }

        public static int LargestHeight(this List<Bitmap> bitmaps)
        {
            var largestHeight = int.MinValue;

            foreach (var bitmap in bitmaps)
            {
                if (bitmap.Height > largestHeight)
                    largestHeight = bitmap.Height;
            }

            return largestHeight;
        }
    }
}
