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
            public Stream Stream { get; }
            public Entry(FileMetadata metadata, Stream stream)
            {
                Filename = metadata.filename;
                ExtensionHash = metadata.extensionHash;
                Stream = stream;
            }
        }

        public ARC(string filename)
        {
            using (var br = new BinaryReader(File.OpenRead(filename)))
            {
                var header = br.ReadStruct<Header>();
                var lst = br.ReadMultiple<FileMetadata>(12, header.entryCount).ToList();
                AddRange(lst.Select(metadata => new Entry(metadata, GetDecompressedStream(metadata.offset))));

                MemoryStream GetDecompressedStream(int offset)
                {
                    br.BaseStream.Position = offset + 2;
                    var ms = new MemoryStream();
                    using (var ds = new DeflateStream(br.BaseStream, CompressionMode.Decompress, true))
                    {
                        ds.CopyTo(ms);
                    }
                    ms.Position = 0;
                    return ms;
                }
            }
        }
    }
}
