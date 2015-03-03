using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ConvertJsonTool
{
    public class ShipViaSetting
    {
        public string WarehouseNumber { get; set; }

        public string ShipViaCode { get; set; }

        public string ServiceName { get; set; }

        public byte? PromisedTransitDay { get; set; }

        public bool? SignatureServicieSupported { get; set; }

        public bool? SaturdayDeliverySupported { get; set; }

        public decimal? SaturdayDeliveryCharge { get; set; }

        public string SupportedAddressType { get; set; }

        public int? DailySOCount { get; set; }

        public decimal? Allowance { get; set; }

        public ExpressionGroup WeightSizeLimitExpression { get; set; }

        public string SqlForTransitDay { get; set; }

        public string SqlForZipCodeReachability { get; set; }

        public string SqlForWeightAndDimensionLimit { get; set; }

        public string SqlForOtherLimit { get; set; }

        public string UserID { get; set; }

        public decimal? NoCapMaxWeight { get; set; }

        public SettingInfo MaxShipmentValue { get; set; }

        public SettingInfo MinShipmentValue { get; set; }

        public SettingInfo MaxFreightCost { get; set; }

        public SettingInfo MinFreightCost { get; set; }
    }

    public class SettingInfo
    {   
        [XmlAttribute]
        public string DefaultValue { get; set; }

        [XmlElement(ElementName="Setting")]
        public List<ExtraSetting> Settings { get; set; }
    }

    public class ExtraSetting
    {
        [XmlIgnore]
        public string Shipvia { get; set; }

        [XmlElement(ElementName="Country")]
        public string CountryCode { get; set; }

        [XmlIgnore]
        public string Type { get; set; }

        public string Value { get; set; }
    }
}
