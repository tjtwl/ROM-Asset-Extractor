using Newtonsoft.Json;
using RomAssetExtractor.Pokemon;
using RomAssetExtractor.Pokemon.Entities;
using RomAssetExtractor.GbaSystem;
using RomAssetExtractor.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using static RomAssetExtractor.Pokemon.Entities.BattleAnimScriptCommand;
using static RomAssetExtractor.Pokemon.PokemonRomConstants;

namespace RomAssetExtractor
{
    public class AssetExtractor : IDisposable
    {
        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter> {
                new Newtonsoft.Json.Converters.StringEnumConverter()
            }
        };

        private const string DIRECTORY_POKEMON = "pokemon";
        private const string DIRECTORY_POKEMON_BACK = "back";
        private const string DIRECTORY_POKEMON_SHINY = "shiny";
        private const string DIRECTORY_ICONS = "icons";

        private const string DIRECTORY_OVERWORLD = "overworld";

        private const string DIRECTORY_TRAINERS = "trainers";
        private const string DIRECTORY_TRAINERS_BACK = "back";

        private const string DIRECTORY_TILES = "tiles";
        private const string DIRECTORY_MAPS = "maps";

        private const string DIRECTORY_BATTLE_ANIMS = "battle_anims";

        //private const int SUBSPRITE_TABLE_COUNT = 6;

        private readonly PokemonRomReader reader;
        private string outputDirectory;

        public AssetExtractor(FileStream romFileStream, FileStream romInfoYamlFileStream, string outputDirectory)
        {
            reader = new PokemonRomReader(romFileStream);
            reader.Init(FindPokemonRomInfo(romInfoYamlFileStream));
            PrepareOutputDirectories(outputDirectory);
        }

        private void PrepareOutputDirectories(string outputDirectory)
        {
            this.outputDirectory = outputDirectory;

            Directory.CreateDirectory(outputDirectory);
            Directory.CreateDirectory(Path.Combine(outputDirectory, DIRECTORY_POKEMON, DIRECTORY_ICONS));
            Directory.CreateDirectory(Path.Combine(outputDirectory, DIRECTORY_POKEMON, DIRECTORY_POKEMON_SHINY));
            Directory.CreateDirectory(Path.Combine(outputDirectory, DIRECTORY_POKEMON, DIRECTORY_POKEMON_BACK, DIRECTORY_POKEMON_SHINY));
            Directory.CreateDirectory(Path.Combine(outputDirectory, DIRECTORY_OVERWORLD));
            Directory.CreateDirectory(Path.Combine(outputDirectory, DIRECTORY_TRAINERS, DIRECTORY_TRAINERS_BACK));
            Directory.CreateDirectory(Path.Combine(outputDirectory, DIRECTORY_MAPS, DIRECTORY_TILES));
            Directory.CreateDirectory(Path.Combine(outputDirectory, DIRECTORY_BATTLE_ANIMS));
        }

        public void Dispose()
            => reader.Dispose();

        private static PokemonRomConstants[] DeserializePokemonRomInfos(FileStream romInfoYamlFileStream)
        {
            var deserializer = new DeserializerBuilder()
                .Build();
            var parser = new MergingParser(new Parser(new StreamReader(romInfoYamlFileStream)));

            return deserializer.Deserialize<PokemonRomConstants[]>(parser);
        }

        public PokemonRomConstants FindPokemonRomInfo(FileStream romInfoYamlFileStream)
        {
            var pokemonRomInfos = DeserializePokemonRomInfos(romInfoYamlFileStream);

            reader.GoToOffset(0xAC);

            var gameCode = reader.ReadString(4);

            foreach (var romInfo in pokemonRomInfos)
            {
                if (romInfo.Code == gameCode)
                    return romInfo;
            }

            return null;
        }

