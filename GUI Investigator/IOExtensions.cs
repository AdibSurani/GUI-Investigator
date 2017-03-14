using System;
using System.IO;
using System.Runtime.InteropServices;

namespace GUI_Investigator
{
    static class IOExtensions
    {
        public static unsafe T ReadStruct<T>(this BinaryReader br)
        {
            return br.ReadBytes(Marshal.SizeOf<T>()).ToStruct<T>();
        }

        public static unsafe T ToStruct<T>(this byte[] bytes, int baseOffset = 0, int itemOffset = 0)
        {
            fixed (byte* pBuffer = bytes)
            {
                return Marshal.PtrToStructure<T>((IntPtr)(pBuffer + baseOffset + itemOffset * Marshal.SizeOf<T>()));
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
    }
}