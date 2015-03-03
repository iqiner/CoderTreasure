using System.Xml.Serialization;

namespace ConvertJsonTool
{
    [XmlRoot("EnumMember")]
    public class EnumMemberMetadata
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Value { get; set; }
    }
}
