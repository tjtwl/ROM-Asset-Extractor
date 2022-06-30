using System.Runtime.InteropServices;

namespace RomAssetExtractor.Pokemon.Entities
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BattleAnimSpriteTemplateReference
    {
        // Source: battle_anim_effects in pokefirered
        public ushort TileIndex;
        public ushort PaletteTileIndex;
        public int OamPointer; // Different OamData pointers indicate different sizes (see gOamData_AffineOff_ObjNormal_8x16 in pokefirered)
        public int AnimsPointer;
        public int ImagesPointer;
        public int AffineAnimsPointer;
        public int CallbackPointer;
    }
}
