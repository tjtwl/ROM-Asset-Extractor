using RomAssetExtractor.Utilities;
using System.Runtime.InteropServices;

namespace RomAssetExtractor.Pokemon.Entities
{
    [StructLayout(LayoutKind.Sequential)]
    public struct OverworldSpriteReference
    {
        // Useful sources:
        // Source (1): https://www.pokecommunity.com/showthread.php?t=211535
        // Source (2): https://gbdev.gg8.se/wiki/articles/OAM_DMA_tutorial
        // Source (3): https://github.com/pret/pokefirered

        public ushort tileTag; // Always? FFFF

        public byte PaletteIndex;
        public byte _;
        public byte ReflectionPaletteIndex;
        public byte __;
        public ushort FrameSize;

        public short Width;
        public short Height;

        // Bit usage: 4,2,1,1
        private byte flags;
        [BitField(4, nameof(flags))] public int PaletteSlot => BitField.Parse(this);
        [BitField(2, nameof(flags))] public int ShadowSize => BitField.Parse(this);
        [BitField(1, nameof(flags))] public bool Inanimate => BitField.Parse(this) == 1;
        [BitField(1, nameof(flags))] public bool DisableReflectionPaletteLoad => BitField.Parse(this) == 1;

        public byte Tracks;

        public int OamPointer;
        public int SpriteSubTables;
        public int AnimationPointers;
        public int SpriteFramePointers;
        public int AffineAnimationPointers;
    }
}
