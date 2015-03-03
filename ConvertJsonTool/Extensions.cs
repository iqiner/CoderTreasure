using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConvertJsonTool
{
    public static class Extensions
    {
        public static IList<string> SplitString(this string str, bool removeWhiteSpace = true, char seperator = ',')
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return new List<string>();
            }

            var strs = str.Split(seperator);
            if (!strs.Any())
            {
                return new List<string>();
            }

            var list = new List<string>();
            foreach (var s in strs)
            {
                if (removeWhiteSpace)
                {
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        list.Add(s.Trim());
                    }
                }
                else
                {
                    list.Add(s);
                }
            }
            return list;
        }
    }
}
