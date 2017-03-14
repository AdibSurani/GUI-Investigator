using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GUI_Investigator
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle : IXmlSerializable// Note: This can also be a colour. XmlConvert is used to preserve negative zeros
    {
        public float X0, Y0, X1, Y1;
        public override string ToString() => string.Join(",", new[] { X0, Y0, X1, Y1 }.Select(XmlConvert.ToString));
        public static Rectangle Parse(string s)
        {
            var fs = s.Split(',').Select(XmlConvert.ToSingle).ToList();
            return new Rectangle { X0 = fs[0], Y0 = fs[1], X1 = fs[2], Y1 = fs[3] };
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            this = Parse(reader.ReadString());
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(ToString());
        }
    }
}
