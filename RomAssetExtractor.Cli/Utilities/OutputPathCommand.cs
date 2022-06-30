using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Cli.Utilities
{
    class OutputPathCommand : Command
    {
        private const string OUTPUT_DEFAULT = "output";

        private static Command instance;
        public static Command Instance
        {
            get
            {
                if (instance == null)
                    instance = new OutputPathCommand();

                return instance;
            }
        }

        private string outputArgument;

        public override string[] GetCommands() => new[]
        {
            "--output",
            "-o",
        };

        public override void AppendHelpText(StringBuilder helpTextBuilder, string tabs)
        {
            helpTextBuilder.Append(tabs);
            helpTextBuilder.AppendLine("The path to where the assets should be extracted.");
            helpTextBuilder.Append(tabs);
            helpTextBuilder.Append("Defaults to: ");
            helpTextBuilder.Append(OUTPUT_DEFAULT);
        }

        public override int GetArgumentCount()
            => 1;
        public override bool GetIsRequired()
            => false;

        public override void Consume(string argument)
            => outputArgument = argument;

        public override bool Execute()
        {
            if (outputArgument == null)
                throw new ArgumentException("No output path specified!");

            if (!Directory.Exists(outputArgument))
                throw new ArgumentException("Invalid output path specified!");

            Program.OutputDirectory = outputArgument;
            return true;
        }

        public override bool ExecuteDefault()
        {
            Program.OutputDirectory = OUTPUT_DEFAULT;
            return true;
        }
    }
}
