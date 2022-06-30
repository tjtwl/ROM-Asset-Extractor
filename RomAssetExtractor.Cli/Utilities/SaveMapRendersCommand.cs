using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Cli.Utilities
{
    class SaveMapRendersCommand : Command
    {
        private const bool DEFAULT_SAVE = false;

        private static Command instance;
        public static Command Instance
        {
            get
            {
                if (instance == null)
                    instance = new SaveMapRendersCommand();

                return instance;
            }
        }

        private bool shouldSave;

        public override string[] GetCommands() => new[]
        {
            "--save-map-renders",
            "-smr",
        };

        public override void AppendHelpText(StringBuilder helpTextBuilder, string tabs)
        {
            helpTextBuilder.Append(tabs);
            helpTextBuilder.AppendLine("Indicate that full map renders should be saved alongside the tileset bitmaps by specifying True or False.");
            helpTextBuilder.Append(tabs);
            helpTextBuilder.Append("Defaults to: ");
            helpTextBuilder.Append(DEFAULT_SAVE);
        }

        public override int GetArgumentCount()
            => 1;

        public override bool GetIsRequired()
            => false;

        public override void Consume(string argument)
            => shouldSave = argument == "1" ? true 
            : (argument == "0" ? false 
                : bool.Parse(argument));

        public override bool Execute()
        {
            Program.ShouldSaveMapRenders = shouldSave;
            return true;
        }

        public override bool ExecuteDefault()
        {
            Program.ShouldSaveMapRenders = DEFAULT_SAVE;
            return true;
        }
    }
}