        public Bank[] ExtractAllMapData(out int mapCount, bool includeMapRenders, BitmapSaver bitmapSaver = null)
        {
            var mapBanks = Map.ReadAllMapBanks(reader, out mapCount);

            if (bitmapSaver == null)
                return mapBanks;

            foreach (var bank in mapBanks)
            {
                foreach (var map in bank.Maps)
                {
                    var tilesetBitmapBottom = map.CreateTilesetBitmap(reader, MetatileLayer.Bottom);
                    bitmapSaver.Add(new SaveableBitmap
                    {
                        Bitmap = tilesetBitmapBottom,
                        Path = Path.Combine(outputDirectory, DIRECTORY_MAPS, DIRECTORY_TILES, $"{bank.Id}.{map.Id}_tileset.png")
                    });

                    // Metatile parts to be drawn on top of players (plants, bridges, tall houses, etc)
                    var tilesetBitmapTop = map.CreateTilesetBitmap(reader, MetatileLayer.Top);
                    bitmapSaver.Add(new SaveableBitmap
                    {
                        Bitmap = tilesetBitmapTop,
                        Path = Path.Combine(outputDirectory, DIRECTORY_MAPS, DIRECTORY_TILES, $"{bank.Id}.{map.Id}_tileset_overlays.png")
                    });

                    if (includeMapRenders)
                    {
                        var bitmap = map.CreateBitmap(reader);

                        bitmapSaver.Add(new SaveableBitmap
                        {
                            Bitmap = bitmap,
                            Path = Path.Combine(outputDirectory, DIRECTORY_MAPS, $"{bank.Id}.{map.Id}.png")
                        });
                    }

                    // Ignore ContextSwitchDeadlock 
                    Thread.CurrentThread.Join(0);
                }
            }

            return mapBanks;
        }

        private object ReadAllMapMetatiles(Map map)
        {
            return map.Metatiles.ReadAllMapMetatiles(reader, map);
        }

        public void FillMapsWithPokemonEncounters(Bank[] banks, SpeciesModel[] monsters)
        {
            WildSpeciesEncounters.FillMapsWithEncounters(reader, ref banks, monsters);
        }

        private void ExtractTrainerSprites(int spriteCount, SpriteReference[] pointers, SpriteReference[] palettePointers, string pathPart = null, BitmapSaver bitmapSaver = null)
        {
            for (int i = 0; i < spriteCount; i++)
            {
                var outputName = $"{i}.png";

                reader.GoToPointer(pointers[i].Pointer);
                var pixels = reader.ReadCompressedSprite();

                reader.GoToPointer(palettePointers[i].Pointer);
                var palette = reader.ReadCompressedPaletteColors(true);

                if (bitmapSaver != null)
                    bitmapSaver.Add(new SaveableBitmap
                    {
                        Bitmap = GbaGraphics.ToBitmap(PokemonRomReader.ToSequence(pixels), palette),
                        Path = pathPart != null
                            ? Path.Combine(outputDirectory, DIRECTORY_TRAINERS, pathPart, outputName)
                            : Path.Combine(outputDirectory, DIRECTORY_TRAINERS, outputName)
                    });
            }
        }

        public void ExtractAllTrainers(BitmapSaver bitmapSaver = null)
        {
            var frontSpriteCount = reader.Constants.TrainerSpriteCount;

            reader.GoToPointer(reader.Constants.TrainerSprites);
            var frontPointers = reader.ReadMany<SpriteReference>(frontSpriteCount);

            reader.GoToPointer(reader.Constants.TrainerPalettes);
            var frontPalettePointers = reader.ReadMany<SpriteReference>(frontSpriteCount);

            ExtractTrainerSprites(frontSpriteCount, frontPointers, frontPalettePointers, bitmapSaver: bitmapSaver);

            var backSpriteCount = reader.Constants.TrainerBackSpriteCount;

            reader.GoToPointer(reader.Constants.TrainerBackSprites);
            var backPointers = reader.ReadMany<SpriteReference>(backSpriteCount);

            reader.GoToPointer(reader.Constants.TrainerBackPalettes);
            var backPalettePointers = reader.ReadMany<SpriteReference>(backSpriteCount);

            ExtractTrainerSprites(backSpriteCount, backPointers, backPalettePointers, DIRECTORY_TRAINERS_BACK, bitmapSaver);
        }

