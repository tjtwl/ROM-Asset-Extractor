using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using RomAssetExtractor.Utilities;
using System.Globalization;

namespace RomAssetExtractor.GbaSystem
{
    public struct Pointer : IYamlConvertible
    {
        public int TargetOffset;
        public int TargetOffsetNormalized => RomReader.NormalizeOffset(TargetOffset);

        public bool IsNull
            => TargetOffset == 0;

        public Pointer OffsetBy(int offset)
            => new Pointer() { TargetOffset = TargetOffset + offset };

        public int Distance(Pointer pointer)
            => Distance(pointer.TargetOffset);

        public int Distance(int offset)
            => TargetOffset - offset;

        public static unsafe int GetSize() => sizeof(Pointer);

        public override bool Equals(object obj)
            => TargetOffset.Equals(obj);

        public override int GetHashCode()
            => TargetOffset.GetHashCode();

        void IYamlConvertible.Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            if (!parser.TryConsume<Scalar>(out var scalar) 
                || !int.TryParse(scalar.Value.TrimStart("0x"), NumberStyles.HexNumber, null, out var scalarValue))
                throw new InvalidCastException("Not a valid pointer!!");

            TargetOffset = scalarValue;
        }

        void IYamlConvertible.Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(Pointer left, Pointer right)
            => left.TargetOffset == right.TargetOffset;

        public static bool operator !=(Pointer left, Pointer right)
            => left.TargetOffset != right.TargetOffset;

        public static bool operator >(Pointer left, Pointer right)
            => left.TargetOffset > right.TargetOffset;

        public static bool operator <(Pointer left, Pointer right)
            => left.TargetOffset < right.TargetOffset;

        public static bool operator >=(Pointer left, Pointer right)
            => left.TargetOffset >= right.TargetOffset;

        public static bool operator <=(Pointer left, Pointer right)
            => left.TargetOffset <= right.TargetOffset;
    }
}
