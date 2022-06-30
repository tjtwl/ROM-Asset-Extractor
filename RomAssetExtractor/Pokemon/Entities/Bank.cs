using Newtonsoft.Json;
using RomAssetExtractor.GbaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Pokemon.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BankModel
    {
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public int MapCount { get; set; }
        [JsonProperty]
        public Map[] Maps { get; set; }
    }

    public class Bank : BankModel
    {
        internal Pointer[] MapPointers { get; private set; }
        internal Pointer Pointer { get; private set; }

        private int currentMapIndex = 0;

        public Bank(int id, Pointer pointer)
        {
            Id = id;
            Pointer = pointer;
        }

        public void SetMapPointers(Pointer[] mapPointers)
        {
            Maps = new Map[mapPointers.Length];
            MapPointers = mapPointers;
        }

        public void AddMap(Map map)
        {
            Maps[currentMapIndex++] = map;
        }

        public override string ToString()
        {
            return $"{nameof(Bank)}({Id})";
        }
    }
}