        // TODO: Clean up this repetetive code
        public SpeciesModel[] ExtractAllPokemon(BitmapSaver bitmapSaver = null)
        {
            var species = new List<SpeciesModel>();
            var overallIndex = 0;

            foreach (var speciesConstantPair in reader.Constants.SpeciesConstants)
            {
                var speciesConstant = speciesConstantPair.Value;

                if (!speciesConstant.Skip) 
                { 
                    for (int i = 0; i < speciesConstant.Amount; i++)
                    {
                        var index = overallIndex + i;

                        reader.GoToPointer(reader.Constants.SpeciesBaseStats.OffsetBy((index + speciesConstant.SpeciesBaseStatsOffset) * reader.GetSizeOf<BaseStats>()));
                        var baseStats = reader.Read<BaseStats>();

                        reader.GoToPointer(reader.Constants.SpeciesIcons.OffsetBy(index * reader.GetSizeOf<Pointer>()));
                        var iconPointer = reader.ReadPointer();

                        var spriteReferenceOffset = index * reader.GetSizeOf<SpriteReference>();

                        reader.GoToPointer(reader.Constants.SpeciesSprites.OffsetBy(spriteReferenceOffset));
                        var frontPointer = reader.Read<SpriteReference>();
                        var gameIndex = frontPointer.GameIndex;

                        reader.GoToPointer(reader.Constants.SpeciesBackSprites.OffsetBy(spriteReferenceOffset));
                        var backPointer = reader.Read<SpriteReference>();

                        reader.GoToPointer(reader.Constants.SpeciesPalettes.OffsetBy(spriteReferenceOffset));
                        var palettePointer = reader.Read<SpriteReference>();

                        reader.GoToPointer(reader.Constants.SpeciesShinyPalettes.OffsetBy(spriteReferenceOffset));
                        var shinyPalettePointer = reader.Read<SpriteReference>();

                        string name = "";

                        if (speciesConstant.DataType == SpeciesDataType.AlphabetName)
                            name = $"UNOWN {(char)(i + speciesConstant.AsciiOffset)}";
                        else if (speciesConstant.DataType == SpeciesDataType.IsEgg)
                            name = "EGG";
                        else if(speciesConstant.DataType == SpeciesDataType.NormalName)
                        {
                            reader.GoToPointer(reader.Constants.SpeciesNames.OffsetBy(index * reader.Constants.SpeciesNameSize));
                            name = reader.ReadString();
                        }

                        var splitChar = string.Empty;

                        if (!string.IsNullOrWhiteSpace(name))
                            splitChar = "_";

                        var outputName = $"{gameIndex}{splitChar}{name.MakePathSafe()}.png";
                        var iconPath = Path.Combine(DIRECTORY_ICONS, outputName);
                        var frontPath = Path.Combine(outputName);
                        var frontShinyPath = Path.Combine(DIRECTORY_POKEMON_SHINY, outputName);
                        var backPath = Path.Combine(DIRECTORY_POKEMON_BACK, outputName);
                        var backShinyPath = Path.Combine(DIRECTORY_POKEMON_BACK, DIRECTORY_POKEMON_SHINY, outputName);

                        reader.GoToPointer(reader.Constants.SpeciesEvolutions.OffsetBy(index * reader.Constants.SpeciesMaxEvolutions  * reader.GetSizeOf<Evolution>()));
                        var evolutions = reader.ReadManyNotNull<Evolution>(reader.Constants.SpeciesMaxEvolutions);

                        var numSpeeds = reader.Constants.MaxLevel + 1;
                        reader.GoToPointer(reader.Constants.ExperienceSpeeds.OffsetBy(baseStats.growthRate * numSpeeds * sizeof(uint)));
                        var experienceSpeeds = reader.ReadMany<uint>(numSpeeds);

                        var monster = new SpeciesModel(gameIndex, name)
                        {
                            BaseStats = speciesConstant.SpeciesBaseStatsOffset == SpeciesConstant.NO_BASE_STAT ? default : baseStats,
                            Evolutions = evolutions,
                            ExperienceSpeeds = experienceSpeeds,
                            Tag = speciesConstantPair.Key,

                            IconPath = iconPath,
                            FrontPath = frontPath,
                            FrontShinyPath = frontShinyPath,
                            BackPath = backPath,
                            BackShinyPath = backShinyPath,
                        };
                        species.Add(monster);

                        File.WriteAllText(
                            path: Path.Combine(outputDirectory, DIRECTORY_POKEMON, $"{gameIndex}.json"),
                            contents: JsonConvert.SerializeObject(monster, Formatting.Indented, SerializerSettings)
                        );

                        reader.GoToPointer(frontPointer.Pointer);
                        var frontPixels = reader.ReadCompressedSprite();

                        reader.GoToPointer(backPointer.Pointer);
                        var backPixels = reader.ReadCompressedSprite();

                        reader.GoToPointer(palettePointer.Pointer);
                        var colors = reader.ReadCompressedPaletteColors(true);

                        reader.GoToPointer(shinyPalettePointer.Pointer);
                        var shinyPalette = reader.ReadCompressedPaletteColors(true);

                        reader.GoToPointer(reader.Constants.SpeciesIconPaletteChoices.OffsetBy(frontPointer.GameIndex));
                        var paletteIndex = reader.ReadByte();

                        reader.GoToPointer(reader.Constants.SpeciesIconPalettes.OffsetBy(reader.GetSizeOf<Palette>(paletteIndex)));
                        var iconPalette = reader.Read<Palette>();

                        reader.GoToPointer(iconPointer);
                        var iconPixels = reader.ReadSprite(32, 64);

                        if (bitmapSaver != null)
                        {
                            bitmapSaver.Add(new SaveableBitmap
                            {
                                Bitmap = GbaGraphics.ToBitmap(
                                        pixels: PokemonRomReader.ToSequence(iconPixels),
                                        colorPalette: iconPalette.ToTrueColors(),
                                        width: 32,
                                        height: 64),
                                Path = Path.Combine(outputDirectory, DIRECTORY_POKEMON, iconPath)
                            });

                            //# XXX Castform and Deoxys have multiple forms. The sprites (and
                            //# palettes, in Castform's case) are all lumped together. They
                            //# should be split into separate images.
                            bitmapSaver.Add(new SaveableBitmap
                            {
                                Bitmap = GbaGraphics.ToBitmap(
                                        pixels: PokemonRomReader.ToSequence(frontPixels),
                                        colorPalette: colors),
                                Path = Path.Combine(outputDirectory, DIRECTORY_POKEMON, frontPath)
                            });
                            bitmapSaver.Add(new SaveableBitmap
                            {
                                Bitmap = GbaGraphics.ToBitmap(
                                        pixels: PokemonRomReader.ToSequence(frontPixels),
                                        colorPalette: shinyPalette),
                                Path = Path.Combine(outputDirectory, DIRECTORY_POKEMON, frontShinyPath)
                            });

                            bitmapSaver.Add(new SaveableBitmap
                            {
                                Bitmap = GbaGraphics.ToBitmap(
                                        pixels: PokemonRomReader.ToSequence(backPixels),
                                        colorPalette: colors),
                                Path = Path.Combine(outputDirectory, DIRECTORY_POKEMON, backPath)
                            });
                            bitmapSaver.Add(new SaveableBitmap
                            {
                                Bitmap = GbaGraphics.ToBitmap(
                                        pixels: PokemonRomReader.ToSequence(backPixels),
                                        colorPalette: shinyPalette),
                                Path = Path.Combine(outputDirectory, DIRECTORY_POKEMON, backShinyPath)
                            });
                        }
                    }
                }

                overallIndex += speciesConstant.Amount;
            }

            return species.ToArray();
        }

