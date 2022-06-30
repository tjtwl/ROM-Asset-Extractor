using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RomAssetExtractor.Utilities
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class BitField : Attribute
    {
        private readonly int size;
        private readonly int order;
        private readonly string flagFieldName;

        /// <summary>
        /// BitField attributes must be applied to properties in the order they occur in the bitfield
        /// </summary>
        /// <remarks>
        /// It is important you do NOT specify the <paramref name="_"/> parameter. It is automatically filled by CompilerServices to track the order of BitField attributes. That is later used to find all bitfields belonging to a property
        /// </remarks>
        /// <param name="size">Size in bits that this property takes up in the bitfield</param>
        /// <param name="flagFieldName">Which field holds this property amongst the other bitfields</param>
        /// <param name="_">Do not specify this!</param>
        public BitField(int size, string flagFieldName, [CallerLineNumber] int _ = 0)
        {
            this.size = size;
            this.order = _;
            this.flagFieldName = flagFieldName;
        }

        private static List<BitField> GetPrecedingBitFieldAttributes(Type instanceType, int accessorCallerLine = 0) 
        {
            // Find all bitfields ordered by their occurance in the given type
            var allBitFields = new SortedList<int, BitField>();
            
            foreach (var property in instanceType.GetProperties())
            {
                var attribute = property.GetCustomAttribute<BitField>(true);

                if (attribute != null)
                    allBitFields.Add(attribute.order, attribute);
            }

            var bitFields = new List<BitField>();
            string lastBitFieldFlag = null;

            // Ensure we get all bitfields that do not belong to other fields
            foreach (var bitField in allBitFields.Values)
            {
                if (bitField.order > accessorCallerLine)
                    break;

                if (lastBitFieldFlag != null && bitField.flagFieldName != lastBitFieldFlag)
                    bitFields.Clear();

                bitFields.Add(bitField);
                lastBitFieldFlag = bitField.flagFieldName;
            }

            return bitFields;
        }

        /// <summary>
        /// Call this from a property marked with [BitField].
        /// 
        /// Unmasks and unshift the value in the bitfield, based on information in the [BitFieldAttribute] directly in front of this property.
        /// 
        /// <example>
        /// <code>
        /// <![CDATA[
        ///     private ushort flags;
        ///     [BitField(9, nameof(flags))] public int X => BitField.Parse(this);
        ///     [BitField(5, nameof(flags))] public int MatrixNum => BitField.Parse(this);
        ///     [BitField(2, nameof(flags))] public int Size => BitField.Parse(this);
        /// ]]>
        /// </code>
        /// </example>
        /// </summary>
        /// <remarks>
        /// It is important you do NOT specify the <paramref name="_"/> parameter. It is automatically filled by CompilerServices to help find the BitField attribute belonging to the property from which this is called.
        /// </remarks>
        /// <param name="instance">The instance in which the bitfield is present</param>
        /// <param name="_">Do not specify this!</param>
        /// <returns>The value in the bitfield</returns>
        public static int Parse(object instance, [CallerLineNumber] int _ = 0)
        {
            var bitFieldAttributes = GetPrecedingBitFieldAttributes(instance.GetType(), _);
            int shift = 0;
            int mask = -1;

            for (int i = 0; i < bitFieldAttributes.Count; i++)
            {
                if (i > 0)
                    shift += bitFieldAttributes[i - 1].size;

                mask = ((1 << bitFieldAttributes[i].size) - 1) << shift;
            }

            if (mask == -1)
                throw new AccessViolationException("BitField.Parse can only work on accessors marked with [BitField(size)]");

            var type = instance.GetType();
            var field = type.GetField(bitFieldAttributes[0].flagFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            var bitFieldValue = field.GetValue(instance);
            return (Convert.ToInt32(bitFieldValue) & mask) >> shift;
        }
    }
}
