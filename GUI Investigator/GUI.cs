using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static System.Linq.Enumerable;

namespace GUI_Investigator
{
    [XmlRoot("gui")]
    public class GUI
    {
        [XmlAttribute]
        public int filenameHash, flag0, flag1, otherflags, width, height;
        [XmlAttribute]
        public int somecount0, somecount1, somecount2, somecount3, othercount;
        [XmlElement("anim")]
        public List<Anim> anims;
        [XmlElement("pane")]
        public List<Pane> panes;
        [XmlElement("event")]
        public List<Event> events;
        public Unknown unknown;

        const int Entry24StructSize = 48;

        public static GUI FromByteArray(byte[] bytes)
        {
            var header = Read<Header>(0);
            var spl = Encoding.ASCII.GetString(bytes, (int)header.dataStringOffset, (int)(header.table24offset - header.dataStringOffset)).Split('\0');
            var dicString = spl.Select((str, i) => (spl.Take(i).Sum(s => s.Length + 1), str)).ToDictionary(p => p.Item1, p => p.Item2);

            #region a bunch of local functions
            T Read<T>(long baseOffset, int itemOffset = 0)
            {
                return bytes.ToStruct<T>((int)baseOffset, itemOffset);
            }

            List<T> ReadMultiple<T>(long offset, int count)
            {
                return Range(0, count).Select(i => Read<T>(offset, i)).ToList();
            }

            string GetData(int dataType, int dataOffset, int extraOffset = 0)
            {
                dataOffset += (dataType == 3 ? 1 : dataType == 4 ? 16 : 4) * extraOffset;
                switch ((byte)dataType)
                {
                    case 2: return XmlConvert.ToString(Read<float>(header.data32bitOffset + dataOffset));
                    case 3: return XmlConvert.ToString(Read<byte>(header.dataBoolOffset + dataOffset) == 1);
                    case 4: return Read<Rectangle>(header.dataRectOffset + dataOffset).ToString();
                    case 15: return "";
                    case 17: return XmlConvert.ToString(dataOffset == 1);
                    case 18: return XmlConvert.ToString(dataOffset);
                    default: return XmlConvert.ToString(Read<int>(header.data32bitOffset + dataOffset));
                }
            };

            long[] GetMiscInt(int offset)
            {
                return new[] { Read<long>(header.dataMiscOffset + offset) };
            }

            List<Rectangle> GetRectList(int offset)
            {
                return Range(0, Read<int>(header.dataMiscOffset + offset)).Select(i => Read<Rectangle>(header.dataMiscOffset + offset + 8, i)).ToList();
            }

            Property GetProperty(int n4)
            {
                var e4 = Read<Entry4>(header.table4offset, n4);
                return new Property
                {
                    datatype = e4.dataType,
                    name = dicString[e4.strProperty],
                    value = GetData(e4.dataType, e4.dataOffset)
                };
            }

            AnimatedProperty GetAnimatedProperty(int n5)
            {
                var e5 = Read<Entry5>(header.table5offset, n5);
                return new AnimatedProperty
                {
                    id = e5.id,
                    datatype = e5.dataType,
                    name = dicString[e5.strProperty],
                    changes = (from i in Range(0, e5.count)
                               let e6 = Read<Entry6>(header.table6offset, e5.table6start + i)
                               select new AnimatedProperty.Change
                               {
                                   frame = e6.frame,
                                   frameType = e6.frameType,
                                   value = GetData(e5.dataType, e5.dataOffset, i)
                               }).ToList()
                };
            }
            #endregion

            return new GUI
            {
                filenameHash = header.filenameHash, // hash
                flag0 = header.flag0,
                flag1 = header.flag1,
                otherflags = header.otherFlags,
                width = header.width,
                height = header.height,
                somecount0 = header.somecount0,
                somecount1 = header.somecount1,
                somecount2 = header.somecount2,
                somecount3 = header.somecount3,
                othercount = (int)header.otherCount,
                anims = (from e0 in ReadMultiple<Entry0>(header.table0offset, header.table0count)
                         select new Anim
                         {
                             id = e0.id,
                             name = dicString[e0.strName],
                             table2subcount = e0.table2subcount, // can be discovered?
                             seqs = (from n1 in Range(e0.table1start, e0.table1count)
                                     let e1 = Read<Entry1>(header.table1offset, n1)
                                     select new Anim.Sequence
                                     {
                                         id = e1.id,
                                         maxframes = e1.maxframes, // can be discovered
                                         name = dicString[e1.strName]
                                     }).ToList(),
                             panes = (from n2 in Range(e0.table2start, e0.table2count)
                                      let e2 = Read<Entry2>(header.table2offset, n2)
                                      let e2tex = e2.texture == -1 || e2.tagHash == 0x2787DB24 ? new List<Rectangle>() : GetRectList(e2.texture)
                                      let e2val = e2.tagHash != 0x2787DB24 ? null : GetMiscInt(e2.texture)
                                      select new Anim.AnimPane
                                      {
                                          id = e2.id,
                                          type = e2.tagHash, //hash
                                          name = dicString[e2.strName],
                                          next = e2.next,
                                          child = e2.child,
                                          maps = e2tex, // can we also map this to e24 somehow?
                                          something5 = e2val,
                                          props = Range(e2.table4start, e2.table4count).Select(GetProperty).ToList(),
                                          states = (from index in Range(0, e0.table1count)
                                                    let e1 = Read<Entry1>(header.table1offset, e0.table1start + index)
                                                    let e3 = Read<Entry3>(header.table3offset, e2.table3start + index)
                                                    select new Anim.AnimPane.State
                                                    {
                                                        sequencename = dicString[e1.strName], // not an e3 property
                                                        maxframes = e3.maxframes, // can be discovered
                                                        unk0 = e3.unk0,
                                                        unk1 = e3.unk1,
                                                        props = Range(e3.table4start, e3.table4count).Select(GetProperty).ToList(),
                                                        animprops = Range(e3.table5start, e3.table5count).Select(GetAnimatedProperty).ToList(),
                                                    }).ToList()
                                      }).ToList()
                         }).ToList(),
                events = (from e8 in ReadMultiple<Entry8>(header.table8offset, header.table8count)
                          let e9 = e8.type == 2 ? Read<Entry9>(header.table9offset, e8.table9entry) : null
                          select new Event
                          {
                              id = e8.id,
                              type = e8.type,
                              name = dicString[e8.strName],
                              t9entry = e8.table9entry,
                              t9values = e9 == null ? null : new Event.T9Values
                              {
                                  unk0 = e9.unk0,
                                  unk1 = e9.unk1,
                                  unk2 = e9.unk2,
                                  unk3 = e9.unk3,
                                  unk4 = e9.unk4,
                                  maxframes = e9.maxframes,
                                  animprops = Range(e9.table5start, e9.table5count).Select(GetAnimatedProperty).ToList()
                              },
                          }).ToList(),
                panes = (from n7 in Range(0, header.table7count)
                         let e7 = Read<Entry7>(header.table7offset, n7)
                         select new Pane
                         {
                             id = e7.id,
                             type = e7.tagHash, // X8
                             name = dicString[e7.strName],
                             next = e7.next,
                             child = e7.child,
                             props = Range(e7.table4start, e7.table4count).Select(GetProperty).ToList(),
                             animprop = GetAnimatedProperty((int)header.table5subcount + n7)
                         }).ToList(),
                unknown = new Unknown
                {
                    parsed11 = (from e11 in ReadMultiple<Entry11>(header.table11offset, header.table11count)
                                select new Parsed11 { id = e11.id }).ToList(),
                    parsed15 = (from e15 in ReadMultiple<Entry15>(header.table15offset, header.table15count)
                                select new Parsed15 { id = e15.id, unk = e15.unk }).ToList(),
                    parsed16 = (from e16 in ReadMultiple<Entry16>(header.table16offset, header.table16count)
                                select new Parsed16 { unk0 = e16.unk0, unk1 = e16.unk1, unk2 = e16.unk2, unk3 = e16.unk3 }).ToList(),
                    parsed17 = (from e17 in ReadMultiple<Entry17>(header.table17offset, header.table17count)
                                select new Parsed17 { id = e17.id, name = dicString[e17.strName], varHash = e17.varHash, id2 = e17.id2 }).ToList(), // hash
                    parsed18 = (from e18 in ReadMultiple<Entry18>(header.table18offset, header.table18count)
                                select new Parsed18
                                {
                                    id = e18.id,
                                    unk = e18.unk,
                                    width = e18.width,
                                    height = e18.height,
                                    sclX = e18.scaleX,
                                    sclY = e18.scaleY,
                                    sclZ = e18.scaleZ,
                                    sclW = e18.scaleW,
                                    //scl = e18.scale,
                                    name = dicString[e18.strName],
                                    path = e18.strPath == -1 ? null : dicString[e18.strPath]
                                }).ToList(),
                    parsed19 = (from e19 in ReadMultiple<Entry19>(header.table19offset, header.table19count)
                                //select new Parsed19 { path = dicString[e19.strPath] }).ToList(),
                                select new Parsed19 { unk = e19.unk, path = e19.strPath == -1 ? null : dicString[e19.strPath] }).ToList(),
                    parsed20 = (from e20 in ReadMultiple<Entry20>(header.table20offset, header.table20count)
                                select new Parsed20 { unkHash = e20.unkHash, unk0 = e20.unk0, unk1 = e20.unk1, unk2 = e20.unk2, unk3 = e20.unk3 }).ToList(), //hash
                    parsed22 = (from e22 in ReadMultiple<Entry22>(header.table22offset, header.table22count)
                                select new Parsed22 { unk = e22.unk, path = dicString[e22.strPath] }).ToList(),
                    parsed24 = (from e24 in ReadMultiple<Entry24>(header.table24offset, header.table24size / Entry24StructSize)
                                select new Parsed24 { dst = e24.dst, unk = e24.unk, src = e24.src }).ToList()
                }
            };
        }

