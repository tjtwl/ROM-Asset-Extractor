using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Pokemon.Entities
{
    // Source: pokemon.h from pokefirered
    public enum EvolutionMethod
    {
        EvoFriendship = 0x0001, // Pokémon levels up with friendship ≥ 220
        EvoFriendshipDay = 0x0002, // Pokémon levels up during the day with friendship ≥ 220
        EvoFriendshipNight = 0x0003, // Pokémon levels up at night with friendship ≥ 220
        EvoLevel = 0x0004, // Pokémon reaches the specified level
        EvoTrade = 0x0005, // Pokémon is traded
        EvoTradeItem = 0x0006, // Pokémon is traded while it's holding the specified item
        EvoItem = 0x0007, // specified item is used on Pokémon
        EvoLevelAtkGtDef = 0x0008, // Pokémon reaches the specified level with attack > defense
        EvoLevelAtkEqDef = 0x0009, // Pokémon reaches the specified level with attack = defense
        EvoLevelAtkLtDef = 0x000a, // Pokémon reaches the specified level with attack < defense
        EvoLevelSilcoon = 0x000b, // Pokémon reaches the specified level with a Silcoon personality value
        EvoLevelCascoon = 0x000c, // Pokémon reaches the specified level with a Cascoon personality value
        EvoLevelNinjask = 0x000d, // Pokémon reaches the specified level (special value for Ninjask)
        EvoLevelShedinja = 0x000e, // Pokémon reaches the specified level (special value for Shedinja)
        EvoBeauty = 0x000f, // Pokémon levels up with beauty ≥ specified value
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Evolution
    {
        private ushort method;
        private ushort levelOrItem;
        private ushort targetSpecies;
        private ushort _; // 00 00 padding

        public EvolutionMethod EvolutionMethod => (EvolutionMethod)method;
        public ushort TargetSpeciesIndex => targetSpecies;
    }
}