        public unsafe void ExtractAllOverworldSprites(BitmapSaver bitmapSaver, bool combineRelatedIntoSheet = true)
        {
            reader.GoToPointer(reader.Constants.OverworldSprites);
            var pointers = reader.ReadPointers(reader.Constants.OverworldSpriteCount);
            var spriteReferences = new OverworldSpriteReference[reader.Constants.OverworldSpriteCount];

            for (int i = 0; i < reader.Constants.OverworldSpriteCount; i++)
            {
                reader.GoToPointer(pointers[i]);
                spriteReferences[i] = reader.Read<OverworldSpriteReference>();
            }

            for (int i = 0; i < spriteReferences.Length; i++)
            {
                var spriteReference = spriteReferences[i];
                SpriteFrameImage spriteFrame;
                SpriteFrameImage nextSpriteFrame;
                int nextSpriteFrameOffset;
                int spriteFrameIndex = 0;

                List<Bitmap> relatedBitmaps = null;

                if (combineRelatedIntoSheet)
                    relatedBitmaps = new List<Bitmap>();

                do
                {
                    reader.GoToOffset(spriteReference.SpriteFramePointers + (spriteFrameIndex * sizeof(SpriteFrameImage)));

                    var outputName = $"{i}_{spriteFrameIndex++}.png";
                    spriteFrame = reader.Read<SpriteFrameImage>();
                    nextSpriteFrameOffset = reader.Position;
                    nextSpriteFrame = reader.Peek<SpriteFrameImage>();

                    reader.GoToOffset(spriteFrame.DataPointer);
                    var pixels = reader.ReadSprite(spriteReference.Width, spriteReference.Height, spriteReference.FrameSize);

                    // Find the palette
                    reader.GoToPointer(reader.Constants.OverworldPalettes);
                    int palettePointer = -1;

                    do
                    {
                        var paletteReference = reader.Read<OverworldPaletteReference>();

                        if (paletteReference.Index == spriteReference.PaletteIndex)
                            palettePointer = paletteReference.Pointer;
                    } while (palettePointer == -1);

                    reader.GoToOffset(palettePointer);
                    var palette = reader.Read<Palette>();

                    var bitmap = GbaGraphics.ToBitmap(
                                pixels: PokemonRomReader.ToSequence(pixels),
                                colorPalette: palette.ToTrueColors(),
                                width: spriteReference.Width,
                                height: spriteReference.Height);

                    if (combineRelatedIntoSheet)
                        relatedBitmaps.Add(bitmap);
                    else
                        bitmapSaver.Add(new SaveableBitmap
                        {
                            Bitmap = bitmap,
                            Path = Path.Combine(outputDirectory, DIRECTORY_OVERWORLD, outputName)
                        });
                } while (nextSpriteFrame.DataPointer - spriteFrame.DataPointer == spriteFrame.Size
                    && Array.FindIndex(spriteReferences, otherReference => otherReference.SpriteFramePointers == nextSpriteFrameOffset) == -1);

                if (combineRelatedIntoSheet)
                    bitmapSaver.Add(new SaveableBitmap
                    {
                        Bitmap = relatedBitmaps.Combine(),
                        Path = Path.Combine(outputDirectory, DIRECTORY_OVERWORLD, $"{i}.png")
                    });
            }
        }

