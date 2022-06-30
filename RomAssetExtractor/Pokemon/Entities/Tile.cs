using Newtonsoft.Json;
using RomAssetExtractor.GbaSystem;
using System;
using System.Drawing;

namespace RomAssetExtractor.Pokemon.Entities
{
    // The 4 small segments that make up a metatile
    public class Tile
    {
        public const int SIZE_IN_PIXELS = 8;
        public const int bitsPerPixel = 8; // TODO: Read this from OamData?

        internal int TileId { get; private set; }
        internal int PaletteId { get; private set; }
        internal bool IsXFlipped { get; private set; }
        internal bool IsYFlipped { get; private set; }

        internal byte[] TilePixelsValues { get; private set; }
        private Color[] palette;

        internal Tile(int bitmapLength)
        {
            TilePixelsValues = new byte[bitmapLength];
        }

        internal Bitmap RenderBitmap()
        {
            var pixels = Tileset.ConvertBytesToTilesetImage(TilePixelsValues, 1, out int width, out int height);
            var length = Math.Min(bitsPerPixel * width * height / 8, pixels.Length);
            var colorPalette = palette;

            // Making first color have no alpha for transparent background:
            colorPalette[0] = Color.FromArgb(0, 0, 0, 0);

            var bitmap = GbaGraphics.ToBitmap(pixels, length, 0, colorPalette, width);

            if (IsXFlipped)
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            if (IsYFlipped)
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return bitmap;
        }

        internal static Tile ReadTile(PokemonRomReader reader, Tileset primaryTileset, Tileset secondaryTileset, out bool wasEmpty)
        {
            var tileBits = reader.ReadShort();

            wasEmpty = tileBits == 0x00;

            var bitmapLength = SIZE_IN_PIXELS * SIZE_IN_PIXELS / MetatileLayout.LAYER_COUNT;
            var tile = new Tile(bitmapLength)
            {
                TileId = tileBits & 0x3FF,
                PaletteId = (tileBits & 0xF000) >> 12,
                IsXFlipped = (tileBits & 0x400) > 0,
                IsYFlipped = (tileBits & 0x800) > 0
            };

            var normalizedTileId = tile.TileId;
            
            if (tile.TileId >= primaryTileset.MetatileCount)
                normalizedTileId -= primaryTileset.MetatileCount;

            try
            {
                if (normalizedTileId == tile.TileId)
                {
                    tile.palette = primaryTileset.Palettes[tile.PaletteId].ToTrueColors();

                    for (int i = 0; i < bitmapLength; i++)
                    {
                        tile.TilePixelsValues[i] = primaryTileset.PixelValues[normalizedTileId * bitmapLength + i];
                    }
                }
                else
                {
                    tile.palette = primaryTileset.Palettes[tile.PaletteId].ToTrueColors();

                    for (int i = 0; i < bitmapLength; i++)
                    {
                        tile.TilePixelsValues[i] = secondaryTileset.PixelValues[normalizedTileId * bitmapLength + i];
                    }
                }
            }
            catch(IndexOutOfRangeException)
            {
                // TODO: Unsure why this happens... Glitched maps?
                wasEmpty = true;
            }

            return tile;
        }
    }
}
