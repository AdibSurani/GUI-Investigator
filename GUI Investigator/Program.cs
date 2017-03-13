using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GUI_Investigator
{
    class Program
    {
        static IEnumerable<Tuple<string, Stream>> GUIs(string romfsPath)
        {
            foreach (var path in Directory.GetFiles(Path.Combine(romfsPath, "archive")))
            {
                using (var arc = new ARC(path))
                {
                    foreach (var entry in arc.Entries)
                    {
                        if (entry.extensionHash != 0x22948394) continue;
                        yield return Tuple.Create(Path.GetFileName(entry.filename), (Stream)arc[entry]);
                    }
                }
            }
            foreach (var path in Directory.GetFiles(Path.Combine(romfsPath, "UI"), "*.gui", SearchOption.AllDirectories))
            {
                yield return Tuple.Create(Path.GetFileNameWithoutExtension(path), (Stream)File.OpenRead(path));
            }

        }

        static void Main(string[] args)
        {
            //File.WriteAllBytes("title_chapter.gui", new Reconstruction(XDocument.Load("title_chapter.xml").Root).filedata); return;

            foreach (var gui in GUIs(@"C:\Users\Adib\Documents\Team-If-DGS\ExtractedRomFS"))
            {
                //if (gui.filename != "backlog")
                if (gui.Item1 != "title_chapter")
                {
                    //continue;
                }

                // rectArray problems
                //if (!"sys_msg_jpn|exam_sub00|invest_evidence_sub|sys_panel03|data|title_window|file|start_production|dtc_index_sub|dtc_move_sub|together_change|together_light_3D|battle_light|battle_sub01|tutorial_sce0|tutorial_sce2".Split('|').Contains(gui.Item1)) continue;
                //if (gui.Item1 != "together_change") continue;

                // negative 0.0 floats
                //if ("invest_sub00|sys_panel00|sys_panel01|sys_panel02|sys_panel04|sys_panel05|sys_panel06|sys_panel07|sys_panel08".Split('|').Contains(gui.Item1)) continue;

                // this is a single dataOffset change
                //if (gui.Item1 == "topic") continue;
                //if (gui.Item1 == "together_cursor") continue;
                //if (gui.Item1 == "together_topic") continue;

                //if (gui.filename != " ") continue;
                //if (gui.filename != "sys_msg_jpn") continue;
                //if (gui.filename != "invest_cursor") continue;

                // invest_sub00 has a negative 0.0 float!

                //Debug.WriteLine(gui.Item1);
                new GUI(gui.Item1, gui.Item2);
            }
        }
    }
}
