using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace CodeCenter.Common
{
    public class Commons
    {
        private static Dictionary<string, string> MapNamesDic = new Dictionary<string, string>();

        static Commons()
        {
            try
            {
                var section = ConfigurationManager.GetSection("NameMapping") as Hashtable;
                if (section != null)
                {
                    foreach (string key in section.Keys)
                    {
                        object value = section[key];
                        if (value != null)
                        {
                            MapNamesDic.Add(key, value.ToString());
                        }
                    }
                }
            }
            catch { }
        }

        public static string GetUserName()
        {
            string userID = System.Environment.GetEnvironmentVariable("USERNAME");
            if (!string.IsNullOrEmpty(userID) && MapNamesDic.Keys.Contains(userID.ToLowerInvariant()))
            {
                return MapNamesDic[userID.ToLowerInvariant()];
            }
            return "Sure.J.Deng";
        }
    }
}
