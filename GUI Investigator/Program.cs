using System;
using System.IO;
using System.Linq;

namespace GUI_Investigator
{
    class Program
    {
        const string romfsPath = @"C:\Users\Adib\Documents\Team-If-DGS\ExtractedRomFS";

        // Very intensive testing -- converts all guis (including arcs) -> class -> XML -> class -> gui. Testing for a byte-for-byte match
        static void TestGui2XmlConversion()
        {
            var arcGUIs = from path in Directory.GetFiles(Path.Combine(romfsPath, "archive"))
                          from entry in new ARC(File.OpenRead(path))
                          where Path.GetExtension(entry.Filename) == ".gui"
                          select entry.Data;
            var rawGUIs = from path in Directory.GetFiles(Path.Combine(romfsPath, "UI"), "*.gui", SearchOption.AllDirectories)
                          select File.ReadAllBytes(path);
            foreach (var bytes in arcGUIs.Concat(rawGUIs))
            {
                var bytes2 = GUI.FromXmlString(GUI.FromByteArray(bytes).ToXmlString()).ToByteArray();
                if (!bytes.SequenceEqual(bytes2)) throw new Exception("Byte sequences are not the same");
            }
        }

        // ARC saving feature -- most of them should save back to an exact copy
        static void TestResaving()
        {
            int success = 0, failed = 0;
            foreach (var path in Directory.GetFiles(Path.Combine(romfsPath, "archive")))
            {
                var arc = new ARC(File.OpenRead(path));
                var ms = new MemoryStream();
                arc.Save(ms, true);
                if (new FileInfo(path).Length != ms.Length) throw new Exception("File lengths are not the same");
                if (File.ReadAllBytes(path).SequenceEqual(ms.ToArray()))
                {
                    success++;
                }
                else
                {
                    failed++;
                }
            }
            Console.WriteLine($"PASS: {success}");
            Console.WriteLine($"FAIL: {failed}");
        }

        static void Main(string[] args)
        {
            //TestGui2XmlConversion();
            //TestResaving();
            //return;
            foreach (var path in args)
            {
                switch (Path.GetExtension(path).ToLower())
                {
                    case ".gui":
                        File.WriteAllText(path + ".xml", GUI.FromByteArray(File.ReadAllBytes(path)).ToXmlString());
                        break;
                    case ".xml":
                        File.WriteAllBytes(path + ".gui", GUI.FromXmlString(File.ReadAllText(path)).ToByteArray());
                        break;
                }
            }

            
        }
    }
}
