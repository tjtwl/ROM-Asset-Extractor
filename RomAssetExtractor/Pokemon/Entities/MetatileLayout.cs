using Newtonsoft.Json;
using RomAssetExtractor.GbaSystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Pokemon.Entities
{
    public enum MetatileLayer
    {
        Bottom,
        Top
    }

    public class MetatileLayout
    {
        public const int MAX_METATILES_COUNT = 1024;
        public const int LAYER_COUNT = 2;

        private const int METATILE_MASK_ID = 0x03FF;
        private const int METATILE_MASK_BEHAVIOR = 0xFC00;
        private const int METATILE_SHIFT_BEHAVIOR = 10;

        private int[] Ids { get; set; }
        private int[] Metas { get; set; }

        private Tileset[] Tilesets { get; set; }
        private Tileset PrimaryTileset => Tilesets[0];
        private Tileset SecondaryTileset => Tilesets[1];

        private readonly Dictionary<int, Metatile> cache;
        private Metatile[] orderedMapMetatiles;
        private int currentTilesetIndex;
        private int usedMetatileCount;

        private MetatileLayout(int mapMetatileCount, Tileset primary, Tileset secondary)
        {
            Ids = new int[mapMetatileCount];
            Metas = new int[mapMetatileCount];
            Tilesets = new Tileset[2];
            cache = new Dictionary<int, Metatile>(MAX_METATILES_COUNT);

            currentTilesetIndex = 0;
            this.usedMetatileCount = 0;

            AddTileset(primary);
            AddTileset(secondary);
        }

        private void AddTileset(Tileset tileset)
        {
            if (currentTilesetIndex > 0)
            {
                var otherTileset = Tilesets[currentTilesetIndex - 1];
                tileset.AlternatePixelValues = otherTileset.PixelValues;
                otherTileset.AlternatePixelValues = tileset.PixelValues;
            }

            Tilesets[currentTilesetIndex++] = tileset;

            usedMetatileCount += tileset.MetatileCount;

            if (usedMetatileCount > MAX_METATILES_COUNT)
                throw new NotImplementedException("This shouldn't happen!");
        }

        internal Bitmap RenderTilesetBitmap(PokemonRomReader reader, MetatileLayer layer)
        {
            var metatilesWide = 8;
            metatilesWide = Math.Min(metatilesWide, usedMetatileCount);

            var width = metatilesWide * Metatile.SIZE_IN_PIXELS;
            var height = (usedMetatileCount / metatilesWide * Metatile.SIZE_IN_PIXELS) + Metatile.SIZE_IN_PIXELS;// Something is wrong with my math here? The + Metatile.SIZE_IN_PIXELS seems odd...
            var bitmap = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                for (int metatileIndex = 0; metatileIndex < usedMetatileCount; metatileIndex++)
                {
                    var metatile = GetMetatile(reader, metatileIndex);

                    if (metatile == null)
                        continue;

                    var metatileBitmap = metatile.RenderBitmap(reader, layer);
                    var x = Metatile.SIZE_IN_PIXELS * metatileIndex % width;
                    var y = (int)Math.Floor((double)(Metatile.SIZE_IN_PIXELS * metatileIndex) / width) * Metatile.SIZE_IN_PIXELS;

                    graphics.DrawImage(metatileBitmap, x, y);
                }
            }

            return bitmap;
        }

        internal Metatile GetMetatile(PokemonRomReader reader, int index)
        {
            if (cache.ContainsKey(index))
                return cache[index];

            var metatile = Metatile.ReadMetatile(reader, index, PrimaryTileset, SecondaryTileset, out bool wasCompletelyEmpty);

            if (wasCompletelyEmpty)
                metatile = null;

            cache.Add(index, metatile);

            return metatile;
        }

        public Bitmap RenderBitmap(PokemonRomReader reader, int mapWidth, int mapHeight, MetatileLayer layer)
        {
            var width = mapWidth * Metatile.SIZE_IN_PIXELS;
            var height = mapHeight * Metatile.SIZE_IN_PIXELS;
            var bitmap = new Bitmap(width, height);
            orderedMapMetatiles = new Metatile[width * height];

            using (var graphics = Graphics.FromImage(bitmap))
            {
                for (var row = 0; row < mapHeight; row++)
                {
                    for (var col = 0; col < mapWidth; col++)
                    {
                        var tileIndex = row * mapWidth + col;
                        var metatile = GetMetatile(reader, Ids[tileIndex]);
                        metatile.SetMapAttributes(Metas[tileIndex]);
                        orderedMapMetatiles[tileIndex] = metatile;

                        var x = Metatile.SIZE_IN_PIXELS * col;
                        var y = Metatile.SIZE_IN_PIXELS * row;

                        var metatileBitmap = metatile.RenderBitmap(reader, layer);
                        graphics.DrawImage(metatileBitmap, x, y);
                    }
                }
            }

            return bitmap;
        }

        internal Metatile[] ReadAllMapMetatiles(PokemonRomReader reader, Map map)
        {
            if (orderedMapMetatiles != null)
                return orderedMapMetatiles;

            orderedMapMetatiles = new Metatile[map.Layout.Width * map.Layout.Height];

            for (var row = 0; row < map.Layout.Height; row++)
            {
                for (var col = 0; col < map.Layout.Width; col++)
                {
                    var tileIndex = row * map.Layout.Width + col;
                    var metatile = GetMetatile(reader, Ids[tileIndex]);

                    if(metatile != null)
                        metatile.SetMapAttributes(Metas[tileIndex]);

                    orderedMapMetatiles[tileIndex] = metatile;
                }
            }

            return orderedMapMetatiles;
        }

        internal static MetatileLayout ReadMetatileData(PokemonRomReader reader, Map map, Tileset primary, Tileset secondary)
        {
            var metatileData = new MetatileLayout(map.Layout.Width * map.Layout.Height, primary, secondary);

            var metatileIndex = 0;
            for (var y = 0; y < map.Layout.Height; y++)
            {
                for (var x = 0; x < map.Layout.Width; x++)
                {
                    reader.GoToPointer(map.Layout.TileData.OffsetBy(reader.GetSizeOf<short>(metatileIndex)));

                    var raw = reader.ReadShort();
                    metatileData.Ids[metatileIndex] = raw & METATILE_MASK_ID;
                    metatileData.Metas[metatileIndex] = (raw & METATILE_MASK_BEHAVIOR) >> METATILE_SHIFT_BEHAVIOR;

                    metatileIndex++;
                }
            }

            return metatileData;
        }
    }
}
