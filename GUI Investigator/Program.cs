using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI_Investigator
{
    class Program
    {
        static IEnumerable<(string filename, Stream stream)> GUIs(string romfsPath)
        {
            foreach (var path in Directory.GetFiles(Path.Combine(romfsPath, "archive")))
            {
                using (var arc = new ARC(path))
                {
                    foreach (var entry in arc.Entries)
                    {
                        if (entry.extensionHash != 0x22948394) continue;
                        yield return (Path.GetFileName(entry.filename), arc[entry]);
                    }
                }
            }
            foreach (var path in Directory.GetFiles(Path.Combine(romfsPath, "UI"), "*.gui", SearchOption.AllDirectories))
            {
                yield return (Path.GetFileNameWithoutExtension(path), File.OpenRead(path));
            }

        }

        static void Main(string[] args)
        {
            foreach (var gui in GUIs(@"C:\Users\Adib\Documents\Team-If-DGS\ExtractedRomFS"))
            {
                //if (gui.filename != "backlog")
                if (gui.filename != "title_chapter")
                {
                    continue;
                }

                //if (gui.filename != " ") continue;
                //if (gui.filename != "sys_msg_jpn") continue;
                //if (gui.filename != "invest_cursor") continue;
                //Debug.WriteLine(gui.filename);
                new GUI(gui.filename, gui.stream);
            }
            Console.WriteLine("The end");
        }
    }
}
