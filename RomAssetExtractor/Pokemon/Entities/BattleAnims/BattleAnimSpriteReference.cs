using System.Runtime.InteropServices;

namespace RomAssetExtractor.Pokemon.Entities
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BattleAnimSpriteReference
    {
        // Source: https://www.pokecommunity.com/showthread.php?t=308062
        public int Pointer;
        public short Size;
        public short TileIndex;
    }
}
