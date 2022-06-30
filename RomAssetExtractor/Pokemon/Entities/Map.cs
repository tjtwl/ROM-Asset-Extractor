using Newtonsoft.Json;
using RomAssetExtractor.GbaSystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RomAssetExtractor.Pokemon.Entities.MetatileLayout;

namespace RomAssetExtractor.Pokemon.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MapModel
    {
        [JsonProperty]
        public int BankId { get; set; }
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public string Label { get; set; }

        [JsonProperty]
        public MapHeader Header { get; set; }
        [JsonProperty]
        public MapLayout Layout { get; set; }

        [JsonProperty]
        public WildSpeciesEncounters[] Encounters { get; set; }
    }

    public class Map : MapModel
    {
        public const int NUM_ENCOUNTER_TYPES = 4;

        internal MetatileLayout Metatiles { get; private set; }

        public Map(int bankId, int mapId)
        {
            BankId = bankId;
            Id = mapId;

            Encounters = new WildSpeciesEncounters[NUM_ENCOUNTER_TYPES];
        }

        public string GetCleanLabel()
        {
            return Label.Replace("\\c", "").Replace("\n", "");
        }

        public Bitmap CreateTilesetBitmap(PokemonRomReader reader, MetatileLayer layer)
        {
            return Metatiles.RenderTilesetBitmap(reader, layer);
        }

        public Bitmap CreateBitmap(PokemonRomReader reader)
        {
            var bitmapLayers = new Bitmap[] {
                // Player layer would go here...
                Metatiles.RenderBitmap(reader, Layout.Width, Layout.Height, MetatileLayer.Bottom),
                Metatiles.RenderBitmap(reader, Layout.Width, Layout.Height, MetatileLayer.Top)
            };
            var bitmap = new Bitmap(bitmapLayers[0].Width, bitmapLayers[0].Height, bitmapLayers[0].PixelFormat);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                foreach (var layer in bitmapLayers)
                {
                    graphics.DrawImage(layer, 0, 0);
                }
            }

            return bitmap;
        }

        public override string ToString()
        {
            return $"{nameof(Map)}({BankId}.{Id} {GetCleanLabel()})";
        }

        // Expects reader to be at bank offset pointer for map to read
        public static Map ReadMap(PokemonRomReader reader, int bankId, int mapId)
        {
            var map = new Map(
                bankId: bankId,
                mapId: mapId);

            map.Header = reader.Read<MapHeader>();

            reader.GoToPointer(map.Header.MapLayout);

            map.Layout = reader.Read<MapLayout>();

            // Primary and secondary tilesets
            var sharedPalettes = Tileset.CreateSharedPalettesList();
            reader.GoToPointer(map.Layout.PrimaryTileset);
            var primaryTileset = Tileset.ReadTileset(reader, sharedPalettes);

            reader.GoToPointer(map.Layout.SecondaryTileset);
            var secondaryTileset = Tileset.ReadTileset(reader, sharedPalettes);

            // Get the metatiles (contains data on which tile uses which of the two palettes)
            map.Metatiles = MetatileLayout.ReadMetatileData(reader, map, primaryTileset, secondaryTileset);

            var mapLabelSize = reader.Constants.Code.StartsWith("BP") ? Pointer.GetSize() : 8;

            reader.GoToPointer(reader.Constants.MapLabels.OffsetBy((map.Header.LabelIndex + reader.Constants.MapNameOffset) * mapLabelSize));

            if(!reader.Constants.Code.StartsWith("BP"))
                reader.Skip(4); // Unsure what these bytes contain in Ruby

            var mapNameOffset = reader.ReadPointer();
            reader.GoToPointer(mapNameOffset);
            map.Label = reader.ReadString();

            return map;
        }

        private static Bank[] FindMapBanks(PokemonRomReader reader)
        {
            reader.GoToPointer(reader.Constants.MapBanks);
            var bankOffset = reader.ReadPointer();
            var banks = new List<Bank>();

            do
            {
                reader.GoToPointer(bankOffset.OffsetBy(reader.GetSizeOf<Pointer>(banks.Count)));

                if (!reader.PeekIsPointer())
                    break;

                banks.Add(new Bank(banks.Count, reader.ReadPointer()));
            } while (true);

            for (int i = 0; i < banks.Count; i++)
            {
                var bank = banks[i];
                var mapPointers = new List<Pointer>();

                do
                {
                    var offset = bank.Pointer.OffsetBy(reader.GetSizeOf<Pointer>(mapPointers.Count));

                    reader.GoToPointer(offset);

                    if (!reader.PeekIsPointer())
                        break;

                    if (offset >= bankOffset)
                        break;

                    if (i + 1 < banks.Count && offset >= banks[i + 1].Pointer)
                        break;

                    mapPointers.Add(reader.ReadPointer());
                } while (true);

                bank.SetMapPointers(mapPointers.ToArray());
            }

            return banks.ToArray();
        }

        public static Bank[] ReadAllMapBanks(PokemonRomReader reader, out int mapCount)
        {
            var banks = FindMapBanks(reader);
            mapCount = 0;

            foreach (var bank in banks)
            {
                for (int mapId = 0; mapId < bank.MapPointers.Length; mapId++)
                {
                    var mapOffset = bank.MapPointers[mapId];
                    reader.GoToPointer(mapOffset);
                    var map = ReadMap(reader, bank.Id, mapId);

                    bank.AddMap(map);
                    mapCount++;
                }
            }

            return banks;
        }

        public static Bank[] ReadMapBank(PokemonRomReader reader, int b, int? mapFilter = null)
        {
            var banks = FindMapBanks(reader);
            var bank = banks[b];

            for (int mapId = 0; mapId < bank.MapCount; mapId++)
            {
                if (mapFilter != null && mapFilter != mapId)
                    continue;

                reader.GoToPointer(bank.MapPointers[mapId]);
                var map = ReadMap(reader, b, mapId);
                bank.AddMap(map);
            }

            return new Bank[] { bank };
        }
    }
}
