using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GenerateDHLGlobalMailRateAndZoneXML
{
    public abstract class ExportBase
    {
        private string sourceFileDir;
        private string xmlFileDir;
        private string currentProcessFileName;

        public ExportBase(string sourceFileDir, string xmlFileDir)
        {
            this.sourceFileDir = sourceFileDir;
            this.xmlFileDir = xmlFileDir;
        }

        protected abstract string ExportSheetName { get; }

        public string CurrentProcessFileName
        {
            get { return this.currentProcessFileName; }
        }

        public void Export()
        {
            if (!Directory.Exists(sourceFileDir))
            {
                Directory.CreateDirectory(sourceFileDir);
                throw new ApplicationException("Can not find any source file >> " + sourceFileDir);
            }

            string[] files = Directory.GetFiles(sourceFileDir);
            foreach (string fileName in files)
            {
                this.currentProcessFileName = Path.GetFileNameWithoutExtension(fileName);
                string xml = string.Empty;

                using (OleDbConnection conn = new OleDbConnection(GetExcelConnectionString(fileName)))
                {
                    conn.Open();
                    OleDbDataAdapter adapter = new OleDbDataAdapter(string.Format("Select * from {0}", this.ExportSheetName), conn);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        xml = GenerateXML(ds);
                    }
                    conn.Close();
                }

                if (!string.IsNullOrWhiteSpace(xml))
                {
                    this.SaveFile(xmlFileDir, this.currentProcessFileName, xml);
                }
            }
        }

        protected abstract string GenerateXML(DataSet ds);

        private void SaveFile(string destinationDirectory, string fileName, string content)
        {
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }
            string fullPath = Path.Combine(destinationDirectory, fileName + ".xml");
            Console.WriteLine("Save connectship xml file: " + fullPath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(content);
            using (XmlTextWriter writer = new XmlTextWriter(fullPath, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                xmlDoc.WriteTo(writer);
                writer.Flush();
            }
        }

        protected virtual string GetExcelConnectionString(string excelFileName)
        {
            return string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=""Excel 8.0;""", excelFileName);
        }
    }

}
