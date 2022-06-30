using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using System.Drawing.Imaging;
using RomAssetExtractor.Pokemon;
using RomAssetExtractor.Pokemon.Entities;

namespace RomAssetExtractor.GbaSystem
{
    public class RomReader : IDisposable
    {
        public const int INT24_SIZE = 3;

        public int Position => (int)binaryReader.BaseStream.Position;
        public int NormalizedPosition => NormalizeOffset(Position);
        public int Length => (int)binaryReader.BaseStream.Length;

        protected BinaryReader binaryReader;

        public RomReader(FileStream romFileStream)
        {
            binaryReader = new BinaryReader(romFileStream);
        }

        public void Dispose()
        {
            binaryReader.Close();
            binaryReader.Dispose();
        }

        public long GetOffset()
            => binaryReader.BaseStream.Position;

        public static int NormalizeOffset(int offset)
        {
            return offset & 0x1FFFFFF;
        }

        public virtual int GoToOffset(int offset)
            => (int)binaryReader.BaseStream.Seek(NormalizeOffset(offset), SeekOrigin.Begin);

        public virtual int GoToPointer(Pointer pointer)
            => GoToOffset(NormalizeOffset(pointer.TargetOffset));

        // Doesn't affect position in reader
        public byte PeekByte()
        {
            var b = ReadByte();
            Skip(-1);
            return b;
        }

        // Doesn't affect position in reader
        public virtual Pointer PeekPointer()
        {
            var pointer = ReadPointer();
            Skip(-Pointer.GetSize());
            return pointer;
        }

        public unsafe bool PeekIsPointer()
        {
            var pointer = ReadBytes(Pointer.GetSize());
            Skip(-Pointer.GetSize());
            return pointer[Pointer.GetSize() - 1] == 0x08;
        }

        internal void Skip(int offset)
            => binaryReader.BaseStream.Seek((long)offset, SeekOrigin.Current);

        internal short ReadShort()
            => BitConverter.ToInt16(binaryReader.ReadBytes(GetSizeOf<short>()), 0);

        internal ushort ReadUShort()
            => BitConverter.ToUInt16(binaryReader.ReadBytes(GetSizeOf<ushort>()), 0);

        internal int ReadInt()
            => BitConverter.ToInt32(binaryReader.ReadBytes(GetSizeOf<int>()), 0);

        internal uint ReadUInt()
            => BitConverter.ToUInt32(binaryReader.ReadBytes(GetSizeOf<uint>()), 0);

        internal int ReadInt24()
        {
            var bytes = binaryReader.ReadBytes(INT24_SIZE);
            return bytes[0] | bytes[1] << 8 | (sbyte)bytes[2] << 16;
        }

        internal uint ReadUInt24()
        {
            var bytes = binaryReader.ReadBytes(INT24_SIZE);
            return (uint)(bytes[0] | bytes[1] << 8 | bytes[2] << 16);
        }

        public bool ReadBool()
            => ReadByte() == 0x01;

        public byte ReadByte()
            => binaryReader.ReadBytes(1)[0];

        public byte[] ReadBytes(int byteCount)
            => binaryReader.ReadBytes(byteCount);

        public byte[] ReadBytesWithPadding(int byteCount, int size)
        {
            var result = new byte[size];
            Array.Copy(ReadBytes(byteCount), result, byteCount);
            return result;
        }

        public int ReadBytesAsInt(int byteCount)
            => BitConverter.ToInt32(ReadBytesWithPadding(byteCount, GetSizeOf<int>()), 0);

        public string ReadString(int byteCount)
            => Encoding.ASCII.GetString(ReadBytes(byteCount));

        public unsafe virtual int GetSizeOf<T>(int count = 1) where T : unmanaged
        {
            return count * sizeof(T);
        }

        public virtual unsafe Pointer ReadPointer()
        {
            return Read<Pointer>();
        }

        public virtual unsafe Pointer[] ReadPointers(int count)
        {
            return ReadMany<Pointer>(count);
        }

        public unsafe T Peek<T>() where T : unmanaged
        {
            var itemSize = sizeof(T);
            var data = ReadBytes(itemSize);
            Skip(-itemSize);

            fixed (byte* cdata = data)
            {
                T* pointers = (T*)cdata;
                return pointers[0];
            }
        }

        public unsafe T Read<T>() where T : unmanaged
        {
            var itemSize = sizeof(T);
            var data = ReadBytes(itemSize);

            fixed (byte* cdata = data)
            {
                T* pointers = (T*)cdata;
                return pointers[0];
            }
        }

        public unsafe T[] ReadMany<T>(int count) where T : unmanaged
        {
            var data = ReadBytes(GetSizeOf<T>(count));
            var countNotNull = count;

            fixed (byte* cdata = data)
            {
                T* pointers = (T*)cdata;
                var references = new T[count];

                for (int i = 0; i < count; i++)
                {
                    references[i] = pointers[i];
                }

                return references;
            }
        }

        public unsafe T[] ReadManyNotNull<T>(int count) where T : unmanaged
        {
            var individualSize = GetSizeOf<T>();
            var data = ReadBytes(individualSize * count);
            var countNotNull = count;
            var nullIndexes = new bool[count];

            fixed (byte* cdata = data)
            {
                T* pointers = (T*)cdata;
                var references = new T[count];

                for (int i = 0; i < count; i++)
                {
                    references[i] = pointers[i];

                    var sum = 0;
                    for (int j = 0; j < individualSize; j++)
                    {
                        sum += data[(i * individualSize) + j];
                    }

                    if(sum == 0)
                    {
                        countNotNull--;
                        nullIndexes[i] = true;
                    }
                }

                if(countNotNull == count)
                    return references;

                var resizedArray = new T[countNotNull];
                var index = 0;

                for (int i = 0; i < nullIndexes.Length; i++)
                {
                    if (!nullIndexes[i])
                        resizedArray[index++] = references[i];
                }

                return resizedArray;
            }
        }

        public byte[] ReadCompressedData()
        {
            return Lz77.Decompress(binaryReader);
        }
    }
}
