using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using static System.Linq.Enumerable;

namespace GUI_Investigator
{
    class GUI
    {
        public GUI(string filename, byte[] bytes)
        {
            var header = Read<Header>(0);
            var spl = Encoding.ASCII.GetString(bytes, header.dataStringOffset, header.table24offset - header.dataStringOffset).Split('\0');
            var dicString = spl.Select((str, i) => Tuple.Create(spl.Take(i).Sum(s => s.Length + 1), str)).ToDictionary(p => p.Item1, p => p.Item2);
            Debug.WriteLine($"{header.filenameHash:X8}\t{filename}");

            #region a bunch of local functions
            T Read<T>(int baseOffset, int itemOffset = 0)
            {
                return bytes.ToStruct<T>(baseOffset, itemOffset);
            }

            List<T> ReadMultiple<T>(int offset, int count)
            {
                return Range(0, count).Select(i => Read<T>(offset, i)).ToList();
            }

            string GetData(int dataType, int dataOffset, int extraOffset = 0)
            {
                dataOffset += (dataType == 3 ? 1 : dataType == 4 ? 16 : 4) * extraOffset;
                switch (dataType)
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

            int[] GetMiscInt(int offset)
            {
                return new[] { Read<int>(header.dataRectArrayOffset + offset) };
            }

            List<Rectangle> GetRectList(int offset)
            {
                return Range(0, Read<int>(header.dataRectArrayOffset + offset)).Select(i => Read<Rectangle>(header.dataRectArrayOffset + offset + 8, i)).ToList();
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

            var gui = new ParsedGUI
            {
                filename = filename,
                id = header.filenameHash, // hash
                flag0 = header.flag0,
                flag1 = header.flag1,
                otherflags = header.otherFlags,
                somecount0 = header.somecount0,
                somecount1 = header.somecount1,
                somecount2 = header.somecount2,
                somecount3 = header.somecount3,
                othercount = header.otherCount,
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
                                          something5 = e2val == null ? (int?)null : e2val[0],
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
                              unk0 = e9 == null ? (int?)null : e9.unk0,
                              unk1 = e9 == null ? (int?)null : e9.unk1,
                              unk2 = e9 == null ? (int?)null : e9.unk2,
                              unk3 = e9 == null ? (int?)null : e9.unk3,
                              maxframes = e9 == null ? (int?)null : e9.maxframes,
                              animprops = e9 == null ? null : Range(e9.table5start, e9.table5count).Select(GetAnimatedProperty).ToList()
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
                             animprop = GetAnimatedProperty(header.table5subcount + n7)
                         }).ToList(),
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
                                width = e18.width,
                                height = e18.height,
                                sclX = e18.scaleX,
                                sclY = e18.scaleY,
                                name = dicString[e18.strName],
                                path = e18.strPath == -1 ? null : dicString[e18.strPath]
                            }).ToList(),
                parsed19 = (from e19 in ReadMultiple<Entry19>(header.table19offset, header.table19count)
                            select new Parsed19 { path = dicString[e19.strPath] }).ToList(),
                parsed20 = (from e20 in ReadMultiple<Entry20>(header.table20offset, header.table20count)
                            select new Parsed20 { unkHash = e20.unkHash, unk0 = e20.unk0, unk1 = e20.unk1, unk2 = e20.unk2, unk3 = e20.unk3 }).ToList(), //hash
                parsed22 = (from e22 in ReadMultiple<Entry22>(header.table22offset, header.table22count)
                            select new Parsed22 { unk = e22.unk, path = dicString[e22.strPath] }).ToList(),
                parsed24 = (from e24 in ReadMultiple<Entry24>(header.table24offset, header.table24size / 52)
                            select new Parsed24 { dst = e24.dst, src = e24.src }).ToList()
            };

            var recon = new Reconstruction(gui);
            Debug.Assert(recon.filedata.SequenceEqual(bytes));
        }

    }
}
