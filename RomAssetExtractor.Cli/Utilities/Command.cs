using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Cli.Utilities
{
    public abstract class Command
    {
        private static List<Command> AllCommands = new List<Command>()
        {
            RomPathCommand.Instance,
            OutputPathCommand.Instance,
            SaveBitmapsCommand.Instance,
            SaveTrainersCommand.Instance,
            SaveMapsCommand.Instance,
            SaveMapRendersCommand.Instance,
        };


        // Which strings (case insensitive) activate this command (e.g: --long-command and -lc)
        public abstract string[] GetCommands();

        // How many arguments should follow this command
        public abstract int GetArgumentCount();

        // Is this command required to be executed?
        public abstract bool GetIsRequired();

        // Returns the commands help text
        public abstract void AppendHelpText(StringBuilder helpTextBuilder, string tabs);

        // Should take in the following arguments and store them
        public abstract void Consume(string argument);

        // Use the stored arguments to execute the task. Return true to allow to proceed.
        public abstract bool Execute();

        // Called when the command is required, but not provided by the user. Return false if no defaults can be set and the user should see an error. True if this method set a default and we can proceed.
        public abstract bool ExecuteDefault();

        public static string GetAllHelpTexts()
        {
            var builder = new StringBuilder();

            builder.AppendLine();
            builder.AppendLine("All possible commands:");
            builder.AppendLine();
            foreach (var command in AllCommands)
            {
                builder.Append("\t");

                var commandStrings = command.GetCommands();

                for (int i = 0; i < commandStrings.Length; i++)
                {
                    if (i > 0)
                        builder.Append(" or ");

                    builder.Append(commandStrings[i]);
                }

                if (command.GetIsRequired())
                    builder.Append(" (required)");

                builder.AppendLine();
                command.AppendHelpText(builder, "\t\t");
                builder.AppendLine();
                builder.AppendLine();
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var commandStrings = GetCommands();

            for (int i = 0; i < commandStrings.Length; i++)
            {
                if (i > 0)
                    builder.Append(" or ");

                builder.Append(commandStrings[i]);
            }

            return builder.ToString();
        }

        public static void HandleArguments(string[] args)
        {
            var commands = AllCommands;
            var usedCommands = new List<Command>();

            for (int a = 0; a < args.Length; a++)
            {
                var command = FindCommandByString(args[a]);

                if (command == null)
                    throw new ArgumentException($"{args[a]} is not a valid command!");

                var consumeArgCount = command.GetArgumentCount();

                if (consumeArgCount > 0)
                {
                    for (int i = 1; i <= consumeArgCount; i++)
                    {
                        if (a + i >= args.Length)
                            break;

                        command.Consume(args[a + i]);
                    }

                    a += consumeArgCount;
                }

                usedCommands.Add(command);

                if (!command.Execute())
                    return;
            }

            foreach (var command in commands)
            {
                if (!usedCommands.Contains(command))
                {
                    if (command.ExecuteDefault())
                        continue;

                    if(command.GetIsRequired())
                        throw new ArgumentNullException($"Missing command! You are required to specify {command}");
                }
            }
        }

        private static Command FindCommandByString(string commandString)
        {
            foreach (var command in AllCommands)
            {
                foreach (var potentialCommandString in command.GetCommands())
                {
                    if (string.Equals(potentialCommandString, commandString, StringComparison.CurrentCultureIgnoreCase))
                        return command;
                }
            }

            return null;
        }
    }
}
