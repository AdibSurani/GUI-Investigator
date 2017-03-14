using System.IO;
using System.Linq;

namespace GUI_Investigator
{
    class Program
    {
        static void Main(string[] args)
        {
            //const string romfsPath = @"C:\Users\Adib\Documents\Team-If-DGS\ExtractedRomFS";
            //var arcGUIs = from path in Directory.GetFiles(Path.Combine(romfsPath, "archive"))
            //              from entry in new ARC(path)
            //              where entry.ExtensionHash == 0x22948394
            //              select new { name = Path.GetFileName(entry.Filename), stream = entry.Stream };
            //var rawGUIs = from path in Directory.GetFiles(Path.Combine(romfsPath, "UI"), "*.gui", SearchOption.AllDirectories)
            //              select new { name = Path.GetFileNameWithoutExtension(path), stream = (Stream)File.OpenRead(path) };
            //foreach (var gui in arcGUIs.Concat(rawGUIs))
            //{
            //    gui.stream.CopyTo(File.Create(@"C:\Users\Adib\Desktop\guis\" + gui.name + ".gui"));
            //    new GUI(gui.name, gui.stream);
            //}

            foreach (var path in Directory.GetFiles(@"C:\Users\Adib\Desktop\guis\"))
            {
                var name = Path.GetFileNameWithoutExtension(path);
                //if (name != "title_chapter") continue;
                new GUI(name, File.ReadAllBytes(path));
            }
        }
    }
}