        // TODO: Also save move anim json
        private unsafe void ExtractAllMoves(BitmapSaver bitmapSaver)
        {
            reader.GoToPointer(reader.Constants.Moves);

            var movesWithQuietBGM = new short[reader.Constants.MovesQuietBgmCount];

            for (int i = 0; i < reader.Constants.MovesQuietBgmCount; i++)
            {
                movesWithQuietBGM[i] = reader.ReadShort();
            }

            while (reader.PeekByte() == 0xFF)
                reader.Skip(1);

            var movePointers = reader.ReadPointers(reader.Constants.MovesCount);
            var statusPointers = reader.ReadPointers(reader.Constants.MovesStatusCount);
            var generalPointers = reader.ReadPointers(reader.Constants.MovesGeneralCount);
            var specialPointers = reader.ReadPointers(reader.Constants.MovesSpecialCount);

            reader.GoToPointer(reader.Constants.MoveNames);
            var moveNames = reader.ReadStringArray(reader.Constants.MovesCount);

            // TODO Go through all Moves, parse the scripts and store them in json. For any createsprite call, put the animation reference in
            var allMoveAnimationScripts = new List<BattleAnimScript>(movePointers.Length + statusPointers.Length + generalPointers.Length + specialPointers.Length);
            BattleAnimScript.ReadManyAnimScriptAtPointers(reader, movePointers, ref allMoveAnimationScripts);
            BattleAnimScript.ReadManyAnimScriptAtPointers(reader, statusPointers, ref allMoveAnimationScripts);
            BattleAnimScript.ReadManyAnimScriptAtPointers(reader, generalPointers, ref allMoveAnimationScripts);
            BattleAnimScript.ReadManyAnimScriptAtPointers(reader, specialPointers, ref allMoveAnimationScripts);

            reader.GoToPointer(reader.Constants.BattleAnimParticles);
            var particleReferences = reader.ReadMany<BattleAnimSpriteReference>(reader.Constants.BattleAnimCount);
            var paletteReferences = reader.ReadMany<BattleAnimPaletteReference>(reader.Constants.BattleAnimCount);

            var sizes = new Dictionary<int, Size>();

            // Discover unused Oam Data for battle animations
            if (!reader.Constants.BattleAnimTemplatesUnknown.IsNull)
            {
                reader.GoToPointer((Pointer)reader.Constants.BattleAnimTemplatesUnknown);
                var spriteTemplates = reader.ReadMany<BattleAnimSpriteTemplateReference>(reader.Constants.BattleAnimTemplatesUnknownCount);

                foreach (var spriteTemplate in spriteTemplates)
                {
                    reader.GoToOffset(spriteTemplate.OamPointer);
                    var oamData = reader.Read<OamData>();

                    if (!sizes.ContainsKey(spriteTemplate.TileIndex))
                        sizes.Add(spriteTemplate.TileIndex, oamData.Size);
                }
            }

            void DiscoverSizeFromSpriteTemplate(BattleAnimScriptModel animScript)
            {
                foreach (var scriptCall in animScript.ScriptCalls)
                {
                    if (scriptCall.CommandByte == CommandByte.CreateSprite)
                    {
                        var spriteTemplatePointer = scriptCall.GetArgument<int>(0);
                        reader.GoToOffset(spriteTemplatePointer);
                        var spriteTemplate = reader.Read<BattleAnimSpriteTemplateReference>();
                        reader.GoToOffset(spriteTemplate.OamPointer);
                        var oamData = reader.Read<OamData>();

                        if (!sizes.ContainsKey(spriteTemplate.TileIndex))
                            sizes.Add(spriteTemplate.TileIndex, oamData.Size);
                    }
                }

                if (animScript.SubScripts != null)
                {
                    foreach (var subScript in animScript.SubScripts)
                    {
                        DiscoverSizeFromSpriteTemplate(subScript);
                    }
                }
            }

            foreach (var animScript in allMoveAnimationScripts)
            {
                DiscoverSizeFromSpriteTemplate(animScript);
            }

            foreach (var particleReference in particleReferences)
            {
                reader.GoToOffset(particleReference.Pointer);

                var width = PokemonRomReader.SPRITE_TILE_SIZE;
                if (sizes.ContainsKey(particleReference.TileIndex))
                {
                    var size = sizes[particleReference.TileIndex];
                    width = size.Width;
                }

                var pixels = reader.ReadCompressedSprite(out var height, width);
                var paletteReference = Array.Find(paletteReferences, p => p.TileIndex == particleReference.TileIndex);

                if (paletteReference.Pointer == 0x00)
                    continue;

                reader.GoToOffset(paletteReference.Pointer);

                var colors = reader.ReadCompressedPaletteColors(true);

                var bitmap = GbaGraphics.ToBitmap(
                            pixels: PokemonRomReader.ToSequence(pixels),
                            colorPalette: colors,
                            width: width,
                            height: height);

                bitmapSaver.Add(new SaveableBitmap
                {
                    Bitmap = bitmap,
                    Path = Path.Combine(outputDirectory, DIRECTORY_BATTLE_ANIMS, $"{particleReference.TileIndex}.png")
                });
            }
        }

