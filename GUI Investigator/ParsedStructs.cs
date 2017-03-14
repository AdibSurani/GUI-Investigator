using System.Collections.Generic;
using System.Xml.Serialization;

namespace GUI_Investigator
{
    public class Property
    {
        [XmlAttribute]
        public int datatype;
        [XmlAttribute]
        public string name, value;
    }

    public class AnimatedProperty
    {
        [XmlAttribute]
        public int id, datatype;
        [XmlAttribute]
        public string name;
        [XmlElement("change")]
        public List<Change> changes;

        public class Change
        {
            [XmlAttribute]
            public int frame, frameType;
            [XmlAttribute]
            public string value;
        }
    }

    public class Event
    {
        [XmlAttribute]
        public int id, type, t9entry;
        [XmlAttribute]
        public string name;
        public T9Values t9values;

        public class T9Values
        {
            [XmlAttribute]
            public int unk0, unk1, unk2, unk3, maxframes;
            [XmlElement("animprop")]
            public List<AnimatedProperty> animprops;
        }
    }

    public class Pane
    {
        [XmlAttribute]
        public int id, type, next, child;
        [XmlAttribute]
        public string name;
        [XmlElement("prop")]
        public List<Property> props;
        public AnimatedProperty animprop;
    }

    public class Anim
    {
        [XmlAttribute]
        public int id, table2subcount; // can be discovered?
        [XmlAttribute]
        public string name;
        [XmlElement("seq")]
        public List<Sequence> seqs;
        [XmlElement("pane")]
        public List<AnimPane> panes;

        public class Sequence
        {
            [XmlAttribute]
            public int id, maxframes;
            [XmlAttribute]
            public string name;
        }

        public class AnimPane : Pane
        {
            [XmlElement]
            public int[] something5;
            [XmlElement("map")]
            public List<Rectangle> maps;
            [XmlElement("state")]
            public List<State> states;

            public class State
            {
                [XmlAttribute]
                public string sequencename; // not really required
                [XmlAttribute]
                public int unk0, unk1, maxframes;
                [XmlElement("prop")]
                public List<Property> props;
                [XmlElement("animprop")]
                public List<AnimatedProperty> animprops;
            }
        }
    }

    public class Parsed11 {[XmlAttribute] public int id; }
    public class Parsed15 {[XmlAttribute] public int id, unk; }
    public class Parsed16 {[XmlAttribute] public int unk0, unk1, unk2, unk3; }
    public class Parsed17 {[XmlAttribute] public int id, id2, varHash;[XmlAttribute] public string name; }
    public class Parsed18 {[XmlAttribute] public int id, width, height;[XmlAttribute] public float sclX, sclY;[XmlAttribute] public string name, path; }
    public class Parsed19 {[XmlAttribute] public string path; }
    public class Parsed20 {[XmlAttribute] public int unk0, unk1, unk2, unk3, unkHash; }
    public class Parsed22 {[XmlAttribute] public int unk;[XmlAttribute] public string path; }
    public class Parsed24 { public Rectangle dst, src; } // @todo: figure out how to XmlAttribute these

    public class Unknown
    {
        [XmlElement]
        public List<Parsed11> parsed11;
        [XmlElement]
        public List<Parsed15> parsed15;
        [XmlElement]
        public List<Parsed16> parsed16;
        [XmlElement]
        public List<Parsed17> parsed17;
        [XmlElement]
        public List<Parsed18> parsed18;
        [XmlElement]
        public List<Parsed19> parsed19;
        [XmlElement]
        public List<Parsed20> parsed20;
        [XmlElement]
        public List<Parsed22> parsed22;
        [XmlElement]
        public List<Parsed24> parsed24;
    }
}
