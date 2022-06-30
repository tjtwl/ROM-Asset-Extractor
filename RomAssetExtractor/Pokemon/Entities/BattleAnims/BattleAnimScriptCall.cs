using Newtonsoft.Json;
using System;

namespace RomAssetExtractor.Pokemon.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BattleAnimScriptCall
    {
        [JsonProperty]
        public BattleAnimScriptCommand Command;
        [JsonProperty]
        public object[] Arguments;
        public BattleAnimScriptCommand.CommandByte CommandByte => Command.Byte;

        public BattleAnimScriptCall(BattleAnimScriptCommand command)
        {
            Command = command;
        }

        internal T GetArgument<T>(int argumentIndex)
        {
            return (T)Arguments[argumentIndex];
        }

        public override string ToString()
        {
            if (Arguments != null)
                return $"{Command.Name}({string.Join(", ", Arguments)})";

            return Command.Name;
        }
    }
}
