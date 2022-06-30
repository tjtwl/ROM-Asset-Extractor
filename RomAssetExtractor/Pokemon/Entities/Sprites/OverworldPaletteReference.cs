using System.Runtime.InteropServices;

namespace RomAssetExtractor.Pokemon.Entities
{
    [StructLayout(LayoutKind.Sequential)]
    public struct OverworldPaletteReference
    {
        public int Pointer;
        public byte Index;
        public byte _; // Always hex: 11 (dec: 17)
    }
}
