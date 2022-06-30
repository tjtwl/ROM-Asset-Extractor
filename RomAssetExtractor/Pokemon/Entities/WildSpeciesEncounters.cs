using Newtonsoft.Json;
using RomAssetExtractor.GbaSystem;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RomAssetExtractor.Pokemon.Entities
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WildSpeciesReference
    {
        public byte BankId;
        public byte MapId;
        public short _;
        public Pointer GrassEncounters;
        public Pointer WaterEncounters;
        public Pointer RockSmashEncounters;
        public Pointer FishingEncounters;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct WildSpeciesEncounterReference
    {
        public byte MinLevel;
        public byte MaxLevel;
        public short PokemonGameIndex;
    }

    public enum WildEncounterType
    {
        GrassOrCave = 0,
        Water = 1,
        RockSmash = 2,
        Fishing = 3
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class WildSpeciesEncounter
    {
        [JsonProperty]
        public WildSpeciesEncounterReference Encounter { get; set; }
        [JsonProperty]
        public SpeciesModel Species { get; set; }

        public WildSpeciesEncounter(WildSpeciesEncounterReference encounterReference, SpeciesModel monster)
        {
            Encounter = encounterReference;
            Species = monster;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class WildSpeciesEncounters
    {
        private const int MAX_RATIO = 255;

        [JsonProperty]
        public WildEncounterType Type { get; private set; }
        [JsonProperty]
        public WildSpeciesEncounter[] CatchRates { get; private set; }
        [JsonProperty]
        public byte Ratio { get; private set; }
        [JsonProperty]
        public int RatioPercentage => (int) ((double)Ratio / MAX_RATIO * 100d);

        public WildSpeciesEncounters(WildEncounterType type, int encounterCount, byte ratio)
        {
            CatchRates = new WildSpeciesEncounter[encounterCount];
            Type = type;
            Ratio = ratio;
        }

        public unsafe static void FillMapsWithEncounters(PokemonRomReader reader, ref Bank[] mapBanks, SpeciesModel[] monsters)
        {
            reader.GoToPointer(reader.Constants.MapWildPokemon);
            var encountersPointer = reader.ReadPointer();
            int currentReferenceId = 0;
            do
            {
                reader.GoToPointer(encountersPointer.OffsetBy(reader.GetSizeOf<WildSpeciesReference>(currentReferenceId)));
                var reference = reader.Read<WildSpeciesReference>();
                var currentBankId = reference.BankId;
                var currentMapId = reference.MapId;

                if (currentBankId == 0xFF && currentMapId == 0xFF)
                    break;

                var map = mapBanks[currentBankId].Maps[currentMapId];

                var encounterTypes = new Dictionary<WildEncounterType, Pointer>()
                {
                    { WildEncounterType.GrassOrCave, reference.GrassEncounters},
                    { WildEncounterType.Water, reference.WaterEncounters},
                    { WildEncounterType.RockSmash, reference.RockSmashEncounters},
                    { WildEncounterType.Fishing, reference.FishingEncounters},
                };

                foreach (var encounterTypePair in encounterTypes)
                {
                    if (!encounterTypePair.Value.IsNull)
                        map.Encounters[(int)encounterTypePair.Key] = ReadMapEncounter(reader, encounterTypePair.Value, map, monsters);
                }

                if (currentReferenceId++ > 1000)
                    throw new Exception("Should have finished looping, but continued for too long! Bug?");
            } while (true);
        }

        private static unsafe WildSpeciesEncounters ReadMapEncounter(PokemonRomReader reader, Pointer encounter, Map map, SpeciesModel[] monsters)
        {
            reader.GoToPointer(encounter);
            var encounterRatio = reader.ReadByte();
            reader.Skip(3); // 00 00 00 everywhere I checked
            var encountersPointer = reader.ReadPointer();
            reader.GoToPointer(encountersPointer);

            var encounterCount = (-encountersPointer.Distance(encounter)) / sizeof(WildSpeciesEncounterReference);
            // Since pointers also point backwars towards just in front of them (at least in FireRed where I checked)
            var encounters = new WildSpeciesEncounters(WildEncounterType.GrassOrCave, encounterCount, encounterRatio);
            
            for (int i = 0; i < encounterCount; i++)
            {
                var encounterReference = reader.Read<WildSpeciesEncounterReference>();

                if (encounterReference.PokemonGameIndex >= monsters.Length || encounterReference.PokemonGameIndex < 0)
                    global::System.Diagnostics.Debugger.Break();

                encounters.CatchRates[i] = new WildSpeciesEncounter(encounterReference, monsters[encounterReference.PokemonGameIndex]);
            }

            return encounters;
        }
    }
}