        public byte[] ToByteArray()
        {
            var gui = this;
            int miscCount = 0; // used for cacheMisc. should be equal to table24.Count at the end of it all

            #region variable/function list
            // this is the order of blocks in a .gui file
            // just a basic list of variables and functions-- no code executes here except for class constructors
            Header header;
            List<Entry0> table0;
            List<Entry1> table1;
            List<Entry2> table2;
            List<Entry3> table3;
            List<Entry4> table4;
            List<Entry5> table5;
            List<Entry7> table7;
            List<Entry8> table8;
            List<Entry9> table9;
            List<Entry10> table10 = new List<Entry10>();
            List<Entry11> table11;
            List<Entry12> table12 = new List<Entry12>();
            List<Entry13> table13 = new List<Entry13>();
            List<Entry14> table14 = new List<Entry14>();
            List<Entry15> table15;
            List<Entry16> table16;
            List<Entry17> table17;
            List<Entry18> table18;
            List<Entry19> table19;
            List<Entry20> table20;
            List<Entry21> table21 = new List<Entry21>();
            List<Entry22> table22;
            List<Entry23> table23 = new List<Entry23>();
            List<Entry6> table6 = new List<Entry6>();
            Cache<bool[]> cacheBool = new Cache<bool[]>(bs => bs.Select(b => (byte)(b ? 1 : 0)).ToArray());
            Cache<string> cache32bit = new Cache<string>(Convert.FromBase64String);
            Cache<string> cacheRect = new Cache<string>(r => r.Split(',').Select(float.Parse).SelectMany(BitConverter.GetBytes).ToArray());
            Cache<object> cacheMisc = new Cache<object>(ParseMisc); // populated below because it needs a local running count
            Cache<string> cacheString = new Cache<string>(s => Encoding.ASCII.GetBytes(s + '\0'));
            List<Entry24> table24 = new List<Entry24>();

            byte[] ParseMisc(object obj)
            {
                switch (obj)
                {
                    case List<Rectangle> lst:
                        var ms = new MemoryStream();
                        using (var bw = new BinaryWriter(ms))
                        {
                            bw.Write(lst.Count);
                            bw.Write(lst.Count == 0 ? 0 : miscCount);
                            foreach (var r in lst) bw.Write(r.StructToArray());
                            miscCount += lst.Count * 6 - 2;
                            bw.Write(new byte[8]);
                            return ms.ToArray();
                        }
                    case long[] arr: return BitConverter.GetBytes(arr[0]).Concat(new byte[8]).ToArray();
                    case byte[] b: return b;
                    default: throw new NotImplementedException();
                }
            }

            int GetDataOffset(int datatype, string s)
            {
                switch ((byte)datatype)
                {
                    case 2: return cache32bit[Convert.ToBase64String(BitConverter.GetBytes(XmlConvert.ToSingle(s)))];
                    case 3: return cacheBool[new[] { bool.Parse(s) }];
                    case 4: return cacheRect[Rectangle.Parse(s).ToString()];
                    case 17: return bool.Parse(s) ? 1 : 0;
                    case 18: return int.Parse(s);
                    default: return cache32bit[Convert.ToBase64String(BitConverter.GetBytes(int.Parse(s)))];
                }
            }

            int GetDataOffsetMulti(int datatype, IEnumerable<string> src)
            {
                switch (datatype)
                {
                    case 2: return cache32bit[Convert.ToBase64String(src.Select(XmlConvert.ToSingle).SelectMany(BitConverter.GetBytes).ToArray())];
                    case 3: return cacheBool[src.Select(bool.Parse).ToArray()];
                    case 4: return cacheRect[string.Join(",", src.Select(Rectangle.Parse))];
                    case 15: return cache32bit.Length;
                    default: return cache32bit[Convert.ToBase64String(src.Select(int.Parse).SelectMany(BitConverter.GetBytes).ToArray())];
                }
            }
            #endregion

            #region list of all properties and animated properties
            var props = (from anim in gui.anims
                         from pane in anim.panes
                         from prop in pane.props
                         select prop)
                         .Concat(from anim in gui.anims
                                 from pane in anim.panes
                                 from state in pane.states
                                 from prop in state.props
                                 select prop)
                         .Concat(from pane in gui.panes
                                 from prop in pane.props
                                 select prop);
            var animprops = (from anim in gui.anims
                             from pane in anim.panes
                             from state in pane.states
                             from animprop in state.animprops
                             select animprop)
                             .Concat(from evt in gui.events
                                     where evt.type == 2 // an enum would be nice eventually
                                     from animprop in evt.t9values.animprops
                                     select animprop)
                             .Concat(from pane in gui.panes
                                     select pane.animprop);
            #endregion

            #region populate tables
            table0 = (from anim in gui.anims
                      select new Entry0
                      {
                          strName = cacheString[anim.name],
                          id = anim.id,
                          table2subcount = (short)anim.table2subcount,
                          table1count = (short)anim.seqs.Count,
                          table2count = (short)anim.panes.Count,
                          table5count = (short)(from pane in anim.panes
                                                from state in pane.states
                                                from animprop in state.animprops
                                                select animprop).Count()
                      }).ToList();
            table1 = (from anim in gui.anims
                      from seq in anim.seqs
                      select new Entry1
                      {
                          strName = cacheString[seq.name],
                          id = seq.id,
                          maxframes = seq.maxframes
                      }).ToList();
            table2 = (from anim in gui.anims
                      from pane in anim.panes
                      select new Entry2
                      {
                          id = pane.id,
                          tagHash = pane.type, // hash
                          next = pane.next,
                          child = pane.child,
                          table4count = (byte)pane.props.Count,
                          table5count = (byte)(from state in pane.states
                                               from animprop in state.animprops
                                               select animprop).Count(),
                          strName = cacheString[pane.name],
                          texture = pane.maps.Any() || pane.type == 0x4F7228FC
                            ? cacheMisc[pane.maps]
                            : pane.something5 != null
                            ? cacheMisc[pane.something5]
                            : -1
                      }).ToList();
            table3 = (from anim in gui.anims
                      from pane in anim.panes
                      from state in pane.states
                      select new Entry3
                      {
                          table4count = (byte)state.props.Count,
                          table5count = (byte)state.animprops.Count,
                          maxframes = (short)state.maxframes,
                          unk0 = (short)state.unk0,
                          unk1 = (short)state.unk1
                      }).ToList();
            var lazy4 = from prop in props
                        select new Entry4
                        {
                            strProperty = cacheString[prop.name],
                            dataType = prop.datatype,
                            dataOffset = GetDataOffset(prop.datatype, prop.value)
                        };
            var lazy5 = from animprop in animprops
                        select new Entry5
                        {
                            strProperty = cacheString[animprop.name],
                            dataType = (byte)animprop.datatype,
                            count = (byte)animprop.changes.Count,
                            id = animprop.id,
                            dataOffset = GetDataOffsetMulti(animprop.datatype, animprop.changes.Select(change => change.value))
                        };
            table6 = (from animprop in animprops
                      from change in animprop.changes
                      select new Entry6
                      {
                          frame = (short)change.frame,
                          frameType = (byte)change.frameType,
                          dataOffset = change.frameType != 8 ? 0 // technically we'd want to read 64 bytes, but mehh
                            : cacheMisc[new[] { 0f, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1 }.SelectMany(BitConverter.GetBytes).ToArray()]
                      }).ToList();
            table7 = (from pane in gui.panes
                      select new Entry7
                      {
                          id = pane.id,
                          next = pane.next,
                          child = pane.child,
                          table4count = pane.props.Count,
                          strName = cacheString[pane.name],
                          tagHash = pane.type // hash
                      }).ToList();
            table8 = (from evt in gui.events
                      select new Entry8
                      {
                          id = evt.id,
                          type = evt.type,
                          strName = cacheString[evt.name],
                          table9entry = evt.t9entry
                      }).ToList();
            table9 = (from evt in gui.events
                      where evt.type == 2
                      select new Entry9
                      {
                          unk0 = evt.t9values.unk0,
                          unk1 = evt.t9values.unk1,
                          unk2 = evt.t9values.unk2,
                          unk3 = evt.t9values.unk3,
                          unk4 = evt.t9values.unk4,
                          maxframes = evt.t9values.maxframes,
                          table5count = evt.t9values.animprops.Count
                      }).ToList();
            table11 = (from e11 in gui.unknown.parsed11
                       select new Entry11 { id = e11.id }).ToList();
            table15 = (from e15 in gui.unknown.parsed15
                       select new Entry15 { id = e15.id, unk = e15.unk }).ToList();
            table16 = (from e16 in gui.unknown.parsed16
                       select new Entry16 { unk0 = e16.unk0, unk1 = e16.unk1, unk2 = e16.unk2, unk3 = e16.unk3 }).ToList();
            table17 = (from e17 in gui.unknown.parsed17
                       select new Entry17 { id = e17.id, strName = cacheString[e17.name], varHash = e17.varHash, id2 = e17.id2 }).ToList(); // hash
            table18 = (from e18 in gui.unknown.parsed18
                       select new Entry18
                       {
                           id = e18.id,
                           unk = e18.unk,
                           width = (short)e18.width,
                           height = (short)e18.height,
                           scaleX = e18.sclX,
                           scaleY = e18.sclY,
                           scaleZ = e18.sclZ,
                           scaleW = e18.sclW,
                           //scale = e18.scl,
                           strPath = e18.path == null ? -1 : cacheString[e18.path],
                           strName = cacheString[e18.name]
                       }).ToList();
            table19 = (from e19 in gui.unknown.parsed19
                       select new Entry19 { unk = e19.unk, strPath = cacheString[e19.path] }).ToList();
            table20 = (from e20 in gui.unknown.parsed20
                       select new Entry20 { unkHash = e20.unkHash, unk0 = e20.unk0, unk1 = e20.unk1, unk2 = e20.unk2, unk3 = e20.unk3 }).ToList(); // hash
            table22 = (from e22 in gui.unknown.parsed22
                       select new Entry22 { unk = e22.unk, strPath = cacheString[e22.path] }).ToList();
            table24 = (from e24 in gui.unknown.parsed24
                       select new Entry24 { dst = e24.dst, unk = e24.unk, src = e24.src }).ToList();

            table4 = lazy4.ToList();
            table5 = lazy5.ToList();
            #endregion

            #region adjust table offsets
            for (int i = 0; i < table0.Count - 1; i++)
            {
                table0[i + 1].table1start = table0[i].table1start + table0[i].table1count;
                table0[i + 1].table2start = table0[i].table2start + table0[i].table2count;
            }
            var stuff = table0.SelectMany(e => Enumerable.Repeat(e.table1count, e.table2count)).ToList();
            for (int i = 0; i < table2.Count - 1; i++)
            {
                table2[i + 1].table4start = table2[i].table4start + table2[i].table4count;
                table2[i + 1].table3start = stuff.Take(i + 1).Sum(n => n);
            }
            if (table3.Any() && table2.Any())
                table3[0].table4start = table2.Last().table4start + table2.Last().table4count;
            for (int i = 0; i < table3.Count - 1; i++)
            {
                table3[i + 1].table4start = table3[i].table4start + table3[i].table4count;
                table3[i + 1].table5start = table3[i].table5start + table3[i].table5count;
            }
            for (int i = 0; i < table5.Count - 1; i++)
            {
                table5[i + 1].table6start = table5[i].table6start + table5[i].count;
            }
            if (table3.Any() && table7.Any())
                table7[0].table4start = table3.Last().table4start + table3.Last().table4count;
            for (int i = 0; i < table7.Count - 1; i++)
            {
                table7[i + 1].table4start = table7[i].table4start + table7[i].table4count;
            }
            if (table3.Any() && table9.Any())
                table9[0].table5start = table3.Last().table5start + table3.Last().table5count;
            for (int i = 0; i < table9.Count - 1; i++)
            {
                table9[i + 1].table5start = table9[i].table5start + table9[i].table5count;
            }
            #endregion

            header = new Header
            {
                flag0 = (byte)gui.flag0,
                flag1 = (byte)gui.flag1,
                filenameHash = gui.filenameHash, // hash
                somecount0 = gui.somecount0,
                somecount1 = gui.somecount1,
                somecount2 = gui.somecount2,
                somecount3 = gui.somecount3,
                otherFlags = gui.otherflags,
                width = gui.width,
                height = gui.height,
                otherCount = gui.othercount,
                table5subcount = table5.Count - table7.Count,
                table7count2 = table7.Count
            };

            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                void WriteBytesI(ref int offset, params byte[][] buffers)
                {
                    offset = (int)bw.BaseStream.Position;
                    foreach (var item in buffers)
                        bw.Write(item);
                    while (bw.BaseStream.Position % 16 != 0)
                        bw.BaseStream.WriteByte(0);
                }
                void WriteBytes(ref long offset, params byte[][] buffers)
                {
                    offset = (int)bw.BaseStream.Position;
                    foreach (var item in buffers)
                        bw.Write(item);
                    while (bw.BaseStream.Position % 16 != 0)
                        bw.BaseStream.WriteByte(0);
                }
                void WriteTable<T>(ref long offset, ref int count, List<T> table)
                {
                    count = table.Count;
                    WriteBytes(ref offset, table.Select(item => item.StructToArray()).ToArray());
                }

                WriteBytes(ref header.zero3, header.StructToArray());
                WriteTable(ref header.table0offset, ref header.table0count, table0);
                WriteTable(ref header.table1offset, ref header.table1count, table1);
                WriteTable(ref header.table2offset, ref header.table2count, table2);
                WriteTable(ref header.table3offset, ref header.table3count, table3);
                WriteTable(ref header.table4offset, ref header.table4count, table4);
                WriteTable(ref header.table5offset, ref header.table5count, table5);
                WriteTable(ref header.table7offset, ref header.table7count, table7);
                WriteTable(ref header.table8offset, ref header.table8count, table8);
                WriteTable(ref header.table9offset, ref header.table9count, table9);
                WriteTable(ref header.table10offset, ref header.table10count, table10);
                WriteTable(ref header.table11offset, ref header.table11count, table11);
                WriteTable(ref header.table12offset, ref header.table12count, table12);
                WriteTable(ref header.table13offset, ref header.table13count, table13);
                WriteTable(ref header.table14offset, ref header.table14count, table14);
                WriteTable(ref header.table15offset, ref header.table15count, table15);
                WriteTable(ref header.table16offset, ref header.table16count, table16);
                WriteTable(ref header.table17offset, ref header.table17count, table17);
                WriteTable(ref header.table18offset, ref header.table18count, table18);
                WriteTable(ref header.table19offset, ref header.table19count, table19);
                WriteTable(ref header.table20offset, ref header.table20count, table20);
                WriteTable(ref header.table21offset, ref header.table21count, table21);
                WriteTable(ref header.table22offset, ref header.table22count, table22);
                WriteTable(ref header.table23offset, ref header.table23count, table23);
                WriteTable(ref header.table6offset, ref header.table6count, table6);
                WriteBytes(ref header.dataBoolOffset, cacheBool.Data);
                WriteBytes(ref header.data32bitOffset, cache32bit.Data);
                WriteBytes(ref header.dataRectOffset, cacheRect.Data);
                WriteBytes(ref header.dataMiscOffset, cacheMisc.Data);
                WriteBytes(ref header.dataStringOffset, cacheString.Data);
                WriteTable(ref header.table24offset, ref header.table24size, table24);
                header.table24size *= Entry24StructSize;
                WriteBytesI(ref header.filesize, new byte[0]);
                bw.BaseStream.Position = 0;
                WriteBytes(ref header.zero3, header.StructToArray());
                return ((MemoryStream)bw.BaseStream).ToArray();
            }
        }

        public static GUI FromXmlString(string xml)
        {
            using (var sr = new StringReader(xml))
            {
                return (GUI)new XmlSerializer(typeof(GUI)).Deserialize(sr);
            }
        }

        public string ToXmlString()
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var xmlSettings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                NewLineOnAttributes = false,
                IndentChars = "\t",
                CheckCharacters = false,
                OmitXmlDeclaration = true
            };
            using (var sw = new StringWriter())
            {
                new XmlSerializer(typeof(GUI)).Serialize(XmlWriter.Create(sw, xmlSettings), this, ns);
                return sw.ToString();
            }
        }
    }
}
