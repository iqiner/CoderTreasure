using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace GenerateDispatchSql
{
    public class CatalogConfig : ConfigurationElement
    {
        [ConfigurationProperty("CatalogName", IsRequired = true)]
        public string CatalogName
        {
            get
            {
                return this["CatalogName"].ToString();
            }
            set
            {
                this["CatalogName"] = value;
            }
        }

        [ConfigurationProperty("Priority", IsRequired = false)]
        public int Priority
        {
            get
            {
                return Convert.ToInt32(this["Priority"].ToString());
            }
            set
            {
                this["Priority"] = value;
            }
        }

        [ConfigurationProperty("OtherJoinTableAboutQueryCriteria", IsRequired = false)]
        public string OtherJoinTableAboutQueryCriteria
        {
            get
            {
                return this["OtherJoinTableAboutQueryCriteria"].ToString();
            }
            set
            {
                this["OtherJoinTableAboutQueryCriteria"] = value;
            }
        }

        [ConfigurationProperty("ShipviaCodes", IsRequired = false)]
        public string ShipviaCodes
        {
            get
            {
                return string.Join(",", this["ShipviaCodes"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(shipvia => "''" + shipvia.Trim() + "''").ToArray());
            }
            set
            {
                this["ShipviaCodes"] = value;
            }
        }

        [ConfigurationProperty("LocationTypes", IsRequired = false)]
        public string LocationTypes
        {
            get
            {
                return string.Join(",", this["LocationTypes"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(location => "''" + location.Trim() + "''").ToArray());
            }
            set
            {
                this["LocationTypes"] = value;
            }
        }

        [ConfigurationProperty("TabName", IsRequired = false)]
        public string TabName
        {
            get
            {
                return this["TabName"].ToString();
            }
            set
            {
                this["TabName"] = value;
            }
        }

        [ConfigurationProperty("TabIndex", IsRequired = false)]
        public int TabIndex
        {
            get
            {
                return Convert.ToInt32(this["TabIndex"]);
            }
            set
            {
                this["TabIndex"] = value;
            }
        }

        [ConfigurationProperty("SpecificCriteria", IsRequired = false)]
        public string SpecificCriteria
        {
            get
            {
                return this["SpecificCriteria"].ToString();
            }
            set
            {
                this["SpecificCriteria"] = value;
            }
        }
    }

    public class CatalogConfigCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CatalogConfig();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as CatalogConfig).CatalogName;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "Catalog";
            }
        }
    }

    public class WarehouseElement : ConfigurationElement
    {
        [ConfigurationProperty("Number", IsRequired = true)]
        public string Number
        {
            get
            {
                return this["Number"].ToString();
            }
            set
            {
                this["Number"] = value;
            }
        }

        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public CatalogConfigCollection Catalogs
        {
            get { return base[""] as CatalogConfigCollection; }
            set { base[""] = value; }
        }
    }

    public class WarehouseCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new WarehouseElement();
        }

        public new WarehouseElement this[string key]
        {
            get
            {
                return BaseGet(key) as WarehouseElement;
            }
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as WarehouseElement).Number;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "Warehouse";
            }
        }
    }


    public class CatalogConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public WarehouseCollection Warehouses
        {
            get
            {
                return this[""] as WarehouseCollection;
            }
        }
    }
}
