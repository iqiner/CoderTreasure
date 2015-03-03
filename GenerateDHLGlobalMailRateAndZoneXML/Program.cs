using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;

namespace GenerateDHLGlobalMailRateAndZoneXML
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceFileDir = Path.Combine(System.Environment.CurrentDirectory, "Input");
            string xmlFileDir = Path.Combine(System.Environment.CurrentDirectory, "Output");
            try
            {
                ExportBase exportRate = new ExportRate(sourceFileDir, xmlFileDir);
                exportRate.Export();

                ExportBase exportZone = new ExportZone(sourceFileDir+"/1", xmlFileDir+"/1");
                exportZone.Export();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            Console.ReadKey();
        }

        /*
        private static Dictionary<string,int> GetValidFields(DataRow row)
        {
            Dictionary<string, int> validFields = new Dictionary<string, int>();
            for (int i = 0; i < row.Table.Columns.Count; i++)
            {
                if(row[i] == DBNull.Value )
                {
                    continue;
                }
                string fieldName = row[i].ToString();
                if (fieldName.StartsWith("WEIGHT"))
                {
                    validFields.Add("WEIGHT", i);
                }
                else if (fieldName.StartsWith("AU"))
                {
                    validFields.Add("AU", i);
                }
                else if (fieldName.StartsWith("GB"))
                {
                    validFields.Add(fieldName, i);
                }
            }
            return validFields;
        }

        private static string GetXML(Dictionary<string, Dictionary<decimal, decimal>> rateDic)
        {
            StringBuilder group = new StringBuilder();
            foreach (string key in rateDic.Keys)
            {
                StringBuilder ratelist = new StringBuilder();
                foreach (KeyValuePair<decimal, decimal> kv in rateDic[key])
                {
                    ratelist.Append(string.Format(PackageRatesTemplate, kv.Key, kv.Value).TrimEnd());
                }
                group.Append(string.Format(RateGroupsTemplate, key, ratelist.ToString().Trim()));
            }
            return string.Format(RateListTemplate, group.ToString().Trim());
        }

        private static void GenerateFile(string destinationDirectory,string fileName, string content)
        {
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }
            string fullPath = Path.Combine(destinationDirectory, fileName + ".xml");
            Console.WriteLine("Save connectship xml file: " + fullPath);
            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                writer.Write(content);
            }
        }

        public static string ExcelConnectionString(string excelFileName)
        {
            return string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=""Excel 8.0;""", excelFileName);
        }

        private static string RateListTemplate
        {
            get
            {
                return @"<rateList name="""" wtUnit=""LB"" currency=""USD"">
    {0}
</rateList>
";
            }
        }

        private static string RateGroupsTemplate
        {
            get
            {
                return @"
    <rateGroups zoneAssigned=""{0}"">
        <zoneRate packaging=""CUSTOM"">
            {1}            
        </zoneRate>
    </rateGroups>";
            }
        }

        private static string PackageRatesTemplate
        {
            get
            {
                return @"
            <packageRates type=""1"" weight=""{0}"" amount=""{1}"" minAmount=""{1}""/>
";
            }
        }
        */
    }
}
