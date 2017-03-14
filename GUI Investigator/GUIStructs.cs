using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace GUI_Investigator
{
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

    [StructLayout(LayoutKind.Sequential)]
    struct Rectangle // Note: This can also be a colour. The -0 hack is required to ensure a byte-for-byte match
    {
        public float X0, Y0, X1, Y1;
        public static string f(float f) => BitConverter.ToInt32(BitConverter.GetBytes(f), 0) == int.MinValue ? "-0" : f.ToString("G9");
        public static float f(string s) => s == "-0" ? BitConverter.ToSingle(BitConverter.GetBytes(int.MinValue), 0) : float.Parse(s);
        public override string ToString() => $"{f(X0)},{f(Y0)},{f(X1)},{f(Y1)}";
        public static Rectangle Parse(string s)
        {
            var fs = s.Split(',').Select(f).ToList();
            return new Rectangle { X0 = fs[0], Y0 = fs[1], X1 = fs[2], Y1 = fs[3] };
        }
    }

    //class RectArray : List<Rectangle>
    //{
    //    public int offset;
    //    public RectArray(int offset_, IEnumerable<Rectangle> src)
    //    {
    //        //offset = offset_;
    //        AddRange(src);
    //    }
    //}
}
