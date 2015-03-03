using System.Xml.Serialization;

namespace ConvertJsonTool
{
    public class Expression
    {
        [XmlAttribute]
        public string Variable { get; set; }

        [XmlAttribute]
        public ValueCompareOperators ValueComparator { get; set; }

        [XmlAttribute]
        public string Value { get; set; }
    }
}