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

        public Reconstruction(int flag0)
        {
            cache32bit.hax = flag0 == 0 ? 8 : 4;
            cacheRectArray = new Cache<object>(ParseRectArray);
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
                default: return GetOffset(attrs.Select(attr => (int)attr));
            }
        }

        public void Parse(XElement gui)
        {
            table0 = (from anim in gui.Elements("anim")
                      select new Entry0 { strName = GetOffset(anim.Attribute("name").Value) }
                      ).ToList();
            table1 = (from seq in gui.Elements("anim").Elements("sequence")
                      select new Entry1 { strName = GetOffset(seq.Attribute("name").Value) }
                      ).ToList();
            table2 = (from pane in gui.Elements("anim").Elements("pane")
                      select new Entry2 { strName = GetOffset(pane.Attribute("name").Value) }
                      ).ToList();
            table7 = (from pane in gui.Elements("pane")
                      select new Entry7 { strName = GetOffset(pane.Attribute("name").Value) }
                      ).ToList();
            table8 = (from evt in gui.Elements("event")
                      select new Entry8 { strName = GetOffset(evt.Attribute("name").Value) }
                      ).ToList();
            // missing: table17
            table18 = (from e18 in gui.Element("misc").Elements("e18")
                       select new Entry18 { strPath = GetOffset(e18.Attribute("path").Value), strName = GetOffset(e18.Attribute("name").Value) }
                       ).ToList();

            // missing: table19, table22
            table19 = (from e19 in gui.Element("misc").Elements("e19")
                       select new Entry19 { strPath = GetOffset(e19.Attribute("path").Value) }
                       ).ToList();
            table22 = (from e22 in gui.Element("misc").Elements("e19")
                       select new Entry22 { strPath = GetOffset(e22.Attribute("path").Value) }
                       ).ToList();
            table4 = (from prop in gui.Elements("anim").Elements("pane").Elements("property")
                                    .Concat(gui.Elements("anim").Elements("pane").Elements("state").Elements("property"))
                                    .Concat(gui.Elements("pane").Elements("property"))
                      select new Entry4
                      {
                          strProperty = GetOffset(prop.Attribute("name").Value),
                          dataType = (int)prop.Attribute("datatype"),
                          dataOffset = GetDataOffset((int)prop.Attribute("datatype"), prop.Attribute("value"))
                      }
                      ).ToList();
            table5 = (from animprop in gui.Elements("anim").Elements("pane").Elements("state").Elements("animatedproperty")
                                        .Concat(gui.Elements("event").Elements("animatedproperty"))
                                        .Concat(gui.Elements("pane").Elements("animatedproperty"))
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

            foreach (var anim in gui.Elements("anim"))
            {
                foreach (var seq in anim.Elements("sequence"))
                {
                }
                foreach (var pane in anim.Elements("pane"))
                {
                    foreach (var map in pane.Elements("map"))
                    {
                    }
                    foreach (var prop in pane.Elements("property"))
                    {
                    }
                    foreach (var state in pane.Elements("state"))
                    {
                        foreach (var prop in state.Elements("property"))
                        {
                        }
                        foreach (var animprop in state.Elements("animatedproperty"))
                        {
                            foreach (var change in animprop.Elements("change"))
                            {
                            }
                        }
                    }
                }
                int k = 1;
            }
            foreach (var pane in gui.Elements("pane"))
            {
                foreach (var prop in pane.Elements("property"))
                {
                }
            }
            foreach (var evt in gui.Elements("event"))
            {
                foreach (var animprop in evt.Elements("animatedproperty"))
                {
                    foreach (var change in animprop.Elements("change"))
                    {
                    }
                }
            }
        }
    }
}
