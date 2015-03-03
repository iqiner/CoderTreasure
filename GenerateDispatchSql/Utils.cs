using CodeCenter.Common.Component;
using CodeCenter.Common.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace GenerateDispatchSql
{
    public class Utils
    {
        private static CatalogConfigs CatalogConfigs = XMLHelper.Deserialize<CatalogConfigs>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CatalogConfigs.xml"));
        private static Dictionary<string, string> DBServerMapping = new Dictionary<string, string>();
        private static Dictionary<string, string> MapNamesDic = new Dictionary<string, string>();
        private static int Formnumber = 1;

        static Utils()
        {
            try
            {
                var section = ConfigurationManager.GetSection("NameMapping") as Hashtable;
                if (section != null)
                {
                    foreach (string key in section.Keys)
                    {
                        object value = section[key];
                        if (value != null)
                        {
                            MapNamesDic.Add(key, value.ToString());
                        }
                    }
                }
               
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
            }
            catch { }
        }

        public static string GetUserName()
        {
            string userID = System.Environment.GetEnvironmentVariable("USERNAME");
            if (!string.IsNullOrEmpty(userID) && MapNamesDic.Keys.Contains(userID.ToLowerInvariant()))
            {
                return MapNamesDic[userID.ToLowerInvariant()];
            }
            return "Sure.J.Deng";
        }

        public static string GenerateForms(string CRLNo, params CatalogBase[] CatalogList)
        {
            List<DatabaseReq> DBReq = new List<DatabaseReq>();
            foreach (CatalogBase catalog in CatalogList)
            {
                string fileName = catalog.SerializeToFile();
                DatabaseReq databaseRequest = new DatabaseReq();
                databaseRequest.Server = DBServerMapping[catalog.WarehouseNumber];
                databaseRequest.Database = "Dropship";
                databaseRequest.ScriptFiles = new List<ScriptFile>
                {
                    new ScriptFile{ FileName=fileName, PType= ProcessType.DI, OType= ObjectType.U, OName="dbo.DispatchSql"}
                };
                DBReq.Add(databaseRequest);
            }

            DBForm form = new DBForm("DispatchSql", "Feedback");
            form.MessageCallback = new Action<string>(msg => Console.WriteLine(msg));
            return form.GenerateForm(Formnumber++, CRLNo, DBReq);
        }

        public static List<Catalog> GetConfigs(string warehouseNumber)
        {
            return CatalogConfigs[warehouseNumber].Catalogs;
        }
    }
}
