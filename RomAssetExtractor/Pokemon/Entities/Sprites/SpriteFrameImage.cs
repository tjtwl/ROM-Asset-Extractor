using System.Runtime.InteropServices;

namespace RomAssetExtractor.Pokemon.Entities
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SpriteFrameImage
    {
        public int DataPointer;
        public ushort Size;
    }
}
