using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace BirdDog
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string connectionString = "USER ID=WHDbo;PASSWORD =2Dev4WH;DATA SOURCE =NewSql;INITIAL CATALOG =FedEx";
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Execute.sql");
                string sql = string.Empty;
                using (FileStream file = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        sql = reader.ReadToEnd();
                    }
                }
                List<ShipmentEntity> shipmentList = new List<ShipmentEntity>();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sql, conn))
                    {
                        command.CommandTimeout = 1800;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            string trackingNumber = reader.GetString(reader.GetOrdinal("TrackingNumber")).Trim();
                            string salesOrderNumber = reader.GetString(reader.GetOrdinal("SalesOrderNumber")).Trim();
                            Console.WriteLine("Tracking Number:" + trackingNumber);
                            ShipmentEntity shipment = shipmentList.FirstOrDefault(entity => entity.TrackingNumber == trackingNumber && entity.SalesOrderNumber == salesOrderNumber);
                            if (shipment == null)
                            {
                                shipment = new ShipmentEntity();

                                shipment.TrackingNumber = trackingNumber;
                                shipment.SalesOrderNumber = salesOrderNumber;
                                shipment.PONumber = reader.GetString(reader.GetOrdinal("PONumber")).Trim();
                                shipment.ShipDate = reader.GetDateTime(reader.GetOrdinal("ShipDate"));
                                shipment.CarrierName = reader.GetString(reader.GetOrdinal("CarrierName")).Trim();
                                shipment.ShipmentWeight = reader.GetDecimal(reader.GetOrdinal("ShipmentWeight"));
                                shipment.TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"));
                                shipment.ConsigneeID = reader.GetInt32(reader.GetOrdinal("ConsigneeID")).ToString();
                                shipment.ShipperName = reader.GetString(reader.GetOrdinal("ShipperName")).Trim();
                                shipmentList.Add(shipment);
                            }

                            if (shipment.ItemList == null)
                            {
                                shipment.ItemList = new List<ItemEntity>();
                            }
                            ItemEntity itemEntity = new ItemEntity
                            {
                                Item = reader.GetString(reader.GetOrdinal("ItemNumber")).Trim(),
                                Qty = reader.GetInt32(reader.GetOrdinal("Qty")),
                                Weight = reader.GetDecimal(reader.GetOrdinal("Weight")),
                                UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice"))
                            };
                            if (!shipment.ItemList.Exists(item => item.Item == itemEntity.Item))
                            {
                                shipment.ItemList.Add(itemEntity);
                            }
                        }
                    }
              
                    conn.Close();
                }

                List<ShipmentEntity> shipments = new List<ShipmentEntity>();
                var groups = shipmentList.GroupBy(item => new { TrackingNumber = item.TrackingNumber });

                foreach (var group in groups)
                {
                    ShipmentEntity shipment = group.FirstOrDefault();
                    shipments.Add(shipment);
                    foreach (ShipmentEntity _entity in group)
                    {
                        if (!shipment.Equals(_entity))
                        {
                            shipment.ShipmentWeight += _entity.ShipmentWeight;
                            shipment.TotalAmount += _entity.TotalAmount;

                            foreach (ItemEntity item in _entity.ItemList)
                            {
                                ItemEntity _item = shipment.ItemList.FirstOrDefault(i => i.Item == item.Item);
                                if (_item == null)
                                {
                                    shipment.ItemList.Add(item);
                                }
                                else
                                {
                                    _item.Qty += item.Qty;
                                }
                            }
                        }
                    }
                }

                Console.WriteLine(shipments.Count);
                string exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export.csv");
                if (File.Exists(exportPath))
                {
                    File.Delete(exportPath);
                }
                using (StreamWriter writer = File.CreateText(exportPath))
                {
                    string header = "Shipment Ref#,Po Number,Ship Date,Carrier Name,Sales Order Number,Shipment Weight,Freight Amount,Consignee ID,Warehouse Number,Shipment Cost,No of Pieces,Line#,Item Number,Qty,Weight";
                    writer.WriteLine(header);
                    foreach (ShipmentEntity shipment in shipments)
                    {
                        writer.WriteLine(shipment.ToString());
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace.ToString());
                using (StreamWriter writer = File.CreateText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log.txt")))
                {
                    writer.WriteLine(ex.Message);
                    writer.WriteLine(ex.StackTrace.ToString());
                    writer.Flush();
                }
            }
            Console.ReadKey();
        }
    }

    public class ShipmentEntity
    {
        public string TrackingNumber { get; set; }

        public string PONumber { get; set; }

        public DateTime ShipDate { get; set; }

        public string CarrierName { get; set; }

        public string SalesOrderNumber { get; set; }

        public decimal ShipmentWeight { get; set; }

        public decimal TotalAmount { get; set; }

        public string ConsigneeID { get; set; }

        public string ShipperName { get; set; }

        public decimal ShipmentCost
        {
            get
            {
                if (this.ItemList == null)
                {
                    return 0;
                }
                decimal cost = 0;
                this.ItemList.ForEach(item =>
                {
                    cost += item.Qty * item.UnitPrice;
                });
                return cost;
            }
        }

        public int NoofPieces
        {
            get
            {
                if (this.ItemList == null)
                {
                    return 0;
                }
                else
                {
                    int count = 0;
                    this.ItemList.ForEach(item => count += item.Qty);
                    return count;
                }
            }
        }

        public int SKU
        {
            get
            {
                if (this.ItemList == null)
                {
                    return 0;
                }
                else
                {
                    return this.ItemList.Count;
                }
            }
        }

        public List<ItemEntity> ItemList { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int lineNumber = 0;
            foreach (ItemEntity item in this.ItemList)
            {
                sb.Append(this.TrackingNumber.Replace(",", "\",\"")
                                            .Replace("\"", "\"\""));
                sb.Append(",");
                sb.Append(this.PONumber);
                sb.Append(",");
                sb.Append(this.ShipDate.ToString("MMddyyyy"));
                sb.Append(",");
                sb.Append(this.CarrierName);
                sb.Append(",");
                sb.Append(this.SalesOrderNumber);
                sb.Append(",");
                sb.Append(this.ShipmentWeight);
                sb.Append(",");
                sb.Append(this.TotalAmount);
                sb.Append(",");
                sb.Append(this.ConsigneeID);
                sb.Append(",");
                sb.Append(this.ShipperName);
                sb.Append(",");
                sb.Append(this.ShipmentCost);
                sb.Append(",");
                sb.Append(this.NoofPieces);
                sb.Append(",");
                sb.Append(++lineNumber);
                sb.Append(",");
                sb.Append(item.Item);
                sb.Append(",");
                sb.Append(item.Qty);
                sb.Append(",");
                sb.Append(item.Weight);
                if (lineNumber < this.ItemList.Count)
                {
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
    }

    public class ItemEntity
    {
        public string Item { get; set; }

        public int Qty { get; set; }

        public decimal Weight { get; set; }

        public decimal UnitPrice { get; set; }
    }
}
