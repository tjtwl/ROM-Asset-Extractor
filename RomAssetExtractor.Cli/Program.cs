using RomAssetExtractor.Cli.Utilities;
using System;

namespace RomAssetExtractor.Cli
{
    public class Program
    {
        public static string OutputDirectory { get; set; }
        public static string RomPath { get; set; }
        public static bool ShouldSaveBitmaps { get; set; }
        public static bool ShouldSaveTrainers { get; set; }
        public static bool ShouldSaveMaps { get; set; }
        public static bool ShouldSaveMapRenders { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Unity ROM Asset Extractor");
            Console.WriteLine("for Gen-III Pokémon games");
            Console.WriteLine("\tby TheJjokerR: https://github.com/thejjokerr/");
            Console.WriteLine();
            Console.WriteLine("\tSome logic and constants have been taken from these open-source projects:");
            Console.WriteLine("\t- Jugales: https://pokewebkit.com/start");
            Console.WriteLine("\t- Magical: https://github.com/magical/pokemon-gba-sprites/");
            Console.WriteLine("\t- Kyoufu Kawa: https://www.romhacking.net/utilities/463/");
            Console.WriteLine();
            Console.Write(Command.GetAllHelpTexts());

            try
            {
                Command.HandleArguments(args);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
                return;
            }

            // Do not close the application until all processing has been completed.
            AssetExtractor.ExtractRom(RomPath, OutputDirectory, ShouldSaveBitmaps, ShouldSaveTrainers, ShouldSaveMaps, ShouldSaveMapRenders).GetAwaiter().GetResult();

            Console.WriteLine("Completely finished with extraction.");
            Console.WriteLine();
            Console.WriteLine("Press any key to close");
            Console.ReadKey();
        }
    }
}
