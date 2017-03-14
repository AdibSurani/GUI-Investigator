using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;

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
        public int dataMiscOffset;
        public int table5subcount; // table5count - table7count
        public int table24offset;

        // padding
        public int zero4;
        public int zero5;
        public int zero6;
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
        public int table1start;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry1
    {
        public int id;
        public int maxframes;
        public int strName;
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
        public int tagHash;
        public int table4start;
        public int table3start;
        public int texture;
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
        public int table5start;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry4
    {
        public int dataType;
        public int zero;
        public int strProperty;
        public int dataOffset;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry5
    {
        public byte dataType;
        public byte count; // counts table6count as well as number of dataItems to take
        public short zero0;
        public int zero1;
        public int id; // can be automatically determined?
        public int strProperty;
        public int table6start;
        public int dataOffset;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry6
    {
        public short frame;
        public byte zero;
        public byte frameType;
        public int dataOffset;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry7
    {
        public int id;
        public int zero;
        public int next;
        public int child;
        public int table4count;
        public int strName;
        public int tagHash;
        public int table4start;
        public int texture = -1;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry8
    {
        public int id;
        public int type;
        public int zero;
        public int strName;
        public int table9entry;
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
        public int zero1;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry10
    { }

    [StructLayout(LayoutKind.Sequential)]
    class Entry11 // size 4
    {
        public int id;
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
    class Entry17 // size 32
    {
        public int id, zero1, strName, varHash, zero4, zero5, id2, zero7;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry18
    {
        public int id;
        public int zero0;
        public short width;
        public short height;
        public int zero1;
        public float scaleX;
        public float scaleY;
        public int strPath;
        public int strName;
        public Rectangle zero2;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry19 // size 16
    {
        public int zero0;
        public int zero1;
        public int strPath;
        public int zero2;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry20 // size 20
    {
        public int unkHash;
        public int unk0;
        public int unk1;
        public int unk2;
        public int unk3;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry21
    { }

    [StructLayout(LayoutKind.Sequential)]
    class Entry22 // size 12
    {
        public int unk;
        public int zero;
        public int strPath;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Entry23
    { }

    [StructLayout(LayoutKind.Sequential)]
    class Entry24
    {
        public Rectangle dst, zero0, src; // destination and source rectangles
        public int zero1;
    }
}
