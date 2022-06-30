using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.GbaSystem
{
    public static class GbaColor
    {
        public static unsafe Color[] UnpackColors(byte[] data, bool convertFromBit16)
        {
            var dataLength = data.Length;

            //char* cdata = data;
            var cdata = Marshal.AllocHGlobal(dataLength);
            Marshal.Copy(data, 0, cdata, dataLength);

            var palette = (ushort*)cdata;
            ushort x;

            var size = (int)(data.Length / sizeof(short));
            var colors = new byte[size][];

            for (int i = 0; i < size; i++)
            {
                x = palette[i];
                var rgb = new byte[] {
                    (byte)(x & 0x1f),
                    (byte)((x >> 5) & 0x1f),
                    (byte)((x >> 10) & 0x1f)
                };
                colors[i] = rgb;
            }

            Marshal.FreeHGlobal(cdata);

            if (convertFromBit16)
                return From16BitColorsToTrueColors(colors);
            else
                return ToColors(colors);
        }

        public static Color[] ToColors(byte[][] data)
        {
            Color[] colors = new Color[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                colors[i] = Color.FromArgb(
                    (int)data[i][0],
                    (int)data[i][1],
                    (int)data[i][2]);
            }

            return colors;
        }

        public static Color[] From16BitColorsToTrueColors(byte[][] data)
        {
            Color[] colors = new Color[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                colors[i] = Color.FromArgb(
                    (int)Math.Round(data[i][0] * 255.0 / 31.0),
                    (int)Math.Round(data[i][1] * 255.0 / 31.0),
                    (int)Math.Round(data[i][2] * 255.0 / 31.0));
            }

            return colors;
        }
    }
}
