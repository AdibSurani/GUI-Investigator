using System;
using System.Collections.Generic;
using System.IO;

namespace GUI_Investigator
{
    class Cache<T>
    {
        Dictionary<T, int> dic = new Dictionary<T, int>();
        Func<T, byte[]> func;
        MemoryStream ms = new MemoryStream();
        public int Length => (int)ms.Length;

        public Cache(Func<T, byte[]> func_) => func = func_;

        public int this[T key]
        {
            get
            {
                if (key is null) return -1;
                if (dic.TryGetValue(key, out int offset)) return offset;
                offset = Length;
                dic.Add(key, offset);
                var bytes = func(key);
                ms.Write(bytes, 0, bytes.Length);
                return offset;
            }
        }

        public byte[] Data
        {
            get
            {
                while (Length % 16 != 0) ms.WriteByte(0);
                return ms.ToArray();
            }
        }
    }
}
