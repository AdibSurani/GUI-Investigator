using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GUI_Investigator
{
    class Reconstruction
    {
        // this is the order of blocks in a .gui file
        public Header header = new Header();
        public List<Entry0> table0 = new List<Entry0>();
        public List<Entry1> table1 = new List<Entry1>();
        public List<Entry2> table2 = new List<Entry2>();
        public List<Entry3> table3 = new List<Entry3>();
        public List<Entry4> table4 = new List<Entry4>();
        public List<Entry5> table5 = new List<Entry5>();
        public List<Entry7> table7 = new List<Entry7>();
        public List<Entry8> table8 = new List<Entry8>();
        public List<Entry9> table9 = new List<Entry9>();
        public List<Entry10> table10 = new List<Entry10>();
        public List<Entry11> table11 = new List<Entry11>();
        public List<Entry12> table12 = new List<Entry12>();
        public List<Entry13> table13 = new List<Entry13>();
        public List<Entry14> table14 = new List<Entry14>();
        public List<Entry15> table15 = new List<Entry15>();
        public List<Entry16> table16 = new List<Entry16>();
        public List<Entry17> table17 = new List<Entry17>();
        public List<Entry18> table18 = new List<Entry18>();
        public List<Entry19> table19 = new List<Entry19>();
        public List<Entry20> table20 = new List<Entry20>();
        public List<Entry21> table21 = new List<Entry21>();
        public List<Entry22> table22 = new List<Entry22>();
        public List<Entry23> table23 = new List<Entry23>();
        public List<Entry6> table6 = new List<Entry6>();
        public Cache<bool[]> cacheBool = new Cache<bool[]>(bs => bs.Select(b => (byte)(b ? 1 : 0)).ToArray());
        public Cache<string> cache32bit = new Cache<string>(Convert.FromBase64String);
        public Cache<string> cacheRect = new Cache<string>(r => r.Split(',').Select(float.Parse).SelectMany(BitConverter.GetBytes).ToArray());
        public Cache<object> cacheRectArray;
        public Cache<string> cacheString = new Cache<string>(s => Encoding.ASCII.GetBytes(s + '\0'));
        public List<Entry24> table24 = new List<Entry24>();

        public byte[] filedata;

        public Reconstruction(XElement guixml, ParsedGUI gui)
        {
            int rectArrayCount = 0; // should be equal to table24.Count at the end of it all
            cacheRectArray = new Cache<object>(ParseRectArray);
            Parse(guixml, gui);

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

        public int CacheString(string s) => cacheString[s];

        int GetDataOffset(int datatype, XAttribute attr)
        {
            switch (datatype)
            {
                case 2: return cache32bit[Convert.ToBase64String(BitConverter.GetBytes((float)attr))];
                case 3: return cacheBool[new[] { (bool)attr }];
                case 4: return cacheRect[Rectangle.Parse(attr.Value).ToString()];
                case 17: return (bool)attr ? 1 : 0;
                case 18: return (int)attr;
                default: return cache32bit[Convert.ToBase64String(BitConverter.GetBytes((int)attr))];
            }
        }

        int GetDataOffsetMulti(int datatype, IEnumerable<XAttribute> attrs)
        {
            switch (datatype)
            {
                case 2: return cache32bit[Convert.ToBase64String(attrs.Select(attr => (float)attr).SelectMany(BitConverter.GetBytes).ToArray())];
                case 3: return cacheBool[attrs.Select(attr => (bool)attr).ToArray()];
                case 4: return cacheRect[string.Join(",", attrs.Select(attr => Rectangle.Parse(attr.Value)))];
                case 15: return cache32bit.Length;
                default: return cache32bit[Convert.ToBase64String(attrs.Select(attr => (int)attr).SelectMany(BitConverter.GetBytes).ToArray())];
            }
        }

        int FromHex(string s) => Convert.ToInt32(s, 16);

        public void Parse(XElement guixml, ParsedGUI gui)
        {
            table0 = (from anim in gui.anims
                      select new Entry0
                      {
                          strName = cacheString[anim.name],
                          id = anim.id,
                          table2subcount = (short)anim.table2subcount,
                          table1count = (short)anim.seqs.Count,
                          table2count = (short)anim.panes.Count,
                          table5count = (short)anim.panes.SelectMany(pane => pane.states).SelectMany(state => state.animprops).Count()
                      }).ToList();
            table1 = (from seq in gui.anims.SelectMany(anim => anim.seqs)
                      select new Entry1
                      {
                          strName = cacheString[seq.name],
                          id = seq.id,
                          maxframes = seq.maxframes
                      }).ToList();
            table2 = (from pane in guixml.Elements("anim").Elements("pane")
                      let maps = pane.Elements("map")
                      let something5 = pane.Element("something5")
                      select new Entry2
                      {
                          id = (int)pane.Attribute("id"),
                          tagHash = FromHex(pane.Attribute("type").Value),
                          next = (int)pane.Attribute("next"),
                          child = (int)pane.Attribute("child"),
                          table4count = (byte)pane.Elements("property").Count(),
                          table5count = (byte)pane.Elements("state").Elements("animatedproperty").Count(),
                          strName = CacheString(pane.Attribute("name").Value),
                          texture = maps.Any() || FromHex(pane.Attribute("type").Value) == 0x4F7228FC
                            ? cacheRectArray[maps.Select(map => Rectangle.Parse(map.Attribute("rect").Value)).ToList()]
                            : something5 != null
                            ? cacheRectArray[new[] { (int)something5.Attribute("value") }]
                            : -1
                      }
                      ).ToList();
            table3 = (from state in guixml.Elements("anim").Elements("pane").Elements("state")
                      select new Entry3
                      {
                          table4count = (byte)state.Elements("property").Count(),
                          table5count = (byte)state.Elements("animatedproperty").Count(),
                          maxframes = (short)state.Attribute("maxframes"),
                          unk0 = (short)state.Attribute("unk0"),
                          unk1 = (short)state.Attribute("unk1")
                      }
                      ).ToList();

            // tables 4-6 are deferred until later
            table7 = (from pane in guixml.Elements("pane")
                      select new Entry7
                      {
                          id = (int)pane.Attribute("id"),
                          next = (int)pane.Attribute("next"),
                          child = (int)pane.Attribute("child"),
                          table4count = pane.Elements("property").Count(),
                          strName = CacheString(pane.Attribute("name").Value),
                          tagHash = FromHex(pane.Attribute("type").Value)
                      }
                      ).ToList();
            table8 = (from evt in guixml.Elements("event")
                      select new Entry8
                      {
                          id = (int)evt.Attribute("id"),
                          type = (int)evt.Attribute("type"),
                          strName = CacheString(evt.Attribute("name").Value),
                          table9entry = (int)evt.Attribute("t9entry")
                      }
                      ).ToList();
            table9 = (from evt in guixml.Elements("event")
                      where (int)evt.Attribute("type") == 2
                      let unks = evt.Attribute("e9unks").Value.Split(',').Select(int.Parse).ToList()
                      select new Entry9
                      {
                          unk0 = unks[0],
                          unk1 = unks[1],
                          unk2 = unks[2],
                          unk3 = unks[3],
                          maxframes = (int)evt.Attribute("maxframes"),
                          table5count = evt.Elements("animatedproperty").Count()
                      }
                      ).ToList();
            table11 = (from e11 in guixml.Elements("misc").Elements("e11")
                       select new Entry11
                       {
                           id = (int)e11.Attribute("id")
                       }
                       ).ToList();
            table15 = (from e15 in guixml.Elements("misc").Elements("e15")
                       select new Entry15
                       {
                           id = (int)e15.Attribute("id"),
                           unk = (int)e15.Attribute("unk")
                       }
                       ).ToList();
            table16 = (from e16 in guixml.Elements("misc").Elements("e16")
                       let unks = e16.Attribute("unks").Value.Split(',').Select(int.Parse).ToList()
                       select new Entry16
                       {
                           unk0 = unks[0],
                           unk1 = unks[1],
                           unk2 = unks[2],
                           unk3 = unks[3]
                       }
                       ).ToList();
            table17 = (from e17 in guixml.Elements("misc").Elements("e17")
                       select new Entry17
                       {
                           id = (int)e17.Attribute("id"),
                           strName = CacheString(e17.Attribute("name").Value),
                           varHash = FromHex(e17.Attribute("varHash").Value),
                           id2 = (int)e17.Attribute("id2")
                       }
                      ).ToList();
            table18 = (from e18 in guixml.Element("misc").Elements("e18")
                       select new Entry18
                       {
                           id = (int)e18.Attribute("id"),
                           width = (short)e18.Attribute("width"),
                           height = (short)e18.Attribute("height"),
                           scaleX = (float)e18.Attribute("sclX"),
                           scaleY = (float)e18.Attribute("sclY"),
                           strPath = e18.Attribute("path") == null ? -1 : CacheString(e18.Attribute("path").Value),
                           strName = CacheString(e18.Attribute("name").Value)
                       }
                       ).ToList();
            table19 = (from e19 in guixml.Element("misc").Elements("e19")
                       select new Entry19 { strPath = CacheString(e19.Attribute("path").Value) }
                       ).ToList();
            table20 = (from e20 in guixml.Element("misc").Elements("e20")
                       let unks = e20.Attribute("unks").Value.Split(',').Select(int.Parse).ToList()
                       select new Entry20
                       {
                           unkHash = FromHex(e20.Attribute("unkHash").Value),
                           unk0 = unks[0],
                           unk1 = unks[1],
                           unk2 = unks[2],
                           unk3 = unks[3]
                       }
                       ).ToList();
            table22 = (from e22 in guixml.Element("misc").Elements("e22")
                       select new Entry22
                       {
                           unk = (int)e22.Attribute("unk"),
                           strPath = CacheString(e22.Attribute("path").Value)
                       }
                       ).ToList();
            table24 = (from e24 in guixml.Element("misc").Elements("e24")
                       select new Entry24
                       {
                           dst = Rectangle.Parse(e24.Attribute("dst").Value),
                           src = Rectangle.Parse(e24.Attribute("src").Value)
                       }
                       ).ToList();

            // now we jump onto the properties
            var props = guixml.Elements("anim").Elements("pane").Elements("property")
                         .Concat(guixml.Elements("anim").Elements("pane").Elements("state").Elements("property"))
                         .Concat(guixml.Elements("pane").Elements("property"));
            var animprops = guixml.Elements("anim").Elements("pane").Elements("state").Elements("animatedproperty")
                             .Concat(guixml.Elements("event").Elements("animatedproperty"))
                             .Concat(guixml.Elements("pane").Elements("animatedproperty"));

            table4 = (from prop in props
                      select new Entry4
                      {
                          strProperty = CacheString(prop.Attribute("name").Value),
                          dataType = (int)prop.Attribute("datatype"),
                          dataOffset = GetDataOffset((int)prop.Attribute("datatype"), prop.Attribute("value"))
                      }
                      ).ToList();
            table5 = (from animprop in animprops
                      select new Entry5
                      {
                          strProperty = CacheString(animprop.Attribute("name").Value),
                          dataType = (byte)(int)animprop.Attribute("datatype"),
                          count = (byte)animprop.Elements("change").Count(),
                          id = (int)animprop.Attribute("id"),
                          dataOffset = GetDataOffsetMulti((int)animprop.Attribute("datatype"), animprop.Elements("change").Attributes("value"))
                          // some dataOffset stuff
                      }
                      ).ToList();
            table6 = (from change in animprops.Elements("change")
                      select new Entry6
                      {
                          frame = (short)change.Attribute("frame"),
                          frameType = (byte)(int)change.Attribute("frameType"),
                          dataOffset = (int)change.Attribute("frameType") != 8 ? 0 // technically we'd want to read 64 bytes, but mehh
                            : cacheRectArray[new[] { 0f, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1 }.SelectMany(BitConverter.GetBytes).ToArray()]
                      }
                      ).ToList();

            // start offset adjustments
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

            var somecounts = guixml.Attribute("somecounts").Value.Split(',').Select(int.Parse).ToList();
            header = new Header
            {
                flag0 = (byte)(int)guixml.Attribute("flag0"),
                flag1 = (byte)(int)guixml.Attribute("flag1"),
                filenameHash = FromHex(guixml.Attribute("id").Value),
                somecount0 = somecounts[0],
                somecount1 = somecounts[1],
                somecount2 = somecounts[2],
                somecount3 = somecounts[3],
                otherFlags = (int)guixml.Attribute("otherflags"),
                otherCount = (int)guixml.Attribute("othercount"),
                table0count = table0.Count,
                table1count = table1.Count,
                table2count = table2.Count,
                table3count = table3.Count,
                table4count = table4.Count,
                table5count = table5.Count,
                table6count = table6.Count,
                table7count = table7.Count,
                table8count = table8.Count,
                table9count = table9.Count,
                table10count = table10.Count,
                table11count = table11.Count,
                table12count = table12.Count,
                table13count = table13.Count,
                table14count = table14.Count,
                table15count = table15.Count,
                table16count = table16.Count,
                table17count = table17.Count,
                table18count = table18.Count,
                table19count = table19.Count,
                table20count = table20.Count,
                table21count = table21.Count,
                table22count = table22.Count,
                table23count = table23.Count,
                table24size = table24.Count * 52,
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
                void WriteTable<T>(ref int offset, List<T> table)
                {
                    WriteBytes(ref offset, table.Select(item => item.StructToArray()).ToArray());
                }

                WriteBytes(ref header.zero3, header.StructToArray());
                WriteTable(ref header.table0offset, table0);
                WriteTable(ref header.table1offset, table1);
                WriteTable(ref header.table2offset, table2);
                WriteTable(ref header.table3offset, table3);
                WriteTable(ref header.table4offset, table4);
                WriteTable(ref header.table5offset, table5);
                WriteTable(ref header.table7offset, table7);
                WriteTable(ref header.table8offset, table8);
                WriteTable(ref header.table9offset, table9);
                WriteTable(ref header.table10offset, table10);
                WriteTable(ref header.table11offset, table11);
                WriteTable(ref header.table12offset, table12);
                WriteTable(ref header.table13offset, table13);
                WriteTable(ref header.table14offset, table14);
                WriteTable(ref header.table15offset, table15);
                WriteTable(ref header.table16offset, table16);
                WriteTable(ref header.table17offset, table17);
                WriteTable(ref header.table18offset, table18);
                WriteTable(ref header.table19offset, table19);
                WriteTable(ref header.table20offset, table20);
                WriteTable(ref header.table21offset, table21);
                WriteTable(ref header.table22offset, table22);
                WriteTable(ref header.table23offset, table23);
                WriteTable(ref header.table6offset, table6);
                WriteBytes(ref header.dataBoolOffset, cacheBool.Data);
                WriteBytes(ref header.data32bitOffset, cache32bit.Data);
                WriteBytes(ref header.dataRectOffset, cacheRect.Data);
                WriteBytes(ref header.dataRectArrayOffset, cacheRectArray.Data);
                WriteBytes(ref header.dataStringOffset, cacheString.Data);
                WriteTable(ref header.table24offset, table24);
                WriteBytes(ref header.filesize, new byte[0]);
                bw.BaseStream.Position = 0;
                WriteBytes(ref header.zero3, header.StructToArray());
                filedata = ((MemoryStream)bw.BaseStream).ToArray();
            }
        }
    }
}
