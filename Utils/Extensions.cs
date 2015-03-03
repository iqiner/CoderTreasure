using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCenter.Common.Extensions
{
    public static class Extensions
    {
        public static bool IsNullOrEmptyCollection<T>(this IEnumerable<T> list)
        {
            if (list == null || list.Count() == 0)
            {
                return true;
            }
            return false;
        }
    }
}
