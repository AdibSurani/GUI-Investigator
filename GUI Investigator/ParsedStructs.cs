/*
 * NOT CURRENTLY IN USE
 * 
 * 
 * 
 * 
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI_Investigator
{
    class Property
    {
        public int datatype;
        public string name;
        public string value;
    }

    class AnimatedProperty
    {
        public int id;
        public int datatype;
        public string name;
        public List<Change> changes;

        public class Change
        {
            public int frame;
            public int frameType;
            public string value;
        }
    }

    class Event
    {
        public int id;
        public int type;
        public string name;
        public int t9entry;
        public int? unk0;
        public int? unk1;
        public int? unk2;
        public int? unk3;
        public int? maxframes;
        public List<AnimatedProperty> animprops;
    }

    class Pane
    {
        public int id;
        public int type;
        public string name;
        public int next;
        public int child;
        public List<Property> props;
        public List<AnimatedProperty> animprops;
    }

    class Anim
    {
        public int id;
        public string name;
        public int table2subcount; // can be discovered?
        public List<Sequence> seqs;
        public List<AnimPane> panes;


        public class Sequence
        {
            public int id;
            public int maxframes;
            public string name;
        }

        public class AnimPane : Pane
        {
            public List<Rectangle> maps;
            public int? something5;
            public List<State> states;

            public class State
            {
                public string sequencename; // not really required
                public int maxframes;
                public int unk0;
                public int unk1;
                public List<Property> props;
                public List<AnimatedProperty> animprops;
            }
        }
    }

    class Parsed11 { public int id; }
    class Parsed15 { public int id, unk; }
    class Parsed16 { public int unk0, unk1, unk2, unk3; }
    class Parsed17 { public int id, id2, varHash; public string name; }
    class Parsed18 { public int id, width, height; public float sclX, sclY; public string name, path; }
    class Parsed19 { public string path; }
    class Parsed20 { public int unk0, unk1, unk2, unk3, unkHash; }
    class Parsed22 { public int unk; public string path; }
    class Parsed24 { public Rectangle dst, src; } // how to serialize these properly??

    class ParsedGUI
    {
        public string filename;
        public int id;
        public int flag0, flag1, otherflags;
        public int somecount0, somecount1, somecount2, somecount3, othercount;
        public List<Anim> anims;
        public List<Pane> panes;
        public List<Event> events;
        public List<Parsed11> parsed11;
        public List<Parsed15> parsed15;
        public List<Parsed16> parsed16;
        public List<Parsed17> parsed17;
        public List<Parsed18> parsed18;
        public List<Parsed19> parsed19;
        public List<Parsed20> parsed20;
        public List<Parsed22> parsed22;
        public List<Parsed24> parsed24;
    }
}
