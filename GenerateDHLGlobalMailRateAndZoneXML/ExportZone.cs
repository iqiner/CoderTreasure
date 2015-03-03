using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace GenerateDHLGlobalMailRateAndZoneXML
{
    #region Zone Entity

    public class CountryCode
    {
        public static string GBR = "UNITED_KINGDOM";
        public static string AUS = "AUSTRALIA";
    }

    public class PostCode
    {
        public string StartPostCode { get; set; }

        public string EndPostCode { get; set; }

        public int Zone { get; set; }

        public override string ToString()
        {
            return string.Format(@"<PostalCode startPostalCode=""{0}"" endPostalCode=""{1}"" />", this.StartPostCode, this.EndPostCode);
        }
    }

    public class Zone
    {
        private List<PostCode> postCodes = new List<PostCode>();

        public string Country { get; set; }

        public string ZoneID { get; set; }

        public void Add(PostCode postCode)
        {
            if (!this.postCodes.Contains(postCode))
            {
                this.postCodes.Add(postCode);
            }
        }
        public void Add(List<PostCode> postCodes)
        {
            this.postCodes.AddRange(postCodes);
        }

        public override string ToString()
        {
            StringBuilder codes = new StringBuilder();
            this.postCodes.ForEach(code => codes.Append(code.ToString()));
            return string.Format(@"<zone country=""{0}"" zoneAssigned=""{1}"">{2}</zone>", this.Country, this.ZoneID, codes.ToString());
        }
    }

    public class ZoneList
    {
        private string name = "";
        private List<Zone> zonelist = new List<Zone>();

        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public void Add(Zone zone)
        {
            if (!this.zonelist.Contains(zone))
            {
                this.zonelist.Add(zone);
            }
        }

        public override string ToString()
        {
            StringBuilder zones = new StringBuilder();
            this.zonelist.ForEach(zone => zones.Append(zone.ToString()));

            return string.Format(@"<zoneList name=""{0}"">{1}</zoneList>", name, zones.ToString());
        }
    }

    #endregion

    public class ExportZone : ExportBase
    {
        public ExportZone(string sourceFileDir, string xmlFileDir)
            : base(sourceFileDir, xmlFileDir)
        { }

        protected override string ExportSheetName
        {
            get { return "[Sheet2$]"; }
        }

        protected override string GenerateXML(DataSet ds)
        {

            List<PostCode> codes = new List<PostCode>();
            for (int i = 1; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow row = ds.Tables[0].Rows[i];
                if (row[1] == DBNull.Value)
                {
                    break;
                }

                string postCodes = row[1].ToString().Trim();
                int zone = Convert.ToInt32(row[4].ToString().Substring(0, 1));

                foreach (string code in postCodes.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string _code = Regex.Replace(code, @"(non-geo)+|(shared)+|(\[\d+\])+|\s+", "", RegexOptions.IgnoreCase);
                    if (!string.IsNullOrWhiteSpace(_code) && !codes.Exists(c => c.StartPostCode == _code))
                    {
                        codes.Add(new PostCode { StartPostCode = _code, EndPostCode = _code, Zone = zone });
                    }
                }
            }

            ZoneList zoneList = new ZoneList { Name = "DHL_GM_UK_AU_Zone" };
            codes.GroupBy(code => code.Zone).OrderBy(code => code.Key).ToList().ForEach(g =>
            {
                Zone zone = new Zone { Country = CountryCode.GBR, ZoneID = "GB-" + g.Key };
                zone.Add(g.ToList());
                zoneList.Add(zone);
            });

            Zone zoneForAU = new Zone { Country = CountryCode.AUS, ZoneID = "AU" };
            zoneForAU.Add(new PostCode { StartPostCode = "00000-0000", EndPostCode = "99999-9999" });
            zoneList.Add(zoneForAU);

            return String.Format(@"
<MiscData>
    <configurationList name=""DHL_GM_UK_AU"">
		    <namepair name=""DIM_RATING_FACTOR"" value=""166"" />
		    <namepair name=""MAX_PKG_LENGTH"" value=""42"" />
		    <namepair name=""MAX_PKG_LENGTH_GIRTH"" value=""79"" />
		    <namepair name=""MAX_PKG_WEIGHT"" value=""44"" />
		    <namepair name=""SATURDAY_DELIVERY"" value=""FALSE"" />
	    </configurationList>
        {0}
</MiscData>", zoneList.ToString());
        }

        protected override string GetExcelConnectionString(string excelFileName)
        {
            return string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""Excel 12.0;""", excelFileName);
        }
    }
}
