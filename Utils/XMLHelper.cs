using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CodeCenter.Common.Utils
{
    public class XMLHelper
    {
        public static T Deserialize<T>(string path)
            where T : class
        {
            if (!File.Exists(path))
            {
                return default(T);
            }
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StreamReader reader = new StreamReader(path))
                {
                    return serializer.Deserialize(reader) as T;
                }
            }
            catch
            {
                return default(T);
            }
        }

        public static string Serialize<T>(T entity)
            where T : class
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings setting = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };
            try
            {
                using (XmlWriter writer = XmlWriter.Create(sb, setting))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(writer, entity);
                }
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
