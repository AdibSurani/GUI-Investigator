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

        static int Pad(int n) => (n + 15) & ~15;

        public GUI(string filename, Stream stream)
        {
            using (var br = new BinaryReader(stream))
            {
                var header = br.ReadStruct<Header>();
                header.Test((int)stream.Length);

                var table0 = br.ReadMultiple<Entry0>(header.table0offset, header.table0count).ToList();
                var table1 = br.ReadMultiple<Entry1>(header.table1offset, header.table1count).ToList();
                var table2 = br.ReadMultiple<Entry2>(header.table2offset, header.table2count).ToList();
                var table3 = br.ReadMultiple<Entry3>(header.table3offset, header.table3count).ToList();
                var table4 = br.ReadMultiple<Entry4>(header.table4offset, header.table4count).ToList();
                var table5 = br.ReadMultiple<Entry5>(header.table5offset, header.table5count).ToList();
                var table6 = br.ReadMultiple<Entry6>(header.table6offset, header.table6count).ToList();
                var table7 = br.ReadMultiple<Entry7>(header.table7offset, header.table7count).ToList();
                var table8 = br.ReadMultiple<Entry8>(header.table8offset, header.table8count).ToList();
                var table9 = br.ReadMultiple<Entry9>(header.table9offset, header.table9count).ToList();
                var table10 = br.ReadMultiple<Entry10>(header.table10offset, header.table10count).ToList();
                var table11 = br.ReadMultiple<Entry11>(header.table11offset, header.table11count).ToList();
                var table12 = br.ReadMultiple<Entry12>(header.table12offset, header.table12count).ToList();
                var table13 = br.ReadMultiple<Entry13>(header.table13offset, header.table13count).ToList();
                var table14 = br.ReadMultiple<Entry14>(header.table14offset, header.table14count).ToList();
                var table15 = br.ReadMultiple<Entry15>(header.table15offset, header.table15count).ToList();
                var table16 = br.ReadMultiple<Entry16>(header.table16offset, header.table16count).ToList();
                var table17 = br.ReadMultiple<Entry17>(header.table17offset, header.table17count).ToList();
                var table18 = br.ReadMultiple<Entry18>(header.table18offset, header.table18count).ToList();
                var table19 = br.ReadMultiple<Entry19>(header.table19offset, header.table19count).ToList();
                var table20 = br.ReadMultiple<Entry20>(header.table20offset, header.table20count).ToList();
                var table21 = br.ReadMultiple<Entry21>(header.table21offset, header.table21count).ToList();
                var table22 = br.ReadMultiple<Entry22>(header.table22offset, header.table22count).ToList();
                var table23 = br.ReadMultiple<Entry23>(header.table23offset, header.table23count).ToList();
                var table24 = br.ReadMultiple<Entry24>(header.table24offset, header.table24size / 52).ToList();

                br.BaseStream.Position = header.dataBoolOffset;
                var dataBool = br.ReadBytes(header.data32bitOffset - header.dataBoolOffset);
                var data32bit = br.ReadBytes(header.dataRectOffset - header.data32bitOffset);
                var dataRect = br.ReadBytes(header.dataRectArrayOffset - header.dataRectOffset);
                var dataRectArray = br.ReadBytes(header.dataStringOffset - header.dataRectArrayOffset);
                var dataString = br.ReadBytes(header.table24offset - header.dataStringOffset);

                var spl = Encoding.ASCII.GetString(dataString).Split('\0');
                var dicString = spl.Select((str, i) => Tuple.Create(spl.Take(i).Sum(s => s.Length + 1), str)).ToDictionary(p => p.Item1, p => p.Item2);

                int GetDataTypeLength(int n)
                {
                    switch (n)
                    {
                        case 3: return 1;
                        case 4: return 16;
                        default: return 4;
                    }
                }

                object GetData(int dataType, int dataOffset, int extraOffset = 0)
                {
                    dataOffset += GetDataTypeLength(dataType) * extraOffset;
                    switch (dataType)
                    {
                        case 2: return BitConverter.ToSingle(data32bit, dataOffset);
                        case 3: return BitConverter.ToBoolean(dataBool, dataOffset);
                        case 4: return dataRect.ToStruct<Rectangle>(dataOffset);
                        case 15: return "";
                        case 17: return dataOffset == 1;
                        case 18: return dataOffset;
                        case 32: return new[] { BitConverter.ToInt32(dataRectArray, dataOffset) };
                        case 33:
                            return new RectArray(BitConverter.ToInt32(dataRectArray, dataOffset + 4),
                                Range(0, BitConverter.ToInt32(dataRectArray, dataOffset)).Select(i =>
                                dataRectArray.ToStruct<Rectangle>(dataOffset + 8 + i * 16)));
                        case 64: return new ArraySegment<byte>(dataRectArray, dataOffset, 64).ToArray();
                        default: return BitConverter.ToInt32(data32bit, dataOffset);
                    }
                };

                XElement GetProperty(int n4)
                {
                    var e4 = table4[n4];
                    return new XElement("property",
                        new XAttribute("datatype", e4.dataType),
                        new XAttribute("name", dicString[e4.strProperty]),
                        new XAttribute("value", GetData(e4.dataType, e4.dataOffset)));
                }

                XElement GetAnimatedProperty(int n5)
                {
                    var e5 = table5[n5];
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
                            //e6.frameType != 8 ? null : BitConverter.ToString((byte[])GetData(64, e6.dataOffset))));
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
                                   where name.StartsWith("zero") ? !IsZero(value) ? throw new InvalidDataException($"{tableName}.{name} = {value}") : false : true
                                   where !name.StartsWith("zero")
                                   select name.StartsWith("str") ? (int)value == -1 ? "(null)" : dicString[(int)value]
                                       : name.EndsWith("Hash") ? $"{value:X8}" : $"{value}";
                        sb.AppendLine(string.Join(sep, vals));
                    }
                    return new XElement("block", new XAttribute("name", tableName), new XAttribute("count", table.Count), sb.ToString());

                    bool IsZero(object obj)
                    {
                        switch (obj)
                        {
                            case float f: return f == 0;
                            case int i: return i == 0;
                            case short s: return s == 0;
                            case byte b: return b == 0;
                            case Rectangle r: return r.X0 == 0 && r.Y0 == 0 && r.X1 == 0 && r.Y1 == 0;
                            default: throw new ArgumentException("Unknown type " + obj.GetType());
                        }
                    }
                }

                // All the cool printing stuff goes here!
                MyPrint(new XElement("gui",
                    new XAttribute("filename", filename),
                    new XAttribute("flag0", header.flag0),
                    new XAttribute("flag1", header.flag1),
                    new XAttribute("otherflags", header.otherFlags),
                    new XAttribute("somecounts", string.Join(",", header.somecount0, header.somecount1, header.somecount2, header.somecount3)),
                    new XAttribute("othercount", header.otherCount),
                    PrintTable(table0, nameof(table0)),
                    PrintTable(table1, nameof(table1)),
                    PrintTable(table2, nameof(table2)),
                    PrintTable(table3, nameof(table3)),
                    PrintTable(table4, nameof(table4)),
                    PrintTable(table5, nameof(table5)),
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
                                let e2tex = e2.texture == -1 || e2.tagHash == 0x2787DB24 ? null : (RectArray)GetData(33, e2.texture)
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
                                    //where e3.table4count + e3.table5count != 0
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

                // 11, 15, 16, 17, 18, 19, 20, 22, 24
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

                ////////////////////////
                // let us construct!

                var recon = new Reconstruction(gui);

                //foreach (var e in recon.table5) Debug.WriteLine(e.dataOffset);

                Debug.Assert(recon.cacheBool.Data.SequenceEqual(dataBool));
                Debug.Assert(recon.cache32bit.Data.SequenceEqual(data32bit));
                Debug.Assert(recon.cacheRect.Data.SequenceEqual(dataRect));
                Debug.Assert(recon.cacheRectArray.Data.SequenceEqual(dataRectArray));
                Debug.Assert(recon.cacheString.Data.SequenceEqual(dataString));

                Debug.Assert(recon.table0.TableEqual(table0));
                Debug.Assert(recon.table1.TableEqual(table1));
                Debug.Assert(recon.table2.TableEqual(table2));
                Debug.Assert(recon.table3.TableEqual(table3));
                Debug.Assert(recon.table4.TableEqual(table4));
                Debug.Assert(recon.table5.TableEqual(table5));
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

                Debug.Assert(recon.rectArrayCount == table24.Count);
                Debug.Assert(recon.header.StructToArray().SequenceEqual(header.StructToArray()));

                br.BaseStream.Position = 0;
                var filedata = br.ReadBytes((int)br.BaseStream.Length);
                //Debug.Assert(recon.filedata.SequenceEqual(filedata));

                //Debug.Assert(table7.All(e => e.texture == -1));

            }
        }
    }
}
