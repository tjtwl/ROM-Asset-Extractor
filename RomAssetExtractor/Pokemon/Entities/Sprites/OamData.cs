using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using RomAssetExtractor.Utilities;

namespace RomAssetExtractor.Pokemon.Entities
{
    [StructLayout(LayoutKind.Sequential)]
    public struct OamData
    {
        private ushort flagsA;
        [BitField(8, nameof(flagsA))] public int Y => BitField.Parse(this);
        [BitField(2, nameof(flagsA))] public int AffineMode => BitField.Parse(this);
        [BitField(2, nameof(flagsA))] public int ObjMode => BitField.Parse(this);
        [BitField(1, nameof(flagsA))] public bool Mosaic => BitField.Parse(this) == 1;
        [BitField(1, nameof(flagsA))] public bool Bpp => BitField.Parse(this) == 1;
        [BitField(2, nameof(flagsA))] public OamShape Shape => (OamShape)BitField.Parse(this);

        private ushort flagsB;
        [BitField(9, nameof(flagsB))] public int X => BitField.Parse(this);
        [BitField(5, nameof(flagsB))] public int MatrixNum => BitField.Parse(this);
        [BitField(2, nameof(flagsB))] public Size Size => ShapeSizes[Shape][BitField.Parse(this)];

        private ushort flagsC;
        [BitField(10, nameof(flagsC))] public int TileNum => BitField.Parse(this);
        [BitField(2, nameof(flagsC))] public int Priority => BitField.Parse(this);
        [BitField(4, nameof(flagsC))] public int PaletteNum => BitField.Parse(this);

        public ushort AffineParam;

        public enum OamShape
        {
            ST_OAM_SQUARE = 0b00,
            ST_OAM_H_RECTANGLE = 0b01,
            ST_OAM_V_RECTANGLE = 0b10,
        }

        public static readonly Dictionary<OamShape, Size[]> ShapeSizes = new Dictionary<OamShape, Size[]>()
        {
            {
                OamShape.ST_OAM_SQUARE,
                new Size[] {
                    new Size(8, 8),
                    new Size(16, 16),
                    new Size(32, 32),
                    new Size(64, 64),
                }
            },
            {
                OamShape.ST_OAM_H_RECTANGLE,
                new Size[] {
                    new Size(16, 8),
                    new Size(32, 8),
                    new Size(32, 16),
                    new Size(64, 32),
                }
            },
            {
                OamShape.ST_OAM_V_RECTANGLE,
                new Size[] {
                    new Size(8, 16),
                    new Size(8, 32),
                    new Size(16, 32),
                    new Size(32, 64),
                }
            },
        };
    }
}
