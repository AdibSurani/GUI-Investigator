using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GUI_Investigator
{
    class Program
    {
        // Very intensive testing -- converts all guis (including arcs) -> class -> XML -> class -> gui. Testing for a byte-for-byte match
        static void Test1()
        {
            const string romfsPath = @"C:\Users\Adib\Documents\Team-If-DGS\ExtractedRomFS";
            var arcGUIs = from path in Directory.GetFiles(Path.Combine(romfsPath, "archive"))
                          from entry in new ARC(path)
                          where entry.ExtensionHash == 0x22948394
                          select entry.Data;
            var rawGUIs = from path in Directory.GetFiles(Path.Combine(romfsPath, "UI"), "*.gui", SearchOption.AllDirectories)
                          select File.ReadAllBytes(path);
            foreach (var bytes in arcGUIs.Concat(rawGUIs))
            {
                Debug.WriteLine(GUI.FromXmlString(GUI.FromByteArray(bytes).ToXmlString()).ToByteArray().SequenceEqual(bytes));
            }
        }

        // Lighter testing with slightly more verbosity
        static void Test2()
        {
            foreach (var path in Directory.GetFiles(@"C:\Users\Adib\Desktop\guis\"))
            {
                var bytes = File.ReadAllBytes(path);
                var gui = GUI.FromByteArray(bytes);
                Debug.WriteLine($"{gui.filenameHash:X8}\t{Path.GetFileName(path)}");

                var gui2 = GUI.FromXmlString(gui.ToXmlString());
                Debug.Assert(gui2.ToByteArray().SequenceEqual(bytes));
            }
        }

        static void Main(string[] args)
        {
            //Test1();
            //Test2();
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
