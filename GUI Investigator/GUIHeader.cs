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
        public int magic = 0x495547; // "GUI"
        public int unk0 = 0x21F13;
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
        public int table7count2; // same as table7count
        public int table24size; // in bytes, 52 * number of entries
        public int otherFlags; // not sure? looks like flags

        public int width = 400; // always 400
        public int height = 240; // always 240
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

        public int dataStringOffset;
        public int table6offset;
        public int dataBoolOffset;
        public int data32bitOffset;
        public int dataRectOffset;
        public int dataRectArrayOffset;
        public int table5subcount; // table5count - table7count
        public int table24offset;

        // padding
        public int zero4;
        public int zero5;
        public int zero6;
    }
}
