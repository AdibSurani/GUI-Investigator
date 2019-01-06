using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GUI_Investigator
{
    class ARC : List<ARC.Entry>
    {
        [StructLayout(LayoutKind.Sequential)]
        class Header
        {
            public int magic = 0x435241;
            public short version = 0x11;
            public short entryCount;
            int padding;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class FileMetadata
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string filename;
            public uint extensionHash;
            public int compressedSize;
            public int uncompressedSize;
            public int offset;
        }

        public class Entry
        {
            public CompressionLevel CompressionLevel { get; set; }
            public byte[] Data { get; set; }
            public string Filename { get; set; }
        }

        public static uint GetHash(string s) => new BitArray(s.Select(x => (byte)x).ToArray()).Cast<bool>()
            .Aggregate(~0u, (h, i) => h / 2 ^ (i ^ h % 2 != 0 ? 0xEDB88320 : 0)) * 2 / 2;

        public static Dictionary<uint, string> ExtensionMap = new Dictionary<uint, string>
        {
            [GetHash("rAIFSM")] = ".xfsa",
            [GetHash("rCameraList")] = ".lcm",
            [GetHash("rCharacter")] = ".xfsc",
            [GetHash("rCollision")] = ".sbc",
            [GetHash("rEffect2D")] = ".e2d",
            [GetHash("rEffectAnim")] = ".ean",
            [GetHash("rEffectList")] = ".efl",
            [GetHash("rGUI")] = ".gui",
            [GetHash("rGUIFont")] = ".gfd",
            [GetHash("rGUIIconInfo")] = ".gii",
            [GetHash("rGUIMessage")] = ".gmd",
            [GetHash("rHit2D")] = ".xfsh",
            [GetHash("rLayoutParameter")] = ".xfsl",
            [GetHash("rMaterial")] = ".mrl",
            [GetHash("rModel")] = ".mod",
            [GetHash("rMotionList")] = ".lmt",
            [GetHash("rPropParam")] = ".prp",
            [GetHash("rScheduler")] = ".sdl",
            [GetHash("rSoundBank")] = ".sbkr",
            [GetHash("rSoundRequest")] = ".srqr",
            [GetHash("rSoundSourceADPCM")] = ".mca",
            [GetHash("rTexture")] = ".tex"
        };

        public ARC(Stream input, bool leaveOpen = false)
        {
            using (var br = new BinaryReader(input, Encoding.Default, leaveOpen))
            {
                var header = br.ReadStruct<Header>();
                var lst = Enumerable.Range(0, header.entryCount).Select(_ => br.ReadStruct<FileMetadata>()).ToList();
                AddRange(lst.Select(metadata =>
                {
                    // zlib header
                    br.BaseStream.Position = metadata.offset + 1;
                    var level = br.ReadByte();

                    // deflate stream
                    var ms = new MemoryStream();
                    using (var ds = new DeflateStream(br.BaseStream, CompressionMode.Decompress, true))
                    {
                        ds.CopyTo(ms);
                    }

                    // ignore adler32 footer, assume checksum is correct

                    return new Entry
                    {
                        CompressionLevel = level == 0x9C ? CompressionLevel.Optimal : CompressionLevel.NoCompression,
                        Data = ms.ToArray(),
                        Filename = metadata.filename + ExtensionMap[metadata.extensionHash]
                    };
                }));
            }
        }

        public void Save(Stream output, bool leaveOpen = false)
        {
            var header = new Header { entryCount = (short)Count };
            var compressedList = this.Select(e =>
            {
                using (var bw = new BinaryWriter(new MemoryStream()))
                {
                    // zlib header
                    bw.Write((short)(e.CompressionLevel == CompressionLevel.Optimal ? 0x9C78 : 0x0178));

                    // deflate stream
                    using (var ds = new DeflateStream(bw.BaseStream, e.CompressionLevel, true))
                    {
                        ds.Write(e.Data, 0, e.Data.Length);
                    }

                    // adler32 footer
                    var (a, b) = e.Data.Aggregate((1, 0), (x, n) => ((x.Item1 + n) % 65521, (x.Item1 + x.Item2 + n) % 65521));
                    bw.Write(new[] { (byte)(b >> 8), (byte)b, (byte)(a >> 8), (byte)a });
                    return ((MemoryStream)bw.BaseStream).ToArray();
                }
            }).ToList();

            using (var bw = new BinaryWriter(output, Encoding.Default, leaveOpen))
            {
                bw.Write(header.StructToArray());
                int padding = Count % 2 == 0 ? 20 : 4;
                for (int i = 0; i < Count; i++)
                {
                    var ext = Path.GetExtension(this[i].Filename);
                    bw.Write(new FileMetadata
                    {
                        filename = this[i].Filename.Remove(this[i].Filename.Length - ext.Length),
                        extensionHash = ExtensionMap.Single(pair => pair.Value == ext).Key,
                        compressedSize = compressedList[i].Length,
                        uncompressedSize = this[i].Data.Length | 0x40000000,
                        offset = 12 + Count * 80 + padding + compressedList.Take(i).Sum(bytes => bytes.Length)
                    }.StructToArray());
                }
                bw.Write(new byte[padding]);
                foreach (var bytes in compressedList)
                {
                    bw.Write(bytes);
                }
            }
        }
    }
}
