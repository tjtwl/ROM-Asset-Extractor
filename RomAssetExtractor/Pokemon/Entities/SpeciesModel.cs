using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Pokemon.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SpeciesModel
    {
        [JsonProperty]
        public short Id { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public BaseStats BaseStats { get; set; }
        [JsonProperty]
        public string Tag { get; set; }
        [JsonProperty]
        public Evolution[] Evolutions { get; set; }
        [JsonProperty]
        public uint[] ExperienceSpeeds { get; set; } // Level 0 - 100

        [JsonProperty]
        public string IconPath;
        [JsonProperty]
        public string FrontPath;
        [JsonProperty]
        public string FrontShinyPath;
        [JsonProperty]
        public string BackPath;
        [JsonProperty]
        public string BackShinyPath;

        public SpeciesModel(short gameIndex, string name)
        {
            Id = gameIndex;
            Name = name;
        }
    }
}
