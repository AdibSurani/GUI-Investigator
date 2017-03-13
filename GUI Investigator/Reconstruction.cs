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

        public int rectArrayCount = 0;
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

        public Reconstruction(XElement gui)
        {
            cacheRectArray = new Cache<object>(ParseRectArray);
            Parse(gui);
        }

        public int GetOffset(object obj)
        {
            switch (obj)
            {
                case string s: return cacheString[s];
                case bool b: return cacheBool[new[] { b }];
                case int i: return cache32bit[Convert.ToBase64String(BitConverter.GetBytes(i))];
                case float f: return cache32bit[Convert.ToBase64String(BitConverter.GetBytes(f))];
                case Rectangle r: return cacheRect[r.ToString()];
                case int[] arr: return cacheRectArray[arr];
                case RectArray lst: return cacheRectArray[lst];
                case byte[] b: return cacheRectArray[b];
                case IEnumerable<int> arr: return cache32bit[Convert.ToBase64String(arr.SelectMany(BitConverter.GetBytes).ToArray())];
                case IEnumerable<float> arr: return cache32bit[Convert.ToBase64String(arr.SelectMany(BitConverter.GetBytes).ToArray())];
                case IEnumerable<bool> arr: return cacheBool[arr.ToArray()];
                case IEnumerable<Rectangle> arr: return cacheRect[string.Join(",", arr)];
            }
            throw new NotImplementedException();
        }

        int GetDataOffset(int datatype, XAttribute attr)
        {
            switch (datatype)
            {
                case 2: return GetOffset((float)attr);
                case 3: return GetOffset((bool)attr);
                case 4: return GetOffset(Rectangle.Parse(attr.Value));
                case 17: return (bool)attr ? 1 : 0;
                case 18: return (int)attr;
                default: return GetOffset((int)attr);
            }
        }

        int GetDataOffsetMulti(int datatype, IEnumerable<XAttribute> attrs)
        {
            switch (datatype)
            {
                case 2: return GetOffset(attrs.Select(attr => (float)attr));
                case 3: return GetOffset(attrs.Select(attr => (bool)attr));
                case 4: return GetOffset(attrs.Select(attr => Rectangle.Parse(attr.Value)));
                case 15: return cache32bit.Length;
                default: return GetOffset(attrs.Select(attr => (int)attr));
            }
        }

        int FromHex(string s) => Convert.ToInt32(s, 16);

        public void Parse(XElement gui)
        {
            table0 = (from anim in gui.Elements("anim")
                      select new Entry0
                      {
                          strName = GetOffset(anim.Attribute("name").Value),
                          id = (int)anim.Attribute("id"),
                          table2subcount = (short)anim.Attribute("panesubcount"),
                          table1count = (short)anim.Elements("sequence").Count(),
                          table2count = (short)anim.Elements("pane").Count(),
                          table5count = (short)anim.Elements("pane").Elements("state").Elements("animatedproperty").Count()
                      }
                      ).ToList();
            table1 = (from seq in gui.Elements("anim").Elements("sequence")
                      select new Entry1
                      {
                          strName = GetOffset(seq.Attribute("name").Value),
                          id = (int)seq.Attribute("id"),
                          maxframes = (int)seq.Attribute("maxframes")
                      }
                      ).ToList();
            table2 = (from pane in gui.Elements("anim").Elements("pane")
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
                          strName = GetOffset(pane.Attribute("name").Value),
                            // this texture still needs some investigation
                          texture = maps.Any() || FromHex(pane.Attribute("type").Value) == 0x4F7228FC
                            ? GetOffset(new RectArray(0, maps.Select(map => Rectangle.Parse(map.Attribute("rect").Value))))
                            : something5 != null
                            ? GetOffset(new[] { (int)something5.Attribute("value") })
                            : -1
                      }
                      ).ToList();
            table3 = (from state in gui.Elements("anim").Elements("pane").Elements("state")
                      select new Entry3
                      {
                          table4count = (byte)state.Elements("property").Count(),
                          table5count = (byte)state.Elements("animatedproperty").Count(),
                          maxframes = (short)state.Attribute("maxframes"),
                          unk0 = (short)state.Attribute("unk0"),
                          unk1 = (short)state.Attribute("unk1")
                      }
                      ).ToList();
            // table6 also contributes to texture... in the sense of dataOffset
            table7 = (from pane in gui.Elements("pane")
                      select new Entry7
                      {
                          id = (int)pane.Attribute("id"),
                          next = (int)pane.Attribute("next"),
                          child = (int)pane.Attribute("child"),
                          table4count = pane.Elements("property").Count(),
                          strName = GetOffset(pane.Attribute("name").Value),
                          tagHash = FromHex(pane.Attribute("type").Value)
                      }
                      ).ToList();
            table8 = (from evt in gui.Elements("event")
                      select new Entry8
                      {
                          id = (int)evt.Attribute("id"),
                          type = (int)evt.Attribute("type"),
                          strName = GetOffset(evt.Attribute("name").Value),
                          table9entry = (int)evt.Attribute("t9entry")
                      }
                      ).ToList();
            table9 = (from evt in gui.Elements("event")
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
            table11 = (from e11 in gui.Elements("misc").Elements("e11")
                       select new Entry11
                       {
                           id = (int)e11.Attribute("id")
                       }
                       ).ToList();
            table15 = (from e15 in gui.Elements("misc").Elements("e15")
                       select new Entry15
                       {
                           id = (int)e15.Attribute("id"),
                           unk = (int)e15.Attribute("unk")
                       }
                       ).ToList();
            table16 = (from e16 in gui.Elements("misc").Elements("e16")
                       let unks = e16.Attribute("unks").Value.Split(',').Select(int.Parse).ToList()
                       select new Entry16
                       {
                           unk0 = unks[0],
                           unk1 = unks[1],
                           unk2 = unks[2],
                           unk3 = unks[3]
                       }
                       ).ToList();
            table17 = (from e17 in gui.Elements("misc").Elements("e17")
                       select new Entry17
                       {
                           id = (int)e17.Attribute("id"),
                           strName = GetOffset(e17.Attribute("name").Value),
                           varHash = FromHex(e17.Attribute("varHash").Value),
                           id2 = (int)e17.Attribute("id2")
                       }
                      ).ToList();
            table18 = (from e18 in gui.Element("misc").Elements("e18")
                       select new Entry18
                       {
                           id = (int)e18.Attribute("id"),
                           width = (short)e18.Attribute("width"),
                           height = (short)e18.Attribute("height"),
                           scaleX = (float)e18.Attribute("sclX"),
                           scaleY = (float)e18.Attribute("sclY"),
                           strPath = e18.Attribute("path") == null ? -1 : GetOffset(e18.Attribute("path").Value),
                           strName = GetOffset(e18.Attribute("name").Value)
                       }
                       ).ToList();
            table19 = (from e19 in gui.Element("misc").Elements("e19")
                       select new Entry19 { strPath = GetOffset(e19.Attribute("path").Value) }
                       ).ToList();
            table20 = (from e20 in gui.Element("misc").Elements("e20")
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
            table22 = (from e22 in gui.Element("misc").Elements("e22")
                       select new Entry22
                       {
                           unk = (int)e22.Attribute("unk"),
                           strPath = GetOffset(e22.Attribute("path").Value)
                       }
                       ).ToList();
            table24 = (from e24 in gui.Element("misc").Elements("e24")
                       select new Entry24
                       {
                           dst = Rectangle.Parse(e24.Attribute("dst").Value),
                           src = Rectangle.Parse(e24.Attribute("src").Value)
                       }
                       ).ToList();

            // now we jump onto the properties
            var props = gui.Elements("anim").Elements("pane").Elements("property")
                         .Concat(gui.Elements("anim").Elements("pane").Elements("state").Elements("property"))
                         .Concat(gui.Elements("pane").Elements("property"));
            var animprops = gui.Elements("anim").Elements("pane").Elements("state").Elements("animatedproperty")
                             .Concat(gui.Elements("event").Elements("animatedproperty"))
                             .Concat(gui.Elements("pane").Elements("animatedproperty"));

            table4 = (from prop in props
                      select new Entry4
                      {
                          strProperty = GetOffset(prop.Attribute("name").Value),
                          dataType = (int)prop.Attribute("datatype"),
                          dataOffset = GetDataOffset((int)prop.Attribute("datatype"), prop.Attribute("value"))
                      }
                      ).ToList();
            table5 = (from animprop in animprops
                      select new Entry5
                      {
                          strProperty = GetOffset(animprop.Attribute("name").Value),
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
                          dataOffset = (int)change.Attribute("frameType") != 8 ? 0
                            : GetOffset(new[] { 0f, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1 }.SelectMany(BitConverter.GetBytes).ToArray())
                          // if frameType == 8 there is a need to get 64 bytes from the texture...?
                      }
                      ).ToList();

            // start adjustments
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

            var somecounts = gui.Attribute("somecounts").Value.Split(',').Select(int.Parse).ToList();
            header = new Header
            {
                flag0 = (byte)(int)gui.Attribute("flag0"),
                flag1 = (byte)(int)gui.Attribute("flag1"),
                filenameHash = FromHex(gui.Attribute("id").Value),
                somecount0 = somecounts[0],
                somecount1 = somecounts[1],
                somecount2 = somecounts[2],
                somecount3 = somecounts[3],
                otherFlags = (int)gui.Attribute("otherflags"),
                otherCount = (int)gui.Attribute("othercount")
            };

            header.table0count = table0.Count;
            header.table1count = table1.Count;
            header.table2count = table2.Count;
            header.table3count = table3.Count;
            header.table4count = table4.Count;
            header.table5count = table5.Count;
            header.table6count = table6.Count;
            header.table7count = table7.Count;
            header.table8count = table8.Count;
            header.table9count = table9.Count;
            header.table10count = table10.Count;
            header.table11count = table11.Count;
            header.table12count = table12.Count;
            header.table13count = table13.Count;
            header.table14count = table14.Count;
            header.table15count = table15.Count;
            header.table16count = table16.Count;
            header.table17count = table17.Count;
            header.table18count = table18.Count;
            header.table19count = table19.Count;
            header.table20count = table20.Count;
            header.table21count = table21.Count;
            header.table22count = table22.Count;
            header.table23count = table23.Count;
            header.table24size = table24.Count * 52;
            header.table5subcount = header.table5count - header.table7count;
            header.table7count2 = table7.Count;

            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms, Encoding.Default, true))
            {
                bw.WritePadded(header);
                header.table0offset = (int)bw.BaseStream.Position; bw.WritePadded(table0);
                header.table1offset = (int)bw.BaseStream.Position; bw.WritePadded(table1);
                header.table2offset = (int)bw.BaseStream.Position; bw.WritePadded(table2);
                header.table3offset = (int)bw.BaseStream.Position; bw.WritePadded(table3);
                header.table4offset = (int)bw.BaseStream.Position; bw.WritePadded(table4);
                header.table5offset = (int)bw.BaseStream.Position; bw.WritePadded(table5);
                header.table7offset = (int)bw.BaseStream.Position; bw.WritePadded(table7);
                header.table8offset = (int)bw.BaseStream.Position; bw.WritePadded(table8);
                header.table9offset = (int)bw.BaseStream.Position; bw.WritePadded(table9);
                header.table10offset = (int)bw.BaseStream.Position; bw.WritePadded(table10);
                header.table11offset = (int)bw.BaseStream.Position; bw.WritePadded(table11);
                header.table12offset = (int)bw.BaseStream.Position; bw.WritePadded(table12);
                header.table13offset = (int)bw.BaseStream.Position; bw.WritePadded(table13);
                header.table14offset = (int)bw.BaseStream.Position; bw.WritePadded(table14);
                header.table15offset = (int)bw.BaseStream.Position; bw.WritePadded(table15);
                header.table16offset = (int)bw.BaseStream.Position; bw.WritePadded(table16);
                header.table17offset = (int)bw.BaseStream.Position; bw.WritePadded(table17);
                header.table18offset = (int)bw.BaseStream.Position; bw.WritePadded(table18);
                header.table19offset = (int)bw.BaseStream.Position; bw.WritePadded(table19);
                header.table20offset = (int)bw.BaseStream.Position; bw.WritePadded(table20);
                header.table21offset = (int)bw.BaseStream.Position; bw.WritePadded(table21);
                header.table22offset = (int)bw.BaseStream.Position; bw.WritePadded(table22);
                header.table23offset = (int)bw.BaseStream.Position; bw.WritePadded(table23);
                header.table6offset = (int)bw.BaseStream.Position; bw.WritePadded(table6);
                header.dataBoolOffset = (int)bw.BaseStream.Position; bw.Write(cacheBool.Data);
                header.data32bitOffset = (int)bw.BaseStream.Position; bw.Write(cache32bit.Data);
                header.dataRectOffset = (int)bw.BaseStream.Position; bw.Write(cacheRect.Data);
                header.dataRectArrayOffset = (int)bw.BaseStream.Position; bw.Write(cacheRectArray.Data);
                header.dataStringOffset = (int)bw.BaseStream.Position; bw.Write(cacheString.Data);
                header.table24offset = (int)bw.BaseStream.Position; bw.WritePadded(table24);
                header.filesize = (int)bw.BaseStream.Position;
                bw.BaseStream.Position = 0;
                bw.WritePadded(header);
            }
            filedata = ms.ToArray();
        }
    }
}
