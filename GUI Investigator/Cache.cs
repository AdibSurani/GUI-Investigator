using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GUI_Investigator
{
    class Cache<T>
    {
        Dictionary<T, int> dic;
        Func<T, byte[]> func;
        MemoryStream ms = new MemoryStream();
        public int hax { get; set; } // this ensures a byte-for-byte match with the original

        public Cache(Func<T, byte[]> func_)
        {
            dic = new Dictionary<T, int>();
            func = func_;
        }

        public int this[T key]
        {
            get
            {
                if (dic.TryGetValue(key, out int offset)) return offset;

                if (hax > 0 && dic.Any())
                {
                    var pair = dic.Last();
                    var last = Convert.FromBase64String((string)(object)pair.Key);
                    var curr = Convert.FromBase64String((string)(object)key);
                    if (last.Length <= hax && curr.Take(last.Length).SequenceEqual(last))
                    {
                        ms.Write(curr, last.Length, curr.Length - last.Length);
                        dic.Add(key, pair.Value);
                        return pair.Value;
                    }
                }

                offset = (int)ms.Length;
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
                while (ms.Length % 16 != 0) ms.WriteByte(0);
                return ms.ToArray();
            }
        }
    }
}
