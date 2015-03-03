using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ConvertJsonTool
{
    public class EnumMetadata
    {
        public static List<EnumMetadata> GetAllEnumMetadata()
        {
            var metadata = new List<EnumMetadata>();
            
            var enumType = typeof (LogicalOperators);
            var values = Enum.GetValues(enumType);
            var enumMeta = new EnumMetadata
                               {
                                   EnumMembers = new List<EnumMemberMetadata>(),
                                   EnumType = enumType.Name
                               };
            foreach (var value in values)
            {
                enumMeta.EnumMembers.Add(new EnumMemberMetadata
                                {
                                    Name = value.ToString(),
                                    Value = Convert.ToInt32(value).ToString()
                                });
            }

            metadata.Add(enumMeta);
            
            enumType = typeof (ValueCompareOperators);
            values = Enum.GetValues(enumType);
            enumMeta = new EnumMetadata
                           {
                               EnumMembers = new List<EnumMemberMetadata>(),
                               EnumType = enumType.Name
                           };
            foreach (var value in values)
            {
                enumMeta.EnumMembers.Add(new EnumMemberMetadata()
                                {
                                    Name = value.ToString(),
                                    Value = Convert.ToInt32(value).ToString()
                                });
            }

            metadata.Add(enumMeta);
            
            return metadata;
        }

        public string EnumType { get; set; }

        public List<EnumMemberMetadata> EnumMembers { get; set; }
    }
}
