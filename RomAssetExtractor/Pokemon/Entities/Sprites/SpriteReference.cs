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
    public struct SpriteReference
    {
        public Pointer Pointer;
        public short _; // This is 00 08 for every Pokémon's front sprite, with the shiny palettes it gets interesting :/
        public short GameIndex;
    }
}
