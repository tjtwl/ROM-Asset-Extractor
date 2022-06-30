using Newtonsoft.Json;
using System.Collections.Generic;

namespace RomAssetExtractor.Pokemon.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BattleAnimScriptModel
    {
        internal int Pointer { get; set; }

        [JsonProperty]
        public readonly List<BattleAnimScriptCall> ScriptCalls = new List<BattleAnimScriptCall>();

        [JsonProperty]
        public List<BattleAnimScriptModel> SubScripts { get; protected set; } = null;
    }
}
