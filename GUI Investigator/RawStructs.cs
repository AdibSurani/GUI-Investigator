using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;

namespace GUI_Investigator
{
    [StructLayout(LayoutKind.Sequential)]
    class Header
    {
        public int magic = 0x495547; // "GUI"
        public int unk0 = 0x22715;
        public int filesize;
        public byte flag0; // 0 or 2 -- needs more investigation
        public byte flag1; // 0 or 2 -- needs more investigation
        public short zero0; // always 0
        public int lastModified;
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
        public int table24size; // in bytes, 48 * number of entries
        public int otherFlags; // not sure? looks like flags

        public int width;
        public int height;
        public long otherCount; // 0, 1 or 10 -- needs more investigation

        public long table0offset;
        public long table1offset;
        public long table2offset;
        public long table3offset;
        public long table4offset;
        public long table5offset;

        public long table7offset;
        public long table8offset;
        public long table9offset;
        public long table10offset;
        public long table11offset;
        public long table12offset;
        public long table13offset;
        public long table14offset;
        public long table15offset;
        public long table16offset;
        public long table17offset;
        public long table18offset;
        public long table19offset;
        public long table20offset;
        public long table21offset;
        public long table22offset;
        public long table23offset;

        public long zero3;

        public long dataStringOffset;
        public long table6offset;
        public long dataBoolOffset;
        public long data32bitOffset;
        public long dataRectOffset;
        public long dataMiscOffset;
        public long table5subcount; // table5count - table7count
        public long table24offset;

        // padding
        public long zero4;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry0
    {
        public int id;
        public short table2count;
        public short table1count;
        public short table2subcount; // counts the number of table2 entries satisfying some property
        public short table5count;
        public int table2start;
        public int strName;
        public int SOME_ARBITRARY_PADDING_0;
        public int table1start;
        public int SOME_ARBITRARY_PADDING_1;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry1
    {
        public int id;
        public int maxframes;
        public int strName;
        public int SOME_ARBITRARY_PADDING_0;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry2
    {
        public int id;
        public byte table4count;
        public byte table5count;
        public short zero;
        public int next;
        public int child;
        public int strName;
        public int SOME_ARBITRARY_PADDING_0;
        public int objTypeHash;
        public int SOME_ARBITRARY_PADDING_1;
        public int table4start;
        public int SOME_ARBITRARY_PADDING_2;
        public int table3start;
        public int SOME_ARBITRARY_PADDING_3;
        public int texture;
        public int SOME_ARBITRARY_PADDING_4;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry3
    {
        public short unk0;
        public byte table4count;
        public byte table5count;
        public short unk1;
        public short maxframes;
        public int table4start;
        public int SOME_ARBITRARY_PADDING_0;
        public int table5start;
        public int SOME_ARBITRARY_PADDING_1;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry4
    {
        public int SOME_ARBITRARY_PADDING_0;
        public int SOME_ARBITRARY_PADDING_1;
        public int dataType;
        public int zero;
        public int strProperty;
        public int SOME_ARBITRARY_PADDING_2;
        public int dataOffset;
        public int SOME_ARBITRARY_PADDING_3;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry5
    {
        public int SOME_ARBITRARY_PADDING_0;
        public int SOME_ARBITRARY_PADDING_1;
        public byte dataType;
        public byte count; // counts table6count as well as number of dataItems to take
        public short zero0;
        public int id; // can be automatically determined?
        public int strProperty;
        public int SOME_ARBITRARY_PADDING_2;
        public int table6start;
        public int SOME_ARBITRARY_PADDING_3;
        public int dataOffset;
        public int SOME_ARBITRARY_PADDING_4;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry6
    {
        public short frame;
        public byte zero;
        public byte frameType;
        public int SOME_ARBITRARY_PADDING_0;
        public int dataOffset;
        public int SOME_ARBITRARY_PADDING_1;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry7
    {
        public int id;
        public int zero;
        public int next;
        public int child;
        public int table4count;
        public int SOME_ARBITRARY_PADDING_0;
        public int strName;
        public int SOME_ARBITRARY_PADDING_1;
        public int instTypeHash;
        public int SOME_ARBITRARY_PADDING_2;
        public int table4start;
        public int SOME_ARBITRARY_PADDING_3;
        public int texture = -1;
        public int SOME_ARBITRARY_PADDING_4;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry8
    {
        public int id;
        public int type;
        public int zero;
        public int SOME_ARBITRARY_PADDING_0;
        public int strName;
        public int SOME_ARBITRARY_PADDING_1;
        public int table9entry;
        public int SOME_ARBITRARY_PADDING_2;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry9
    {
        public int unk0;
        public int maxframes;
        public int table5count;
        public int table5start;
        public int zero0;
        public int unk1;
        public int unk2;
        public int unk3;
        public int unk4;
        public int zero1;
        public int SOME_ARBITRARY_PADDING_0;
        public int SOME_ARBITRARY_PADDING_1;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry10
    { }

    [StructLayout(LayoutKind.Sequential)]
    class Entry11 // size 16
    {
        public int id;
        public int SOME_ARBITRARY_PADDING_0;
        public int SOME_ARBITRARY_PADDING_1;
        public int SOME_ARBITRARY_PADDING_2;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry12
    { }

    [StructLayout(LayoutKind.Sequential)]
    class Entry13
    { }

    [StructLayout(LayoutKind.Sequential)]
    class Entry14
    { }

    [StructLayout(LayoutKind.Sequential)]
    class Entry15 // size 12
    {
        public int id, zero, unk;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry16 // size 16
    {
        public int unk0, unk1, unk2, unk3;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry17 // size 40
    {
        public int id, zero1, strName, SOME_ARBITRARY_PADDING_0, varTypeHash, SOME_ARBITRARY_PADDING_1, zero4, zero5, id2, zero7;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry18 // different from android?
    {
        public int id;
        public int unk;
        public int SOME_ARBITRARY_PADDING_0;
        public short width;
        public short height;
        public int zero1;
        public int SOME_ARBITRARY_PADDING_1;
        public Vertex4D scale; // might be swapped other way round?
        public int SOME_ARBITRARY_PADDING_1b;
        public int SOME_ARBITRARY_PADDING_1c;
        public int strPath;
        public int SOME_ARBITRARY_PADDING_2;
        public int strName;
        public int SOME_ARBITRARY_PADDING_3;

    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry19 // size 24
    {
        public int zero0;
        public int SOME_ARBITRARY_PADDING_0;
        public int unk;
        public int SOME_ARBITRARY_PADDING_1;
        public int strPath;
        public int zero2;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry20 // size 20
    {
        public int fontFilterTypeHash;
        public int unk0;
        public int unk1;
        public int unk2;
        public int unk3;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry21
    { }

    [StructLayout(LayoutKind.Sequential)]
    class Entry22 // size 24
    {
        public int SOME_ARBITRARY_PADDING_0;
        public int SOME_ARBITRARY_PADDING_1;
        public int unk;
        public int zero;
        public int strPath;
        public int SOME_ARBITRARY_PADDING_2;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry23
    { }

    [StructLayout(LayoutKind.Sequential)]
    class Entry24
    {
        public Vertex4D dst, unk, src; // destination and source rectangles
    }
}
