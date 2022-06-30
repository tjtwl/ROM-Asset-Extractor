using RomAssetExtractor.GbaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RomAssetExtractor.Pokemon.Entities.BattleAnimScriptCommand;

namespace RomAssetExtractor.Pokemon.Entities
{
    internal class BattleAnimScript : BattleAnimScriptModel
    {
        // Expects to be positioned in front of a move script
        internal static BattleAnimScript ReadAnimScript(PokemonRomReader reader)
        {
            var script = new BattleAnimScript();
            script.Pointer = reader.NormalizedPosition;
            bool commandSuccesful;

            do
            {
                var command = Get(reader.ReadByte());
                commandSuccesful = command.TryRead(reader, out var scriptCall);
                script.ScriptCalls.Add(scriptCall);

                if (scriptCall.CommandByte != CommandByte.Call)
                    continue;

                var subScriptOffset = scriptCall.GetArgument<int>(0);
                int existingSubScriptIndex;
                if (script.SubScripts != null && (existingSubScriptIndex = script.SubScripts.FindIndex(sub => sub.Pointer == subScriptOffset)) != -1)
                {
                    scriptCall.Arguments[0] = existingSubScriptIndex;
                    continue;
                }

                if (script.SubScripts == null)
                    script.SubScripts = new List<BattleAnimScriptModel>();

                var mainScriptPosition = reader.NormalizedPosition;

                reader.GoToOffset(subScriptOffset);
                var subScript = ReadAnimScript(reader);
                script.SubScripts.Add(subScript);
                scriptCall.Arguments[0] = script.SubScripts.Count - 1;

                reader.GoToOffset(mainScriptPosition);
            } while (commandSuccesful);

            return script;
        }

        internal static void ReadManyAnimScriptAtPointers(PokemonRomReader reader, Pointer[] pointers, ref List<BattleAnimScript> output)
        {
            for (int i = 0; i < pointers.Length; i++)
            {
                reader.GoToPointer(pointers[i]);
                output.Add(ReadAnimScript(reader));
            }
        }
    }
}
