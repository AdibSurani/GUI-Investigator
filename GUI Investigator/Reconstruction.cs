using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace GUI_Investigator
{
    class Reconstruction
    {
        // this is the order of blocks in a .gui file
        public Header header;
        public List<Entry0> table0;
        public List<Entry1> table1;
        public List<Entry2> table2;
        public List<Entry3> table3;
        public List<Entry4> table4;
        public List<Entry5> table5;
        public List<Entry7> table7;
        public List<Entry8> table8;
        public List<Entry9> table9;
        public List<Entry10> table10 = new List<Entry10>();
        public List<Entry11> table11;
        public List<Entry12> table12 = new List<Entry12>();
        public List<Entry13> table13 = new List<Entry13>();
        public List<Entry14> table14 = new List<Entry14>();
        public List<Entry15> table15;
        public List<Entry16> table16;
        public List<Entry17> table17;
        public List<Entry18> table18;
        public List<Entry19> table19;
        public List<Entry20> table20;
        public List<Entry21> table21 = new List<Entry21>();
        public List<Entry22> table22;
        public List<Entry23> table23 = new List<Entry23>();
        public List<Entry6> table6 = new List<Entry6>();
        public Cache<bool[]> cacheBool = new Cache<bool[]>(bs => bs.Select(b => (byte)(b ? 1 : 0)).ToArray());
        public Cache<string> cache32bit = new Cache<string>(Convert.FromBase64String);
        public Cache<string> cacheRect = new Cache<string>(r => r.Split(',').Select(float.Parse).SelectMany(BitConverter.GetBytes).ToArray());
        public Cache<object> cacheRectArray; // populated below because it needs a local running count
        public Cache<string> cacheString = new Cache<string>(s => Encoding.ASCII.GetBytes(s + '\0'));
        public List<Entry24> table24 = new List<Entry24>();

        public byte[] filedata;

        public Reconstruction(ParsedGUI gui)
        {
            int rectArrayCount = 0; // should be equal to table24.Count at the end of it all
            cacheRectArray = new Cache<object>(ParseRectArray);
            Parse(gui);

            byte[] ParseRectArray(object obj)
            {
                switch (obj)
                {
                    case List<Rectangle> lst:
                        var ms = new MemoryStream();
                        using (var bw = new BinaryWriter(ms))
                        {
                            bw.Write(lst.Count);
                            bw.Write(lst.Count == 0 ? 0 : rectArrayCount);
                            foreach (var r in lst) bw.Write(r.StructToArray());
                            rectArrayCount += lst.Count;
                            bw.Write(new byte[8]);
                            return ms.ToArray();
                        }
                    case int[] arr: return BitConverter.GetBytes(arr[0]).Concat(new byte[12]).ToArray();
                    case byte[] b: return b;
                    default: throw new NotImplementedException();
                }
            }
        }

        int GetDataOffset(int datatype, string s)
        {
            switch (datatype)
            {
                case 2: return cache32bit[Convert.ToBase64String(BitConverter.GetBytes(XmlConvert.ToSingle(s)))];
                case 3: return cacheBool[new[] { bool.Parse(s) }];
                case 4: return cacheRect[Rectangle.Parse(s).ToString()];
                case 17: return bool.Parse(s) ? 1 : 0;
                case 18: return int.Parse(s);
                default: return cache32bit[Convert.ToBase64String(BitConverter.GetBytes(int.Parse(s)))];
            }
        }

        int GetDataOffset(int datatype, IEnumerable<string> src)
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

        public void Parse(ParsedGUI gui)
        {
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
                                     where evt.animprops != null
                                     from animprop in evt.animprops
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
                            ? cacheRectArray[pane.maps]
                            : pane.something5 != null
                            ? cacheRectArray[new[] { pane.something5.Value }]
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
                            dataOffset = GetDataOffset(animprop.datatype, animprop.changes.Select(change => change.value))
                        };
            table6 = (from animprop in animprops
                      from change in animprop.changes
                      select new Entry6
                      {
                          frame = (short)change.frame,
                          frameType = (byte)change.frameType,
                          dataOffset = change.frameType != 8 ? 0 // technically we'd want to read 64 bytes, but mehh
                            : cacheRectArray[new[] { 0f, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1 }.SelectMany(BitConverter.GetBytes).ToArray()]
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
                          unk0 = evt.unk0.Value,
                          unk1 = evt.unk1.Value,
                          unk2 = evt.unk2.Value,
                          unk3 = evt.unk3.Value,
                          maxframes = evt.maxframes.Value,
                          table5count = evt.animprops.Count
                      }).ToList();
            table11 = (from e11 in gui.parsed11
                       select new Entry11 { id = e11.id }).ToList();
            table15 = (from e15 in gui.parsed15
                       select new Entry15 { id = e15.id, unk = e15.unk }).ToList();
            table16 = (from e16 in gui.parsed16
                       select new Entry16 { unk0 = e16.unk0, unk1 = e16.unk1, unk2 = e16.unk2, unk3 = e16.unk3 }).ToList();
            table17 = (from e17 in gui.parsed17
                       select new Entry17 { id = e17.id, strName = cacheString[e17.name], varHash = e17.varHash, id2 = e17.id2 }).ToList(); // hash
            table18 = (from e18 in gui.parsed18
                       select new Entry18
                       {
                           id = e18.id,
                           width = (short)e18.width,
                           height = (short)e18.height,
                           scaleX = e18.sclX,
                           scaleY = e18.sclY,
                           strPath = e18.path == null ? -1 : cacheString[e18.path],
                           strName = cacheString[e18.name]
                       }).ToList();
            table19 = (from e19 in gui.parsed19
                       select new Entry19 { strPath = cacheString[e19.path] }).ToList();
            table20 = (from e20 in gui.parsed20
                       select new Entry20 { unkHash = e20.unkHash, unk0 = e20.unk0, unk1 = e20.unk1, unk2 = e20.unk2, unk3 = e20.unk3 }).ToList(); // hash
            table22 = (from e22 in gui.parsed22
                       select new Entry22 { unk = e22.unk, strPath = cacheString[e22.path] }).ToList();
            table24 = (from e24 in gui.parsed24
                       select new Entry24 { dst = e24.dst, src = e24.src }).ToList();

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
                filenameHash = gui.id, // hash
                somecount0 = gui.somecount0,
                somecount1 = gui.somecount1,
                somecount2 = gui.somecount2,
                somecount3 = gui.somecount3,
                otherFlags = gui.otherflags,
                otherCount = gui.othercount,
                table5subcount = table5.Count - table7.Count,
                table7count2 = table7.Count
            };

            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                void WriteBytes(ref int offset, params byte[][] buffers)
                {
                    offset = (int)bw.BaseStream.Position;
                    foreach (var item in buffers)
                        bw.Write(item);
                    while (bw.BaseStream.Position % 16 != 0)
                        bw.BaseStream.WriteByte(0);
                }
                void WriteTable<T>(ref int offset, ref int count, List<T> table)
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
                WriteBytes(ref header.dataRectArrayOffset, cacheRectArray.Data);
                WriteBytes(ref header.dataStringOffset, cacheString.Data);
                WriteTable(ref header.table24offset, ref header.table24size, table24);
                header.table24size *= 52;
                WriteBytes(ref header.filesize, new byte[0]);
                bw.BaseStream.Position = 0;
                WriteBytes(ref header.zero3, header.StructToArray());
                filedata = ((MemoryStream)bw.BaseStream).ToArray();
            }
        }
    }
}
