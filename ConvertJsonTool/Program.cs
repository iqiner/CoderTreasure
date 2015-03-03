using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Text;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using Newegg.DataAccess;

namespace ConvertJsonTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> DBServerMapping = new Dictionary<string, string>();
            DBServerMapping.Add("06", "S6sql01");
            DBServerMapping.Add("07", "S7sql01");
            DBServerMapping.Add("08", "S8sql01");
            DBServerMapping.Add("09", "S9sql01");
            DBServerMapping.Add("10", "EWR01Sesq02");
            DBServerMapping.Add("12", "Mem01Sesq01");
            DBServerMapping.Add("14", "S14Sql01");
            DBServerMapping.Add("30", "S30Sql01");
            DBServerMapping.Add("31", "S31Sql01");
            DBServerMapping.Add("32", "S32Sql01");
            Console.WriteLine("Please input warehouse, seperate by comma: ");
            string keys = Console.ReadLine();

            foreach (var key in keys.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!DBServerMapping.Keys.Contains(key))
                {
                    continue;
                }
                string server = DBServerMapping[key];

                string sql = "Select logID, action, content FROM abs.dbo.RateShoppingConfigLog WITH(NOLOCK) where logID=12 order by logID asc";

                DataTable dt = null;
                try
                {
                    DataSet ds = DataAccessHelper.GetDataSet(sql, "AA", server);
                    dt = ds.Tables[0];
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                TranslateToJson(server, dt);
            }
            Console.WriteLine("Complete!");
            Console.ReadLine();
        }

        private static void TranslateToJson(string server, DataTable dt)
        {

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    int logID = row.Field<int>("LogID");
                    string type = row.Field<string>("Action").Trim().ToUpper();
                    string content = row.Field<string>("Content").Trim();
                    Console.WriteLine("Translate logID:" + logID);
                    if (type.Contains("ShipViaSetting".ToUpper()))
                    {
                        var shipviaSetting = GetShipViaSettingFromPlainText(content);
                        if (shipviaSetting != null)
                        {
                            try
                            {
                                string json = ServiceStack.Text.JsonSerializer.SerializeToString(shipviaSetting);
                                if (json != "{}")
                                {
                                    UpdateContent(logID, json, server);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                        }
                    }
                    else if (type.Contains("ShipViaGroup".ToUpper()))
                    {
                        var shipviaGroup = GetShipViaGroupFromPlainText(content);
                        if (shipviaGroup != null)
                        {
                            try
                            {
                                string json = ServiceStack.Text.JsonSerializer.SerializeToString(shipviaGroup);
                                UpdateContent(logID, json, server);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }
        }

        public static void UpdateContent(int logID, string json, string server)
        {
            json = json.Substring(0, json.Length > 4000 ? 4000 : json.Length).Replace("'", "''");
            string sql = String.Format(@"Update abs.dbo.RateShoppingConfigLog set content='{0}' WHERE logID={1}", json, logID);
            DataAccessHelper.AnyExecuteNoResult(sql, server);
        }

        public static ShipViaSetting GetShipViaSettingFromPlainText(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
            {
                return null;
            }

            var shipViaSetting = new ShipViaSetting();
            try
            {
                var fields = plainText.SplitString(true, ';');
                foreach (var field in fields)
                {
                    var pair = field.SplitString(false, ':');
                    if (pair.Count == 2
                        && !string.IsNullOrWhiteSpace(pair[0])
                        && !string.IsNullOrWhiteSpace(pair[1]))
                    {
                        //pair[1] = pair[1].Replace("\"", "\"\"");
                        switch (pair[0].Trim().ToUpper())
                        {
                            case "SHIPVIACODE":
                                shipViaSetting.ShipViaCode = pair[1].Trim();
                                break;

                            case "SERVICENAME":
                                shipViaSetting.ServiceName = pair[1].Trim();
                                break;

                            case "PROMISEDTRANSITDAY":
                                shipViaSetting.PromisedTransitDay = byte.Parse(pair[1].Trim());
                                break;

                            case "SIGNATURESERVICESUPPORTED":
                                shipViaSetting.SignatureServicieSupported = bool.Parse(pair[1].Trim());
                                break;

                            case "SATURDAYDELIVERYSUPPORTED":
                                shipViaSetting.SaturdayDeliverySupported = bool.Parse(pair[1].Trim());
                                break;

                            case "SATURDAYDELIVERYCHARGE":
                                shipViaSetting.SaturdayDeliveryCharge = Decimal.Parse(pair[1].Trim());
                                break;

                            case "SUPPORTEDADDRESSTYPE":
                                shipViaSetting.SupportedAddressType = pair[1].Trim();
                                break;

                            case "DAILYSOCOUNT":
                                shipViaSetting.DailySOCount = int.Parse(pair[1].Trim());
                                break;

                            case "ALLOWANCE":
                                shipViaSetting.Allowance = Decimal.Parse(pair[1].Trim());
                                break;

                            case "WEIGHTSIZELIMITEXPRESSION":
                                shipViaSetting.WeightSizeLimitExpression = FromXmlString(pair[1].Trim());
                                break;

                            case "SQLFORTRANSITDAY":
                                shipViaSetting.SqlForTransitDay = pair[1].Trim();
                                break;

                            case "SQLFORZIPCODEREACHABILITY":
                                shipViaSetting.SqlForZipCodeReachability = pair[1].Trim();
                                break;

                            case "SQLFORWEIGHTANDDIMENSIONLIMIT":
                                shipViaSetting.SqlForWeightAndDimensionLimit = pair[1].Trim();
                                break;

                            case "SQLFOROTHERLIMIT":
                                shipViaSetting.SqlForOtherLimit = pair[1].Trim();
                                break;

                            case "MAXSHIPMENTVALUE":
                                shipViaSetting.MaxShipmentValue = new SettingInfo();
                                shipViaSetting.MaxShipmentValue.DefaultValue = Decimal.Parse(pair[1].Trim()).ToString();
                                break;

                            case "MINSHIPMENTVALUE":
                                shipViaSetting.MinShipmentValue = new SettingInfo();
                                shipViaSetting.MinShipmentValue.DefaultValue = Decimal.Parse(pair[1].Trim()).ToString();
                                break;

                            case "NOCAPMAXWEIGHT":
                                shipViaSetting.NoCapMaxWeight = Decimal.Parse(pair[1].Trim());
                                break;
                        }
                    }
                }

                return shipViaSetting;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static ExpressionGroup FromXmlString(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                return null;
            }

            try
            {
                TextReader reader = new StringReader(xml);

                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ExpressionGroup));
                var exprGroup = (ExpressionGroup)serializer.Deserialize(reader);
                reader.Close();

                return exprGroup;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static ShipViaGroup GetShipViaGroupFromPlainText(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
            {
                return null;
            }

            var shipViaGroup = new ShipViaGroup();
            try
            {
                var fields = plainText.SplitString(true, ';');
                foreach (var field in fields)
                {
                    var pair = field.SplitString(false, ':');
                    if (pair.Count == 2
                       && !string.IsNullOrWhiteSpace(pair[0])
                       && !string.IsNullOrWhiteSpace(pair[1]))
                    {
                        //pair[1] = pair[1].Replace("\"", "\"\"");
                        switch (pair[0].Trim().ToUpper())
                        {
                            case "GROUPID":
                                shipViaGroup.GroupID = int.Parse(pair[1].Trim());
                                break;

                            case "GROUPNAME":
                                shipViaGroup.GroupName = pair[1].Trim();
                                break;

                            case "ORIGINALSHIPVIACODES":
                                shipViaGroup.OriginalShipViaCodes = pair[1].Trim();
                                break;

                            case "RATINGSHIPVIACODES":
                                shipViaGroup.RatingShipViaCodes = pair[1].Trim();
                                break;

                            case "EFFECTIVE":
                                shipViaGroup.Effective = bool.Parse(pair[1].Trim());
                                break;
                        }
                    }
                }

                return shipViaGroup;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
