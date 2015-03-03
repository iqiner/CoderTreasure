using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CodeCenter.Common.Extensions;

namespace GenerateDispatchSql
{
    public class Catalog
    {
        [XmlAttribute]
        public string CatalogName { get; set; }

        [XmlAttribute]
        public int Priority { get; set; }

        [XmlAttribute]
        public string OtherJoinTableAboutQueryCriteria { get; set; }

        [XmlAttribute]
        public string ShipviaCodes { get; set; }

        [XmlAttribute]
        public string LocationTypes { get; set; }

        [XmlAttribute]
        public string TabName { get; set; }

        [XmlIgnore]
        public int? TabIndex { get; set; }

        [XmlAttribute(AttributeName = "TabIndex")]
        public string TabIndexValue
        {
            get
            {
                return TabIndex.HasValue ? TabIndex.Value.ToString() : null;
            }
            set
            {
                int result;
                TabIndex = int.TryParse(value, out result) ? result : (int?)null;

            }
        }

        [XmlAttribute]
        public string SpecificCriteria { get; set; }
    }

    public class Warehouse
    {
        [XmlAttribute("Number")]
        public string Number { get; set; }

        [XmlElement(ElementName = "Catalog")]
        public List<Catalog> Catalogs { get; set; }
    }

    [XmlRoot(ElementName = "CatalogConfig")]
    public class CatalogConfigs
    {
        [XmlElement(ElementName = "Warehouse")]
        public List<Warehouse> Warehouses { get; set; }

        public Warehouse this[string warehouseNumber]
        {
            get
            {
                if (this.Warehouses.IsNullOrEmptyCollection())
                {
                    return null;
                }
                return this.Warehouses.FirstOrDefault(warehouse => warehouse.Number == warehouseNumber);
            }
        }
    }
}