        public static async Task ExtractRom(
                string romPath,
                string outputDirectory,
                bool saveBitmaps = true,
                bool saveTrainers = true,
                bool saveMaps = true,
                bool saveMapRenders = true,
                string yamlPath = "pokeroms.yml",
                TextWriter logWriter = null)
        {
            var romInfoYamlFileStream = File.OpenRead(yamlPath);
            var romFileStream = File.OpenRead(romPath);

            void WriteLogLine(string message = null)
            {
                if (logWriter != null)
                {
                    logWriter.WriteLine(message);
                    logWriter.Flush();
                }
                else
                {
                    Console.WriteLine(message);
                }
            }

            using (var extractor = new AssetExtractor(romFileStream, romInfoYamlFileStream, outputDirectory))
            {
                var bitmapSaver = saveBitmaps ? new BitmapSaver() : null;

                if (bitmapSaver != null)
                    bitmapSaver.StartWriting();
                else
                    WriteLogLine("Skipping bitmap saving for all following assets.");

                if (bitmapSaver != null)
                {
                    Console.WriteLine("Extracting move particle sprites");
                    extractor.ExtractAllMoves(bitmapSaver);
                    Console.WriteLine("Done.");

                    WriteLogLine("Extracting Overworld sprites...");
                    extractor.ExtractAllOverworldSprites(bitmapSaver);
                    WriteLogLine("Done.");
                    WriteLogLine();
                }

                WriteLogLine("Extracting Pokémon...");
                var monsters = extractor.ExtractAllPokemon(bitmapSaver);
                WriteLogLine("Done.");
                WriteLogLine();

                if (saveTrainers)
                {
                    WriteLogLine("Extracting Trainers...");
                    extractor.ExtractAllTrainers(bitmapSaver);
                    WriteLogLine("Done.");
                    WriteLogLine();
                }

                if (saveMaps)
                {
                    WriteLogLine("Extracting Map data...");
                    var mapBanks = extractor.ExtractAllMapData(out int mapCount, saveMapRenders, bitmapSaver);
                    WriteLogLine("Done.");
                    WriteLogLine();
                    WriteLogLine("Filling maps with encounter data...");
                    extractor.FillMapsWithPokemonEncounters(mapBanks, monsters);
                    WriteLogLine("Done.");

                    // TODO: Write map data to seperate json files so memory doesn't fill up with giant objects.
                    WriteLogLine("Saving each maps data as two JSON files (one for metadata, the other for tile data)...");
                    foreach (var bank in mapBanks)
                    {
                        foreach (var map in bank.Maps)
                        {
                            var fileName = $"{bank.Id}.{map.Id}.json";

                            File.WriteAllText(
                                path: Path.Combine(outputDirectory, DIRECTORY_MAPS, fileName),
                                contents: JsonConvert.SerializeObject(map, Formatting.Indented, SerializerSettings)
                            );

                            File.WriteAllText(
                                path: Path.Combine(outputDirectory, DIRECTORY_MAPS, DIRECTORY_TILES, fileName),
                                contents: JsonConvert.SerializeObject(extractor.ReadAllMapMetatiles(map), Formatting.None, SerializerSettings)
                            );
                        }
                    }
                    WriteLogLine("Done.");
                }

                if (bitmapSaver != null)
                    // Wait until the last few tasks have been completed.
                    await bitmapSaver.ForceFinishWriting();
            }
        }
    }
}
