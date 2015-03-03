using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace NeweggConvertJsonTool
{
    public class CustomerCarrierRule
    {
        public int ID { get; set; }

        public int? CustomerNumber { get; set; }

        public string CarrierType { get; set; }

        public string Type { get; set; }

        public DateTime? InDate { get; set; }

        public string InUser { get; set; }

        public string LastEditUser { get; set; }

        public DateTime? LastEditDate { get; set; }
    }
}
