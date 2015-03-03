using System.Collections.Generic;
using System.Xml.Serialization;

namespace ConvertJsonTool
{
    [XmlRoot("ExpressionGroup")]
    public class ExpressionGroup
    {
        [XmlAttribute]
        public LogicalOperators LogicalOperator { get; set; }

        [XmlElement(ElementName = "Expression", Type = typeof(Expression))]
        public List<Expression> Expressions { get; set; }

        [XmlElement(ElementName = "ExpressionGroup", Type = typeof(ExpressionGroup))]
        public List<ExpressionGroup> ExpressionGroups { get; set; }
    }
}