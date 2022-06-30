using RomAssetExtractor.GbaSystem;
using RomAssetExtractor.Utilities;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace RomAssetExtractor.Pokemon
{
    public class PokemonRomConstants
    {
        // Game info
        public string Name{ get; set; }
        public string Language{ get; set; }
        public string Code{ get; set; }

        // Offsets
        public Pointer Moves { get; set; }
        public Pointer MoveNames { get; set; }
        public Pointer BattleAnimParticles { get; set; }
        public Pointer BattleAnimTemplatesUnknown { get; set; }
        public Pointer MapLabels { get; set; }
        public Pointer MapNames { get; set; }
        public Pointer MapBanks { get; set; }
        public Pointer MapWildPokemon { get; set; }
        public Pointer SpeciesBaseStats { get; set; }
        public Pointer SpeciesEvolutions { get; set; }
        public Pointer SpeciesNames { get; set; }
        public Pointer SpeciesIcons { get; set; }
        public Pointer SpeciesIconPaletteChoices { get; set; }
        public Pointer SpeciesIconPalettes { get; set; }        
        public Pointer SpeciesSprites { get; set; }
        public Pointer SpeciesPalettes { get; set; }
        public Pointer SpeciesShinyPalettes { get; set; }
        public Pointer SpeciesBackSprites { get; set; }
        public Pointer ExperienceSpeeds { get; set; }
        public Pointer OverworldSprites { get; set; }
        public Pointer OverworldPalettes { get; set; }
        public Pointer TrainerSprites { get; set; }
        public Pointer TrainerPalettes { get; set; } 
        public Pointer TrainerBackSprites { get; set; } 
        public Pointer TrainerBackPalettes { get; set; }

        // Constant info 
        public int MovesQuietBgmCount { get; set; } // 2 byte table index
        public int MovesCount { get; set; }
        public int SpeciesNameSize { get; set; }
        public int SpeciesMaxEvolutions { get; set; } = 5;
        public int ExperienceSpeedsCount { get; set; }
        public int MaxLevel { get; set; } = 100;
        public int MovesStatusCount { get; set; }
        public int MovesGeneralCount { get; set; }
        public int MovesSpecialCount { get; set; }
        public int BattleAnimCount { get; set; }
        public int BattleAnimTemplatesUnknownCount { get; set; }
        public int SpeciesIconPaletteCount { get; set; }
        public OrderedDictionary<string, SpeciesConstant> SpeciesConstants { get; set; }
        public int OverworldSpriteCount { get; set; }
        public int TrainerSpriteCount{ get; set; }
        public int TrainerBackSpriteCount{ get; set; }
        public OrderedDictionary<string, int> MapTileAttributes { get; set; }
        public List<int> MapTileAttributesSurfOnly { get; set; }
        public int MapTileAttributesJumpNorth { get; set; }
        public int MapTileAttributesJumpEast { get; set; }
        public int MapTileAttributesJumpSouth { get; set; }
        public int MapTileAttributesJumpWest { get; set; }

        public int MapNamesCount{ get; set; }
        public int MapNameOffset{ get; set; }

        public enum SpeciesDataType
        {
            NoNameAndNoBaseStats = 0,
            NormalName = 1,
            IsEgg = 2, 
            AlphabetName = 3,
        }

        public class SpeciesConstant
        {
            public const int NO_BASE_STAT = -1;

            public int Amount;
            public SpeciesDataType DataType = SpeciesDataType.NormalName;
            public bool Skip = false;
            public int SpeciesBaseStatsOffset = 0;
            public int AsciiOffset = 0; // Only for AlphabetName
        }
    }
}