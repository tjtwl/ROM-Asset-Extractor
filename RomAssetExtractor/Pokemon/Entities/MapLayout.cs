using RomAssetExtractor.GbaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Pokemon.Entities
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MapLayout
    {
        public int Width;
        public int Height;

        public Pointer BorderTilemap;
        public Pointer TileData;
        public Pointer PrimaryTileset;
        public Pointer SecondaryTileset;

        public byte BorderWidth;
        public byte BorderHeight;
    }
}
