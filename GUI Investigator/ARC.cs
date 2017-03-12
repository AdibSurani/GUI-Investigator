using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace GUI_Investigator
{
    class ARC : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        class Header
        {
            public int magic;
            public short version;
            public short entries;
            int padding;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class FileEntry
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string filename;
            public uint extensionHash;
            public int compressedSize;
            public int uncompressedSize;
            public int offset;
        }

        FileStream stream;

        public void Dispose() => stream.Dispose();
        public ARC(string filename) => stream = File.OpenRead(filename);

        public IEnumerable<FileEntry> Entries
        {
            get
            {
                stream.Position = 0;
                using (var br = new BinaryReader(stream, Encoding.Default, true))
                {
                    var header = br.ReadStruct<Header>();
                    return br.ReadMultiple<FileEntry>(header.entries).ToList();
                }
            }
        }

        public MemoryStream this[FileEntry entry]
        {
            get
            {
                stream.Position = entry.offset;
                var ms = new MemoryStream();
                using (var ds = new InflaterInputStream(stream) { IsStreamOwner = false })
                {
                    ds.CopyTo(ms);
                }
                ms.Position = 0;
                return ms;
            }
        }
    }
}
