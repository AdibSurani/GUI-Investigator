using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using static System.Linq.Enumerable;

namespace GUI_Investigator
{
    [StructLayout(LayoutKind.Sequential)]
    class Header
    {
        public int magic; // "GUI"
        public int unk0; // 0x21F13
        public int filesize;
        public byte flag0; // 0 or 2 -- needs more investigation
        public byte flag1; // 0 or 2 -- needs more investigation
        public short zero0; // always 0
        public int filenameHash;
        public int zero1; // always 0

        public int somecount0;
        public int somecount1;
        public int somecount2;
        public int somecount3;

        public int table0count;
        public int table1count;
        public int table2count;
        public int table3count;
        public int table4count;
        public int table5count;
        public int table6count;
        public int table7count;
        public int table8count;
        public int table9count;
        public int table10count; // always zero
        public int table11count;
        public int table12count; // always zero
        public int table13count; // always zero
        public int table14count; // always zero
        public int table15count;
        public int table16count;
        public int table17count;
        public int table18count;
        public int table19count;
        public int table20count;
        public int table21count; // always zero
        public int table22count;
        public int table23count; // always zero

        public int zero2;
        public int table17count2; // same as table17count
        public int tableBsize; // in bytes, 52 * number of entries
        public int otherFlags; // not sure? looks like flags

        public int width; // always 400
        public int height; // always 240
        public int otherCount; // 0, 1 or 10 -- needs more investigation

        public int table0offset;
        public int table1offset;
        public int table2offset;
        public int table3offset;
        public int table4offset;
        public int table5offset;

        public int table7offset;
        public int table8offset;
        public int table9offset;
        public int table10offset;
        public int table11offset;
        public int table12offset;
        public int table13offset;
        public int table14offset;
        public int table15offset;
        public int table16offset;
        public int table17offset;
        public int table18offset;
        public int table19offset;
        public int table20offset;
        public int table21offset;
        public int table22offset;
        public int table23offset;

        public int zero3;

        public int dataStringOffset; // the actual .gui throws 
        public int table6offset;
        public int dataBoolOffset;
        public int data32bitOffset;
        public int dataRectOffset;
        public int dataRectArrayOffset;
        public int table5subcount; // table5count - table7count
        public int tableBoffset;
        // table23, table6, dataBool, data32bit, dataRect, dataRectArray, dataString, tableB

        public void Test(int expectedFileSize)
        {
            var ex = new Exception();
            if (magic != 0x495547) throw ex;
            if (unk0 != 0x21F13) throw ex;
            if (filesize != expectedFileSize) throw ex;
            if (flag0 != 0 && flag0 != 2) throw ex;
            if (flag1 != 0 && flag1 != 2) throw ex;
            if (zero0 != 0) throw ex;
            if (zero1 != 0) throw ex;
            if (zero2 != 0) throw ex;
            if (zero3 != 0) throw ex;
            if (width != 400) throw ex;
            if (height != 240) throw ex;
            if (table5subcount != table5count - table7count) throw ex;

            // 25 | 26 is where it cuts off
            //int[] offsetOrder = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 25, 26, 27, 28, 29, 24, 31 };
            //var offsetList = offsetOrder.Select(n => offsets[n]).ToList();
            //offsetList.Insert(0, 0);
            //offsetList.Add(filesize);
            //if (!IsIncreasing(offsetList)) throw ex;
            //var sizeList = offsetList.Skip(1).Zip(offsetList, (a, b) => a - b).ToList();

            //int[] entryCountOrder = {
            //    4, 5, 6, 7, 8, 9,
            //    11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27,
            //    10
            //};
            //var entryCountList = entryCountOrder.Select(n => counts[n]).ToList();

            //int[] entrySizes = {
            //    24, 12, 36, 16, 16, 24,
            //    36, 20, 36, 0, 16, 0, 0, 0, 16, 16, 32, 48, 16, 20, 0, 16, 0,
            //    8
            //};

            //// off: 0-5, 6-22,  25,   [datas] 26-29, 24,    31    // offsets[23] = 0, offsets[30] is unknown, not an offset
            //// cnt: 4-9, 11-27, 10,                         30    // what's the deal with 0,1,2,3,[28 is always zero],29,31?

            //for (int i = 0; i < 23; i++)
            //{
            //    int t = entryCountList[i] * entrySizes[i];
            //    t += 15;
            //    t &= ~15;
            //    if (t != sizeList[i + 1]) throw ex;
            //}
            //var fs = sizeList.Skip(1).Zip(entryCountList, (a, b) => a * 1.0 / b);
            ////var fs = sizeList.Skip(1).Zip(entryCountList, (a, b) => $"{a}/{b}");
            //Console.WriteLine(string.Join(",", fs));

            //Debug.WriteLine(string.Join("\t", sizeList.Skip(25).Take(5).Concat(new[] { 0, 1, 2, 3, 29, 31 }.Select(n => counts[n]))));
            //Debug.WriteLine(sizeList[28]);

            void TestSize<T>(int count, int length)
            {
                if (((count * Marshal.SizeOf<T>() + 15) & ~15) != length) throw new Exception();
            }

            TestSize<Entry0>(table0count, table1offset - table0offset);
            TestSize<Entry1>(table1count, table2offset - table1offset);
            TestSize<Entry2>(table2count, table3offset - table2offset);
            TestSize<Entry3>(table3count, table4offset - table3offset);
            TestSize<Entry4>(table4count, table5offset - table4offset);
            TestSize<Entry5>(table5count, table7offset - table5offset); // note weird item
            TestSize<Entry6>(table6count, dataBoolOffset - table6offset); // note weird item
            TestSize<Entry7>(table7count, table8offset - table7offset);
            TestSize<Entry8>(table8count, table9offset - table8offset);
            TestSize<Entry9>(table9count, table10offset - table9offset);
            TestSize<Entry10>(table10count, table11offset - table10offset);
            TestSize<Entry11>(table11count, table12offset - table11offset);
            TestSize<Entry12>(table12count, table13offset - table12offset);
            TestSize<Entry13>(table13count, table14offset - table13offset);
            TestSize<Entry14>(table14count, table15offset - table14offset);
            TestSize<Entry15>(table15count, table16offset - table15offset);
            TestSize<Entry16>(table16count, table17offset - table16offset);
            TestSize<Entry17>(table17count, table18offset - table17offset);
            TestSize<Entry18>(table18count, table19offset - table18offset);
            TestSize<Entry19>(table19count, table20offset - table19offset);
            TestSize<Entry20>(table20count, table21offset - table20offset);
            TestSize<Entry21>(table21count, table22offset - table21offset);
            TestSize<Entry22>(table22count, table23offset - table22offset);
            TestSize<Entry23>(table23count, table6offset - table23offset); // note weird item
        }

        //static bool IsIncreasing(IEnumerable<int> src)
        //{
        //    int tmp = int.MinValue;
        //    foreach (int n in src)
        //    {
        //        if (n < tmp) return false;
        //        tmp = n;
        //    }
        //    return true;
        //}
    }
}
