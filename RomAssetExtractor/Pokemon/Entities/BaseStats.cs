using RomAssetExtractor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Pokemon.Entities
{
    public enum GrowthRate
    {
        MediumFast = 0,
        Erratic = 1,
        Fluctuating = 2,
        MediumSlow = 3,
        Fast = 4,
        Slow = 5,
        MediumFastCopy1 = 6,
        MediumFastCopy2 = 7,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BaseStats
    {
        public byte BaseHP;
        public byte BaseAttack;
        public byte BaseDefense;
        public byte BaseSpeed;
        public byte BaseSpAttack;
        public byte BaseSpDefense;
        public byte Type1;
        public byte Type2;
        public byte CatchRate;
        public byte ExpYield;

        private ushort evYieldFlags;
        [BitField(2, nameof(evYieldFlags))] public ushort EvYield_HP => (byte)BitField.Parse(this);
        [BitField(2, nameof(evYieldFlags))] public byte EvYield_Attack => (byte)BitField.Parse(this);
        [BitField(2, nameof(evYieldFlags))] public byte EvYield_Defense => (byte)BitField.Parse(this);
        [BitField(2, nameof(evYieldFlags))] public byte EvYield_Speed => (byte)BitField.Parse(this);
        [BitField(2, nameof(evYieldFlags))] public byte EvYield_SpAttack => (byte)BitField.Parse(this);
        [BitField(2, nameof(evYieldFlags))] public byte EvYield_SpDefense => (byte)BitField.Parse(this);

        public ushort Item1;
        public ushort Item2;
        public byte GenderRatio;
        public byte EggCycles;
        public byte Friendship;
        internal byte growthRate;
        public GrowthRate GrowthRate => (GrowthRate)growthRate;
        public byte EggGroup1;
        public byte EggGroup2;
        public byte Ability1;
        public byte Ability2;
        public byte SafariZoneFleeRate;

        private byte appearanceFlags;
        [BitField(7, nameof(appearanceFlags))] public byte BodyColor => (byte)BitField.Parse(this);
        [BitField(1, nameof(appearanceFlags))] public byte NoFlip => (byte)BitField.Parse(this);

        private ushort _; // 00 00 padding
    };
}
