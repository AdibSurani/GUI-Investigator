using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;

namespace GUI_Investigator
{
    class ARC : List<ARC.Entry>
    {
        [StructLayout(LayoutKind.Sequential)]
        class Header
        {
            public int magic;
            public short version;
            public short entryCount;
            int padding;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class FileMetadata
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string filename;
            public int extensionHash;
            public int compressedSize;
            public int uncompressedSize;
            public int offset;
        }

        public class Entry
        {
            public string Filename { get; }
            public int ExtensionHash { get; }
            public byte[] Data { get; }
            public Entry(FileMetadata metadata, byte[] data)
            {
                Filename = metadata.filename;
                ExtensionHash = metadata.extensionHash;
                Data = data;
            }
        }

        public ARC(string filename)
        {
            using (var br = new BinaryReader(File.OpenRead(filename)))
            {
                var header = br.ReadStruct<Header>();
                var lst = Enumerable.Range(0, header.entryCount).Select(_ => br.ReadStruct<FileMetadata>()).ToList();
                AddRange(lst.Select(metadata => new Entry(metadata, GetDecompressedStream(metadata.offset))));

                byte[] GetDecompressedStream(int offset)
                {
                    br.BaseStream.Position = offset + 2;
                    var ms = new MemoryStream();
                    using (var ds = new DeflateStream(br.BaseStream, CompressionMode.Decompress, true))
                    {
                        ds.CopyTo(ms);
                    }
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
        }
    }
}
