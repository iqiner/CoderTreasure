using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateDHLGlobalMailRateAndZoneXML
{
    #region Rate Entity

    public class RateEntiy
    {
        private string type = "1";

        public decimal Weight { get; set; }
        public string Zone { get; set; }
        public decimal Price { get; set; }
        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                this.type = value;
            }
        }

        public override string ToString()
        {
            return string.Format("<packageRates type=\"{2}\" weight=\"{0}\" amount=\"{1}\" minAmount=\"{1}\"/>", this.Weight, this.Price, this.Type);
        }
    }

    public class RateEntityGroup
    {
        private string zone;
        private string packaging = "CUSTOM";
        private List<RateEntiy> rateGroups = new List<RateEntiy>();

        public string Zone
        {
            get
            {
                return this.zone;
            }
            set
            {
                this.zone = value;
            }
        }

        public string Packaging
        {
            get
            {
                return this.packaging;
            }
            set
            {
                this.packaging = value;
            }
        }

        public void Add(RateEntiy entity)
        {
            if (!this.rateGroups.Contains(entity))
            {
                this.rateGroups.Add(entity);
            }
        }

        public override string ToString()
        {
            StringBuilder rates = new StringBuilder();
            this.rateGroups.ForEach(rate => rates.Append(rate.ToString()));
            return string.Format(@"<rateGroups zoneAssigned=""{0}""><zoneRate packaging=""{1}"">{2}</zoneRate></rateGroups>", this.zone, this.packaging, rates.ToString());
        }
    }

    public class RateEntityGroups
    {
        private string name = string.Empty;
        private string weightUnit = "LB";
        private string currency = "USD";
        private List<RateEntityGroup> groups = new List<RateEntityGroup>();

        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        public string WeightUnit
        {
            get { return this.weightUnit; }
            set { this.weightUnit = value; }
        }
        public string Currency
        {
            get { return this.currency; }
            set { this.currency = value; }
        }

        public void Add(RateEntityGroup group)
        {
            if (!this.groups.Contains(group))
            {
                this.groups.Add(group);
            }
        }

        public override string ToString()
        {
            StringBuilder groups = new StringBuilder();
            this.groups.ForEach(group => groups.Append(group.ToString()));
            return string.Format(@"<MiscData><rateList name=""{0}"" wtUnit=""{1}"" currency=""{2}"">{3}</rateList></MiscData>", this.Name, this.WeightUnit, this.Currency, groups.ToString());
        }
    }

    #endregion

    public class ExportRate : ExportBase
    {
        public ExportRate(string sourceFileDir, string xmlFileDir)
            : base(sourceFileDir, xmlFileDir)
        { }

        protected override string ExportSheetName
        {
            get
            {
                return "[PLT$]";
            }
        }

        protected override string GenerateXML(DataSet ds)
        {
            Dictionary<string, Dictionary<decimal, decimal>> rateDic = new Dictionary<string, Dictionary<decimal, decimal>>();

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow row = ds.Tables[0].Rows[i];
                if (row[0] == DBNull.Value)
                {
                    continue;
                }

                string[] header = new string[] { "AU", "GB-1", "GB-2", "GB-3", "GB-4" };
                int[] validColumn = new int[] { 22, 24, 25, 26, 27 };

                for (int j = 0; j < header.Length; j++)
                {
                    string zone = header[j];
                    if (!rateDic.ContainsKey(zone))
                    {
                        rateDic.Add(zone, new Dictionary<decimal, decimal>());
                    }
                    int columnNumber = validColumn[j];
                    if (row[columnNumber] != DBNull.Value)
                    {
                        decimal weight = Math.Round(Convert.ToDecimal(row[0]), 2, MidpointRounding.AwayFromZero);
                        decimal rate = Convert.ToDecimal(row[columnNumber]);
                        rateDic[zone].Add(weight, rate);
                    }
                }
            }

            RateEntityGroups groups = new RateEntityGroups { Name = this.CurrentProcessFileName };

            foreach (string key in rateDic.Keys)
            {
                RateEntityGroup group = new RateEntityGroup { Zone = key };
                foreach (KeyValuePair<decimal, decimal> kv in rateDic[key])
                {
                    group.Add(new RateEntiy { Zone = key, Weight = kv.Key, Price = kv.Value });
                }
                groups.Add(group);
            }
            return groups.ToString();
        }
    }

}
