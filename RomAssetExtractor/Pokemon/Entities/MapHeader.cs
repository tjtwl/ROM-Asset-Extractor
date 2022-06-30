using RomAssetExtractor.GbaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Pokemon.Entities
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MapHeader
    {
        public Pointer MapLayout;
        public Pointer Events;
        public Pointer MapScripts;
        public Pointer MapConnections;

        public ushort MusicId;
        public ushort MapLayoutIndex;
        public byte LabelIndex;

        public byte VisibilityType;
        public byte WeatherType;

        private byte mapType;
        public MapType MapType => (MapType)mapType;

        public byte _; // padding 00
        public byte EscapeRope;
        public byte Flags;
        private byte battleType;
        public MapBattleScene BattleType => (MapBattleScene)battleType;
    }
}
