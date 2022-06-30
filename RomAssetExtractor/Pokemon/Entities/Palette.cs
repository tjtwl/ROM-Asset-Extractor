using RomAssetExtractor.GbaSystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Pokemon.Entities
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Palette
    {
        public const int NUM_COLORS = 16;

        private byte color0;
        private byte color0B;
        private byte color1;
        private byte color1B;
        private byte color2;
        private byte color2B;
        private byte color3;
        private byte color3B;
        private byte color4;
        private byte color4B;
        private byte color5;
        private byte color5B;
        private byte color6;
        private byte color6B;
        private byte color7;
        private byte color7B;
        private byte color8;
        private byte color8B;
        private byte color9;
        private byte color9B;
        private byte color10;
        private byte color10B;
        private byte color11;
        private byte color11B;
        private byte color12;
        private byte color12B;
        private byte color13;
        private byte color13B;
        private byte color14;
        private byte color14B;
        private byte color15;
        private byte color15B;

        public unsafe static int GetSize()
            => sizeof(Palette);

        private byte[] GetAllColorBytes()
            => new byte[]
            {
                color0,
                color0B,
                color1,
                color1B,
                color2,
                color2B,
                color3,
                color3B,
                color4,
                color4B,
                color5,
                color5B,
                color6,
                color6B,
                color7,
                color7B,
                color8,
                color8B,
                color9,
                color9B,
                color10,
                color10B,
                color11,
                color11B,
                color12,
                color12B,
                color13,
                color13B,
                color14,
                color14B,
                color15,
                color15B,
            };

        internal static Palette FromByteArray(byte[] colors)
        {
            var palette = new Palette();
            var currentColor = 0;
            palette.color0 = colors[currentColor];
            palette.color0B = colors[currentColor];
            currentColor++;
            palette.color1 = colors[currentColor];
            palette.color1B = colors[currentColor];
            currentColor++;
            palette.color2 = colors[currentColor];
            palette.color2B = colors[currentColor];
            currentColor++;
            palette.color3 = colors[currentColor];
            palette.color3B = colors[currentColor];
            currentColor++;
            palette.color4 = colors[currentColor];
            palette.color4B = colors[currentColor];
            currentColor++;
            palette.color5 = colors[currentColor];
            palette.color5B = colors[currentColor];
            currentColor++;
            palette.color6 = colors[currentColor];
            palette.color6B = colors[currentColor];
            currentColor++;
            palette.color7 = colors[currentColor];
            palette.color7B = colors[currentColor];
            currentColor++;
            palette.color8 = colors[currentColor];
            palette.color8B = colors[currentColor];
            currentColor++;
            palette.color9 = colors[currentColor];
            palette.color9B = colors[currentColor];
            currentColor++;
            palette.color10 = colors[currentColor];
            palette.color10B = colors[currentColor];
            currentColor++;
            palette.color11 = colors[currentColor];
            palette.color11B = colors[currentColor];
            currentColor++;
            palette.color12 = colors[currentColor];
            palette.color12B = colors[currentColor];
            currentColor++;
            palette.color13 = colors[currentColor];
            palette.color13B = colors[currentColor];
            currentColor++;
            palette.color14 = colors[currentColor];
            palette.color14B = colors[currentColor];
            currentColor++;
            palette.color15 = colors[currentColor];
            palette.color15B = colors[currentColor];

            return palette;
        }

        public Color[] ToTrueColors()
        {
            var paletteColorBytes = GetAllColorBytes();
            var colors = new Color[NUM_COLORS];

            for (int i = 0; i < paletteColorBytes.Length; i+=2)
            {
                var value = paletteColorBytes[i] | (paletteColorBytes[i + 1] << 8);

                var red = (value & 0x1F) << 3;
                var green = (value & 0x3E0) >> 2;
                var blue = (value & 0x7C00) >> 7;

                colors[i / 2] = Color.FromArgb(red, green, blue);
            }

            return colors;
        }
    }
}
