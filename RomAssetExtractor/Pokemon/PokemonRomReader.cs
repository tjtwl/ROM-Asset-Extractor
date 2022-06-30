using RomAssetExtractor.Pokemon.Entities;
using RomAssetExtractor.GbaSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace RomAssetExtractor.Pokemon
{
    public class PokemonRomReader : RomReader
    {
        internal const int SPRITE_TILE_SIZE = 8;

        private PokemonRomConstants constants = null;
        public PokemonRomConstants Constants
        {
            get
            {
                if (constants == null)
                    throw new NullReferenceException("Constants are null! Did you call RomReader.Init(PokemonRomConstants) to setup the reader?");

                return constants;
            }
        }

        public PokemonRomReader(FileStream romFileStream)
            :base(romFileStream)
        {
        }

        internal void Init(PokemonRomConstants pokemonRomConstants)
        {
            constants = pokemonRomConstants;
        }

        // Source: https://github.com/jugales/pokewebkit
        public string[] ReadStringArray(int maxEntries)
        {
            var arrayResult = new string[maxEntries];
            int stringCount = 0;

            while (stringCount < maxEntries)
            {
                arrayResult[stringCount++] = ReadString(); // GetOffset().ToString("X") + 
            }

            return arrayResult;
        }

        public string ReadString()
        {
            var stringResult = string.Empty;

            while (true)
            {
                var byteResult = ReadByte();
                var letter = Alphabet.GetLetter(byteResult);

                if (letter != "\\x")
                {
                    stringResult += letter;
                }
                else
                {
                    if (stringResult.Trim() != string.Empty)
                        return stringResult.Trim();

                    stringResult = string.Empty;
                }

            }
        }

        /// Source: HexManiacAdvance
        public void GuessTileSize(int dataLength, out int width, out int height, int bitsPerPixel)
        {
            var tileByteSize = bitsPerPixel * SPRITE_TILE_SIZE;
            var uncompressedSize = dataLength;
            var tileCount = uncompressedSize / tileByteSize;
            var roughSize = Math.Sqrt(tileCount);
            width = (int)Math.Ceiling(roughSize);
            height = (int)roughSize;
            if (width * height < tileCount) height += 1;
        }

        public byte[,] ReadSprite(out int width, out int height, int byteCount = 1024, int bitsPerPixel = 4)
        {
            var data = ReadBytes(byteCount);
            GuessTileSize(data.Length, out width, out height, bitsPerPixel);
            var pixels = Untile(data, 0, width, height, bitsPerPixel);
            width *= SPRITE_TILE_SIZE;
            height *= SPRITE_TILE_SIZE;
            return pixels;
        }

        public byte[,] ReadSprite(int width, int height, int byteCount = 1024, int bitsPerPixel = 4)
        {
            var data = ReadBytes(byteCount);
            width /= SPRITE_TILE_SIZE;
            height /= SPRITE_TILE_SIZE;
            var pixels = Untile(data, 0, width, height, bitsPerPixel);
            return pixels;
        }

        public byte[,] ReadCompressedSprite(int width = 64, int bitsPerPixel = 4)
        {
            return ReadCompressedSprite(out var _, width, bitsPerPixel);
        }

        public byte[,] ReadCompressedSprite(out int height, int width = 64, int bitsPerPixel = 4)
        {
            var data = ReadCompressedData();
            var tileWidth = width / SPRITE_TILE_SIZE;
            var tileHeight = data.Length / (tileWidth * SPRITE_TILE_SIZE * bitsPerPixel);
            var pixels = Untile(data, 0, tileWidth, tileHeight, bitsPerPixel);
            height = tileHeight * SPRITE_TILE_SIZE;
            return pixels;
        }

        public byte[,] ReadCompressedSprite(out int width, out int height, int bitsPerPixel = 4)
        {
            // Logic from LzTilesetRun.cs in HexManiacAdvance
            var data = ReadCompressedData();
            GuessTileSize(data.Length, out width, out height, bitsPerPixel);
            var pixels = Untile(data, 0, width, height, bitsPerPixel);
            width *= SPRITE_TILE_SIZE;
            height *= SPRITE_TILE_SIZE;
            return pixels;
        }

        public Color[] ReadCompressedPaletteColors(bool convertFromBit16)
        {
            var data = ReadCompressedData();

            return GbaColor.UnpackColors(data, convertFromBit16);
        }

        /// Source: HexManiacAdvance
        internal static byte[] ToSequence(byte[,] pixels)
        {
            if (pixels == null) return new byte[0];
            var data = new byte[pixels.Length];
            var width = pixels.GetLength(0);
            for (int i = 0; i < data.Length; i++)
            {
                var pixel = pixels[i % width, i / width];
                data[i] = pixel;
            }
            return data;
        }

        /// Source: HexManiacAdvance
        /// <summary>
        /// convert from raw values to palette-index values
        /// </summary>
        public static byte[,] Untile(byte[] data, int start, int tileWidth, int tileHeight, int bitsPerPixel)
        {
            var result = new byte[8 * tileWidth, 8 * tileHeight];
            for (int y = 0; y < tileHeight; y++)
            {
                int yOffset = y * 8;
                for (int x = 0; x < tileWidth; x++)
                {
                    var tileStart = ((y * tileWidth) + x) * 8 * bitsPerPixel + start;
                    int xOffset = x * 8;
                    if (bitsPerPixel == 4)
                    {
                        for (int i = 0; i < 32; i++)
                        {
                            int xx = i % 4; // ranges from 0 to 3
                            int yy = i / 4; // ranges from 0 to 7
                            var raw = tileStart + i < data.Length ? data[tileStart + i] : 0;
                            result[xOffset + xx * 2 + 0, yOffset + yy] = (byte)(raw & 0xF);
                            result[xOffset + xx * 2 + 1, yOffset + yy] = (byte)(raw >> 4);
                        }
                    }
                    else if (bitsPerPixel == 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int xx = i % 8;
                            int yy = i / 8;
                            var raw = tileStart + i < data.Length ? data[tileStart + i] : 0;
                            result[xOffset + xx, yOffset + yy] = (byte)raw;
                        }
                    }
                    else if (bitsPerPixel == 2)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            int xx = i % 2; // ranges from 0 to 1
                            int yy = i / 2; // ranges from 0 to 7
                            xx = 1 - xx; // swap the left half with the right half
                            var raw = tileStart + i < data.Length ? data[tileStart + i] : 0;
                            result[xOffset + xx * 4 + 0, yOffset + yy] = (byte)((raw >> 6) & 3);
                            result[xOffset + xx * 4 + 1, yOffset + yy] = (byte)((raw >> 4) & 3);
                            result[xOffset + xx * 4 + 2, yOffset + yy] = (byte)((raw >> 2) & 3);
                            result[xOffset + xx * 4 + 3, yOffset + yy] = (byte)((raw >> 0) & 3);
                        }
                    }
                    else if (bitsPerPixel == 1)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            var raw = tileStart + i < data.Length ? data[tileStart + i] : 0;
                            result[xOffset + 0, yOffset + i] = (byte)((raw >> 0) & 1);
                            result[xOffset + 1, yOffset + i] = (byte)((raw >> 1) & 1);
                            result[xOffset + 2, yOffset + i] = (byte)((raw >> 2) & 1);
                            result[xOffset + 3, yOffset + i] = (byte)((raw >> 3) & 1);
                            result[xOffset + 4, yOffset + i] = (byte)((raw >> 4) & 1);
                            result[xOffset + 5, yOffset + i] = (byte)((raw >> 5) & 1);
                            result[xOffset + 6, yOffset + i] = (byte)((raw >> 6) & 1);
                            result[xOffset + 7, yOffset + i] = (byte)((raw >> 7) & 1);
                        }
                    }
                    else
                    {
                        // unsure what to do here...
                    }
                }
            }
            return result;
        }
    }
}
