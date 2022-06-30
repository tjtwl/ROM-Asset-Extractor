using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Pokemon.Entities
{

    [StructLayout(LayoutKind.Sequential)]
    public struct BattleAnimPaletteReference
    {
        public int Pointer;
        public short TileIndex;
        public short _; // 00 00
    }
}
