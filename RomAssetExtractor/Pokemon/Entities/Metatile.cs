using Newtonsoft.Json;
using RomAssetExtractor.GbaSystem;
using RomAssetExtractor.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Pokemon.Entities
{
    #region notes
    /* 
           10000100 == 84 == sign post
             111011 == 3b == jump down
             111000 == 38 == jump right
            1000000 == 40 == overworld walks right
            1000100 == 44 == overworld slides right
            1000111 == 47 == overworld slides down
            1001000 == 48 == like ice (44 - 47) but with a small difference
            1010001 == 51 == overworld run right
            1010011 == 53 == overworld run down
         1000000010 == 202 == grass encounter
        10000010101 == 415 == water encounter
            1101001 == 69 == warp door
        Extra source: https://www.pokecommunity.com/showthread.php?t=144408
     */
    #endregion notes

    [JsonObject(MemberSerialization.OptIn)]
    public class MetatileModel
    {
        public const int SIZE_IN_PIXELS = Tile.SIZE_IN_PIXELS * 2;

        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public bool IsOverlay { get; set; }
        [JsonProperty]
        public int RawBehavior { get; set; }
        [JsonProperty]
        public int RawMapAttributes { get; set; }
        [JsonProperty]
        public OrderedDictionary<string, int> Attributes { get; set; }
        [JsonProperty]
        public int IsSignPost { get; set; }
        [JsonProperty]
        public bool IsImpassable { get; set; }
        [JsonProperty]
        public bool IsSurfOnly { get; set; }
        [JsonProperty]
        public bool IsJumpNorth { get; set; }
        [JsonProperty]
        public bool IsJumpEast { get; set; }
        [JsonProperty]
        public bool IsJumpSouth { get; set; }
        [JsonProperty]
        public bool IsJumpWest { get; set; }
        [JsonProperty]
        public MetatileEncounter EncounterType { get; set; }

    }

    internal class Metatile : MetatileModel
    {
        internal const int DATA_LENGTH = MetatileLayout.LAYER_COUNT * 8;

        private const int ENCOUNTER_MASK = 0b11000000000;
        private const int ENCOUNTER_SHIFT = 9;

        private const int ATTRIBUTE_MASK = 0b00011111111;
        
        private readonly Tile[] tiles;

        private Metatile(int index, int tilesSize)
        {
            Id = index;
            tiles = new Tile[tilesSize];
        }

        public Bitmap RenderBitmap(PokemonRomReader reader, MetatileLayer? layerId = null)
        {
            if (tiles.Length == 0)
                throw new NotImplementedException("");

            var bitmap = new Bitmap(SIZE_IN_PIXELS, SIZE_IN_PIXELS);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                var start = 0;
                var end = 8; // exclusive

                if (layerId != null)
                {
                    if (layerId == MetatileLayer.Bottom)
                    {
                        end = 4;
                    }
                    else if (layerId == MetatileLayer.Top)
                    {
                        start = 4;
                    }

                    if (IsOverlay)
                    {
                        if (layerId == MetatileLayer.Bottom)
                        {
                            end = 8;
                        }
                        else
                        {
                            end = 0;
                        }
                    }
                }

                for (var tileMetatileIndex = start; tileMetatileIndex < end; tileMetatileIndex++)
                {
                    var tile = tiles[tileMetatileIndex];

                    if (tile == null)
                        continue;

                    GetTilePositions(tileMetatileIndex, out int tileX, out int tileY);

                    var childBitmap = tile.RenderBitmap();
                    graphics.DrawImage(childBitmap, tileX, tileY);
                }
            }

            return bitmap;
        }

        private static void GetTilePositions(int tileMetatileIndex, out int tileX, out int tileY)
        {
            switch (tileMetatileIndex)
            {
                case 0:
                case 4:
                    tileX = 0;
                    tileY = 0;
                    break;
                case 1:
                case 5:
                    tileX = 8;
                    tileY = 0;
                    break;
                case 2:
                case 6:
                    tileX = 0;
                    tileY = 8;
                    break;
                case 3:
                case 7:
                    tileX = 8;
                    tileY = 8;
                    break;
                default:
                    throw new NotImplementedException("More than 2 layers are not yet supported for metatiles");
            }
        }

        internal void SetMapAttributes(int mapAttributes)
        {
            RawMapAttributes = mapAttributes;
            IsImpassable = mapAttributes == 0x01;
        }

        // This method will set offset manually, no need to direct it manually with GoToOffset
        public static Metatile ReadMetatile(PokemonRomReader reader, int index, Tileset primaryTileset, Tileset secondaryTileset, out bool wasCompletelyEmpty)
        {
            var originalIndex = index;
            var tileset = primaryTileset;

            if (index >= primaryTileset.MetatileCount)
            {
                index -= primaryTileset.MetatileCount;
                tileset = secondaryTileset;
            }

            var dataLength = reader.GetSizeOf<short>();
            var metatile = new Metatile(originalIndex, DATA_LENGTH / dataLength);
            var attributeDataLength = reader.Constants.Code.StartsWith("BP") ? reader.GetSizeOf<int>() : reader.GetSizeOf<short>();
            var metatileAttributes = tileset.MetatileAttributes.OffsetBy(index * attributeDataLength);

            reader.GoToPointer(metatileAttributes);
            metatile.RawBehavior = reader.ReadBytesAsInt(attributeDataLength);

            reader.GoToPointer(metatileAttributes.OffsetBy(attributeDataLength - 1));
            byte backgroundId = reader.ReadByte();

            metatile.IsOverlay = backgroundId == (int) MetatileBackground.Overlay || backgroundId == (int)MetatileBackground.UNKNOWN_Overlay;
            
            metatile.EncounterType = (MetatileEncounter)((metatile.RawBehavior & ENCOUNTER_MASK) >> ENCOUNTER_SHIFT);

            var rawDetectedAttributes = metatile.RawBehavior & ATTRIBUTE_MASK;
            metatile.Attributes = new OrderedDictionary<string, int>();
            var allPossibleAttributes = reader.Constants.MapTileAttributes;

            // Work backwards and check biggest flags first
            for (int i = allPossibleAttributes.Count - 1; i > 0; i--)
            {
                var possibleAttribute = allPossibleAttributes[i].Value;

                if ((rawDetectedAttributes & possibleAttribute) == possibleAttribute)
                {
                    metatile.Attributes.Add(allPossibleAttributes[i]);
                    rawDetectedAttributes = rawDetectedAttributes & ~possibleAttribute;

                    if (reader.Constants.MapTileAttributesSurfOnly.Contains(possibleAttribute))
                        metatile.IsSurfOnly = true;

                    if (reader.Constants.MapTileAttributesJumpNorth == possibleAttribute)
                        metatile.IsJumpNorth = true;

                    if (reader.Constants.MapTileAttributesJumpEast == possibleAttribute)
                        metatile.IsJumpEast = true;

                    if (reader.Constants.MapTileAttributesJumpSouth == possibleAttribute)
                        metatile.IsJumpSouth = true;

                    if (reader.Constants.MapTileAttributesJumpWest == possibleAttribute)
                        metatile.IsJumpWest = true;
                }
            }

            // If any attributes are left unhandled
            if (rawDetectedAttributes > 0)
                throw new NotImplementedException($"Unknown attribute({rawDetectedAttributes}) specified on metatile({originalIndex})");

            var tiles = tileset.Metatiles.OffsetBy(index * DATA_LENGTH);
            wasCompletelyEmpty = true;

            for (var tileIndex = 0; tileIndex < DATA_LENGTH / dataLength; tileIndex++)
            {
                reader.GoToPointer(tiles.OffsetBy(tileIndex * dataLength));

                var tile = Tile.ReadTile(reader, primaryTileset, secondaryTileset, out bool wasEmpty);
                metatile.tiles[tileIndex] = tile;

                if (!wasEmpty)
                    wasCompletelyEmpty = false;
            }

            return metatile;
        }
    }
}
