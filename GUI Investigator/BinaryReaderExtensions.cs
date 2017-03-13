using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace GUI_Investigator
{
    static class BinaryReaderExtensions
    {
        public static unsafe T ReadStruct<T>(this BinaryReader br)
        {
            return br.ReadBytes(Marshal.SizeOf<T>()).ToStruct<T>();
        }

        public static unsafe T ToStruct<T>(this byte[] bytes, int offset = 0)
        {
            fixed (byte* pBuffer = bytes)
            {
                return Marshal.PtrToStructure<T>((IntPtr)(pBuffer + offset));
            }
        }

        public static unsafe byte[] StructToArray<T>(this T item)
        {
            var buffer = new byte[Marshal.SizeOf(typeof(T))];
            fixed (byte* pBuffer = buffer)
            {
                Marshal.StructureToPtr(item, (IntPtr)pBuffer, false);
            }
            return buffer;
        }

        public static bool Equal<T>(this T item1, T item2)
        {
            return item1.StructToArray().SequenceEqual(item2.StructToArray());
        }

        public static IEnumerable<T> ReadMultiple<T>(this BinaryReader br, int count)
        {
            while (count-- > 0) yield return br.ReadStruct<T>();
        }

        public static IEnumerable<T> ReadMultiple<T>(this BinaryReader br, int offset, int count)
        {
            var tmp = br.BaseStream.Position;
            br.BaseStream.Position = offset;
            while (count-- > 0) yield return br.ReadStruct<T>();
            br.BaseStream.Position = tmp;
        }

        public static bool TableEqual<T>(this List<T> item1, List<T> item2)
        {
            //return item1.Count == item2.Count && item1.Zip(item2, (x, y) => x.StructToArray().SequenceEqual(y.StructToArray())).All(b => b);
            if (item1.Count != item2.Count) return false;
            for (int i = 0; i < item1.Count; i++)
            {
                System.Diagnostics.Debug.Assert(item1[i].StructToArray().SequenceEqual(item2[i].StructToArray()));
                if (!item1[i].StructToArray().SequenceEqual(item2[i].StructToArray())) return false;
            }
            return true;
        }

        public static void WritePadded<T>(this BinaryWriter bw, T item)
        {
            bw.Write(item.StructToArray());
            while (bw.BaseStream.Position % 16 != 0) bw.BaseStream.WriteByte(0);
        }

        public static void WritePadded<T>(this BinaryWriter bw, List<T> table)
        {
            foreach (var item in table) bw.Write(item.StructToArray());
            while (bw.BaseStream.Position % 16 != 0) bw.BaseStream.WriteByte(0);
        }
    }
}