using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RomAssetExtractor.GbaSystem
{
    public static class Lz77
    {
        public static byte[] Decompress(BinaryReader reader)
        {
            var compressionVersion = reader.ReadByte();
            var decompressedSize = 0;

            for (int i = 0; i < 3; i++)
                decompressedSize |= reader.ReadByte() << (i * 8);

            if (decompressedSize == 0)
                throw new NotImplementedException("[Lz77 Decompression Failed] Invalid size for decompressed data!");

            if (compressionVersion == 0x10)
                return DecompressVersion10(reader, (uint)decompressedSize);

            throw new ArgumentException("[Lz77 Decompression Failed] Could not find a LZ77 compressed file at current reading position.");
        }

        private static int ReadShort(BinaryReader reader)
        {
            // Big-endian
            var num = reader.ReadBytes(2);

            return (num[0] << 8) | num[1];
        }

        private static void WriteByte(IntPtr data, ref int offset, uint size, byte byteToWrite)
        {
            if (size <= offset)
                throw new Exception("decompressed data is larger than expected");

            Marshal.WriteByte(data, offset, byteToWrite);
            offset += 1;
        }

        private static void CopyByte(BinaryReader reader, IntPtr data, ref int offset, uint size)
        {
            WriteByte(data, ref offset, size, reader.ReadByte());
        }

        private static byte[] DecompressVersion10(BinaryReader reader, uint size)
        {
            var data = Marshal.AllocHGlobal((int)size);
            var pos = 0;
            int flag, b, sh, count, disp;
            byte[] decompressedData = new byte[size];

            try
            {

                while (pos < size)
                {
                    b = reader.ReadByte();

                    for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                    {
                        flag = b & 0x80;

                        if (flag == 0)
                            CopyByte(reader, data, ref pos, size);
                        else
                        {
                            sh = ReadShort(reader);
                            count = (sh >> 0xc) + 3;
                            disp = (sh & 0xfff) + 1;

                            for (int j = 0; j < count; j++)
                            {
                                if (pos < disp)
                                {
                                    // Some maps are broken like this! E.g: 6.2 is non - functional and unused in the final game anyway
                                    throw new DataMisalignedException($"Disp ({disp}) out of range (pos = {pos})!");
                                }

                                var readBytes = Marshal.ReadByte(data, pos - disp);
                                WriteByte(data, ref pos, size, readBytes);

                                if (size <= pos)
                                    break;
                            }
                        }

                        b <<= 1;

                        if (size <= pos)
                            break;
                    }
                }

                if (pos != size)
                    throw new DataMisalignedException("Decompressed size does not match the expected size");
            }
            catch(DataMisalignedException ex)
            {
                global::System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                Marshal.Copy(data, decompressedData, 0, (int)size);

                Marshal.FreeHGlobal(data);
            }

            return decompressedData;
        }
    }
}
