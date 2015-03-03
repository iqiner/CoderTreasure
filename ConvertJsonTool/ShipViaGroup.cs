using System.Runtime.Serialization;

namespace ConvertJsonTool
{
    public class ShipViaGroup
    {
        public string WarehouseNumber { get; set; }

        public int GroupID { get; set; }

        public string GroupName { get; set; }

        public string OriginalShipViaCodes { get; set; }

        public string RatingShipViaCodes { get; set; }

        public bool Effective { get; set; }

        public string UserID { get; set; }
    }
}
