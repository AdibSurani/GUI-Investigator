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
    class GUI
    {
        const string sep = "\t";
        static void MyPrint(object s)
        {
            //Debug.WriteLine(s);
        }

        public GUI(string filename, byte[] bytes)
        {
            T Read<T>(int baseOffset, int itemOffset = 0) => bytes.ToStruct<T>(baseOffset, itemOffset);
            List<T> ReadMultiple<T>(int offset, int count) => Range(0, count).Select(i => Read<T>(offset, i)).ToList();

            var header = Read<Header>(0);
            var table0 = ReadMultiple<Entry0>(header.table0offset, header.table0count);
            var table1 = ReadMultiple<Entry1>(header.table1offset, header.table1count);
            var table2 = ReadMultiple<Entry2>(header.table2offset, header.table2count);
            var table3 = ReadMultiple<Entry3>(header.table3offset, header.table3count);
            //var table4 = ReadMultiple<Entry4>(header.table4offset, header.table4count);
            //var table5 = ReadMultiple<Entry5>(header.table5offset, header.table5count);
            var table6 = ReadMultiple<Entry6>(header.table6offset, header.table6count);
            var table7 = ReadMultiple<Entry7>(header.table7offset, header.table7count);
            var table8 = ReadMultiple<Entry8>(header.table8offset, header.table8count);
            var table9 = ReadMultiple<Entry9>(header.table9offset, header.table9count);
            var table10 = ReadMultiple<Entry10>(header.table10offset, header.table10count);
            var table11 = ReadMultiple<Entry11>(header.table11offset, header.table11count);
            var table12 = ReadMultiple<Entry12>(header.table12offset, header.table12count);
            var table13 = ReadMultiple<Entry13>(header.table13offset, header.table13count);
            var table14 = ReadMultiple<Entry14>(header.table14offset, header.table14count);
            var table15 = ReadMultiple<Entry15>(header.table15offset, header.table15count);
            var table16 = ReadMultiple<Entry16>(header.table16offset, header.table16count);
            var table17 = ReadMultiple<Entry17>(header.table17offset, header.table17count);
            var table18 = ReadMultiple<Entry18>(header.table18offset, header.table18count);
            var table19 = ReadMultiple<Entry19>(header.table19offset, header.table19count);
            var table20 = ReadMultiple<Entry20>(header.table20offset, header.table20count);
            var table21 = ReadMultiple<Entry21>(header.table21offset, header.table21count);
            var table22 = ReadMultiple<Entry22>(header.table22offset, header.table22count);
            var table23 = ReadMultiple<Entry23>(header.table23offset, header.table23count);
            var table24 = ReadMultiple<Entry24>(header.table24offset, header.table24size / 52);

            var spl = Encoding.ASCII.GetString(bytes, header.dataStringOffset, header.table24offset - header.dataStringOffset).Split('\0');
            var dicString = spl.Select((str, i) => Tuple.Create(spl.Take(i).Sum(s => s.Length + 1), str)).ToDictionary(p => p.Item1, p => p.Item2);

            object GetData(int dataType, int dataOffset, int extraOffset = 0)
            {
                dataOffset += (dataType == 3 ? 1 : dataType == 4 ? 16 : 4) * extraOffset;
                switch (dataType)
                {
                    case 2: return Read<float>(header.data32bitOffset + dataOffset);
                    case 3: return Read<byte>(header.dataBoolOffset + dataOffset) == 1;
                    case 4: return Read<Rectangle>(header.dataRectOffset + dataOffset);
                    case 15: return "";
                    case 17: return dataOffset == 1;
                    case 18: return dataOffset;
                    case 32: return new[] { Read<int>(header.dataRectArrayOffset + dataOffset) };
                    case 33:
                        return Range(0, Read<int>(header.dataRectArrayOffset + dataOffset)).Select(i =>
                            Read<Rectangle>(header.dataRectArrayOffset + dataOffset + 8, i)).ToList();
                    default: return Read<int>(header.data32bitOffset + dataOffset);
                }
            };

            XElement GetProperty(int n4)
            {
                var e4 = Read<Entry4>(header.table4offset, n4);
                return new XElement("property",
                    new XAttribute("datatype", e4.dataType),
                    new XAttribute("name", dicString[e4.strProperty]),
                    new XAttribute("value", GetData(e4.dataType, e4.dataOffset)));
            }

            XElement GetAnimatedProperty(int n5)
            {
                var e5 = Read<Entry5>(header.table5offset, n5);
                return new XElement("animatedproperty",
                    new XAttribute("id", e5.id), // can be discovered
                    new XAttribute("datatype", e5.dataType),
                    new XAttribute("name", dicString[e5.strProperty]),
                    from i in Range(0, e5.count)
                    let e6 = table6[e5.table6start + i]
                    select new XElement("change",
                        new XAttribute("frame", e6.frame),
                        new XAttribute("frameType", e6.frameType),
                        new XAttribute("value", GetData(e5.dataType, e5.dataOffset, i))
                        ));
            }

            XElement PrintTable<T>(List<T> table, string tableName)
            {
                if (!table.Any()) return null;
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine(string.Join(sep, typeof(T).GetFields().Select(f => f.Name).Where(name => !name.StartsWith("zero"))));
                foreach (var e in table)
                {
                    var vals = from field in typeof(T).GetFields()
                               let name = field.Name
                               let value = field.GetValue(e)
                               where !name.StartsWith("zero")
                               select name.StartsWith("str") ? (int)value == -1 ? "(null)" : dicString[(int)value]
                                   : name.EndsWith("Hash") ? $"{value:X8}" : $"{value}";
                    sb.AppendLine(string.Join(sep, vals));
                }
                return new XElement("block", new XAttribute("name", tableName), new XAttribute("count", table.Count), sb.ToString());
            }

            // All the cool printing stuff goes here!
            MyPrint(new XElement("gui",
                new XAttribute("filename", filename),
                new XAttribute("id", header.filenameHash.ToString("X8")),
                new XAttribute("flag0", header.flag0),
                new XAttribute("flag1", header.flag1),
                new XAttribute("otherflags", header.otherFlags),
                new XAttribute("somecounts", string.Join(",", header.somecount0, header.somecount1, header.somecount2, header.somecount3)),
                new XAttribute("othercount", header.otherCount),
                PrintTable(table0, nameof(table0)),
                PrintTable(table1, nameof(table1)),
                PrintTable(table2, nameof(table2)),
                PrintTable(table3, nameof(table3)),
                //PrintTable(table4, nameof(table4)),
                //PrintTable(table5, nameof(table5)),
                PrintTable(table6, nameof(table6)),
                PrintTable(table7, nameof(table7)),
                PrintTable(table8, nameof(table8)),
                PrintTable(table9, nameof(table9)),
                PrintTable(table10, nameof(table10)),
                PrintTable(table11, nameof(table11)),
                PrintTable(table12, nameof(table12)),
                PrintTable(table13, nameof(table13)),
                PrintTable(table14, nameof(table14)),
                PrintTable(table15, nameof(table15)),
                PrintTable(table16, nameof(table16)),
                PrintTable(table17, nameof(table17)),
                PrintTable(table18, nameof(table18)),
                PrintTable(table19, nameof(table19)),
                PrintTable(table20, nameof(table20)),
                PrintTable(table21, nameof(table21)),
                PrintTable(table22, nameof(table22)),
                PrintTable(table23, nameof(table23)),
                PrintTable(table24, nameof(table24))));

            ////////////////////////
            // animgroups xml
            var anims = from e0 in table0
                        select new XElement("anim",
                            new XAttribute("id", e0.id),
                            new XAttribute("name", dicString[e0.strName]),
                            new XAttribute("panesubcount", e0.table2subcount), // can be discovered?
                            from n1 in Range(e0.table1start, e0.table1count)
                            let e1 = table1[n1]
                            select new XElement("sequence",
                                new XAttribute("id", e1.id),
                                new XAttribute("maxframes", e1.maxframes), // can be discovered
                                new XAttribute("name", dicString[e1.strName])),
                            from n2 in Range(e0.table2start, e0.table2count)
                            let e2 = table2[n2]
                            let e2tex = e2.texture == -1 || e2.tagHash == 0x2787DB24 ? null : (List<Rectangle>)GetData(33, e2.texture)
                            let e2val = e2.tagHash != 0x2787DB24 ? null : (int[])GetData(32, e2.texture)
                            select new XElement("pane",
                                new XAttribute("id", e2.id),
                                new XAttribute("type", $"{e2.tagHash:X8}"),
                                new XAttribute("name", dicString[e2.strName]),
                                new XAttribute("next", e2.next),
                                new XAttribute("child", e2.child),
                                e2tex == null ? null :
                                    from ntex in Range(0, e2tex.Count)
                                    let r = e2tex[ntex] // can we also map this to e24?
                                                        //let e24 = table24[e2tex.offset + ntex] // this currently doesn't work
                                        select new XElement("map", new XAttribute("rect", r)),
                                e2val == null ? null :
                                    new XElement("something5",
                                        new XAttribute("value", e2val[0])),
                                Range(e2.table4start, e2.table4count).Select(GetProperty),
                                from index in Range(0, e0.table1count)
                                let e1 = table1[e0.table1start + index]
                                let e3 = table3[e2.table3start + index]
                                select new XElement("state",
                                    new XAttribute("sequencename", dicString[e1.strName]), // not an e3 property
                                    new XAttribute("maxframes", e3.maxframes), // can be discovered
                                    new XAttribute("unk0", e3.unk0),
                                    new XAttribute("unk1", e3.unk1),
                                    Range(e3.table4start, e3.table4count).Select(GetProperty),
                                    Range(e3.table5start, e3.table5count).Select(GetAnimatedProperty))));

            var events = from e8 in table8
                         let e9 = e8.type == 2 ? table9[e8.table9entry] : null
                         select new XElement("event",
                             new XAttribute("id", e8.id),
                             new XAttribute("type", e8.type),
                             new XAttribute("name", dicString[e8.strName]),
                             new XAttribute("t9entry", e8.table9entry),
                             e9 == null ? null : new XAttribute("e9unks", string.Join(",", e9.unk0, e9.unk1, e9.unk2, e9.unk3)),
                             e9 == null ? null : new XAttribute("maxframes", e9.maxframes), // can be discovered
                             e9 == null ? null : Range(e9.table5start, e9.table5count).Select(GetAnimatedProperty));

            // images xml
            var images = from n7 in Range(0, table7.Count)
                         let e7 = table7[n7]
                         select new XElement("pane",
                             new XAttribute("id", e7.id),
                             new XAttribute("type", $"{e7.tagHash:X8}"),
                             new XAttribute("name", dicString[e7.strName]),
                             new XAttribute("next", e7.next),
                             new XAttribute("child", e7.child),
                             Range(e7.table4start, e7.table4count).Select(GetProperty),
                             GetAnimatedProperty(header.table5subcount + n7));

            var misc = new XElement("misc",
                from e11 in table11
                select new XElement("e11",
                    new XAttribute("id", e11.id)),
                from e15 in table15
                select new XElement("e15",
                    new XAttribute("id", e15.id),
                    new XAttribute("unk", e15.unk)),
                from e16 in table16
                select new XElement("e16",
                    new XAttribute("unks", string.Join(",", e16.unk0, e16.unk1, e16.unk2, e16.unk3))),
                from e17 in table17
                select new XElement("e17",
                    new XAttribute("id", e17.id),
                    new XAttribute("name", dicString[e17.strName]),
                    new XAttribute("varHash", e17.varHash.ToString("X8")),
                    new XAttribute("id2", e17.id2)),
                from e18 in table18
                select new XElement("e18",
                       new XAttribute("id", e18.id),
                       new XAttribute("width", e18.width),
                       new XAttribute("height", e18.height),
                       new XAttribute("sclX", e18.scaleX),
                       new XAttribute("sclY", e18.scaleY),
                       new XAttribute("name", dicString[e18.strName]),
                       e18.strPath == -1 ? null : new XAttribute("path", dicString[e18.strPath])),
                from e19 in table19
                select new XElement("e19",
                    new XAttribute("path", dicString[e19.strPath])),
                from e20 in table20
                select new XElement("e20",
                    new XAttribute("unkHash", e20.unkHash.ToString("X8")),
                    new XAttribute("unks", string.Join(",", e20.unk0, e20.unk1, e20.unk2, e20.unk3))),
                from e22 in table22
                select new XElement("e22",
                    new XAttribute("unk", e22.unk),
                    new XAttribute("path", dicString[e22.strPath])),
                from e24 in table24
                select new XElement("e24",
                    new XAttribute("dst", e24.dst),
                    new XAttribute("src", e24.src)));

            var gui = new XElement("gui",
                new XAttribute("filename", filename),
                new XAttribute("id", header.filenameHash.ToString("X8")),
                new XAttribute("flag0", header.flag0),
                new XAttribute("flag1", header.flag1),
                new XAttribute("otherflags", header.otherFlags),
                new XAttribute("somecounts", string.Join(",", header.somecount0, header.somecount1, header.somecount2, header.somecount3)),
                new XAttribute("othercount", header.otherCount),
                anims, images, events, misc);

            MyPrint(gui.ToString().Replace("  ", "\t"));

            Debug.WriteLine($"{header.filenameHash:X8}\t{filename}");

            var recon = new Reconstruction(gui);
            Debug.Assert(recon.table0.TableEqual(table0));
            Debug.Assert(recon.table1.TableEqual(table1));
            Debug.Assert(recon.table2.TableEqual(table2));
            Debug.Assert(recon.table3.TableEqual(table3));
            //Debug.Assert(recon.table4.TableEqual(table4));
            //Debug.Assert(recon.table5.TableEqual(table5));
            Debug.Assert(recon.table6.TableEqual(table6));
            Debug.Assert(recon.table7.TableEqual(table7));
            Debug.Assert(recon.table8.TableEqual(table8));
            Debug.Assert(recon.table9.TableEqual(table9));
            Debug.Assert(recon.table10.TableEqual(table10));
            Debug.Assert(recon.table11.TableEqual(table11));
            Debug.Assert(recon.table12.TableEqual(table12));
            Debug.Assert(recon.table13.TableEqual(table13));
            Debug.Assert(recon.table14.TableEqual(table14));
            Debug.Assert(recon.table15.TableEqual(table15));
            Debug.Assert(recon.table16.TableEqual(table16));
            Debug.Assert(recon.table17.TableEqual(table17));
            Debug.Assert(recon.table18.TableEqual(table18));
            Debug.Assert(recon.table19.TableEqual(table19));
            Debug.Assert(recon.table20.TableEqual(table20));
            Debug.Assert(recon.table21.TableEqual(table21));
            Debug.Assert(recon.table22.TableEqual(table22));
            Debug.Assert(recon.table23.TableEqual(table23));
            Debug.Assert(recon.table24.TableEqual(table24));
            Debug.Assert(recon.filedata.SequenceEqual(bytes));
        }

    }
}
