using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace GenerateDispatchSql
{
    public abstract class CatalogBase
    {
        public virtual string SqlScriptsHeader
        {
            get
            {
                return @"
Use DropShip
Go

Declare @SqlCount varchar(2000)
Declare @SqlQuery varchar(2000)
Declare @SqlCatalog varchar(2000)
Declare @SortID int";
            }
        }

        public virtual List<Catalog> Configs
        {
            get
            {
                return Utils.GetConfigs(this.WarehouseNumber);
            }
        }

        public virtual string WarehouseNumber { get; set; }

        public abstract string Template { get; }

        public string GenerateDispatchSql()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(this.SqlScriptsHeader);
            this.Configs.ForEach(config =>
            {
                string statement = this.Template;
                foreach (var property in config.GetType().GetProperties())
                {
                    string name = "{{" + property.Name + "}}";
                    if (statement.IndexOf(name) == -1)
                    {
                        continue;
                    }

                    object value = property.GetValue(config, null);
                    string replaceValue = string.Empty;

                    if (value != null)
                    {
                        replaceValue = value.ToString();
                        if (!string.IsNullOrWhiteSpace(replaceValue))
                        {
                            switch (property.Name)
                            {
                                case "LocationTypes":
                                    replaceValue = string.Join(",", replaceValue.Split(',').ToList().ConvertAll(item => string.Format("''{0}''", item.Trim())).ToArray());
                                    replaceValue = string.Format("AND A.LocationType in ({0})", replaceValue);
                                    break;
                                case "ShipviaCodes":
                                    replaceValue = string.Join(",", replaceValue.Split(',').ToList().ConvertAll(item => string.Format("''{0}''", item.Trim())).ToArray());
                                    replaceValue = string.Format("AND A.ShipVia in ({0})", replaceValue);
                                    break;
                                case "SpecificCriteria":
                                    replaceValue = string.Format("AND {0}", replaceValue.Replace("'", "''"));
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            replaceValue = "Null";
                        }
                    }

                    //System.Diagnostics.Trace.WriteLine(property.Name + " : " + replaceValue);
                    statement = statement.Replace(name, replaceValue);
                }
                

                sb.AppendLine(statement);
            });
            return sb.ToString();
        }

        public string SerializeToFile()
        {
            string directory = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "OutPut");
            string name = "DispatchSql_" + this.WarehouseNumber + ".sql";
            string path = Path.Combine(directory, name);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (StreamWriter writer = File.CreateText(path))
            {
                writer.Write(this.GenerateDispatchSql());
            }

            return path;
        }
    }

    public class Catalog07 : CatalogBase
    {
        public override string Template
        {
            get
            {
                return @"
SET @SqlCatalog='INSERT INTO DropShip.dbo.SOSorted ( DropShipID ,SubCode ,SoNumber ,SoDate ,ShipVia ,SortID ,ZoneID ,Item ,LargeItemFlag ,LocationType) SELECT A.DropShipID ,A.SubCode ,A.SoNumber ,A.SoDate ,A.ShipVia ,SortID = {0} ,A.ZoneID ,A.Item ,A.LargeItemFlag ,LocationType FROM DropShip.dbo.SOSorted2 A WITH(NOLOCK) INNER JOIN DropShip.dbo.SOSortTemp B WITH(NOLOCK) ON A.DropShipID = B.DropShipID AND A.SubCode = B.SubCode {{OtherJoinTableAboutQueryCriteria}} WHERE NOT EXISTS (SELECT DropShipID FROM DropShip.dbo.SOSorted C WITH(NOLOCK) WHERE A.DropShipID = c.DropShipID AND A.SubCode = c.SubCode ) {{ShipviaCodes}} {{LocationTypes}} {{SpecificCriteria}}'

INSERT dbo.DispatchSql(sqlName,sqlCount,sqlQuery,sqlCatalog,DefaultQuantity,DefaultLine,Priority,TabName,TabIndex) VALUES('{{CatalogName}}','','',@SqlCatalog,200,0,{{Priority}},'{{TabName}}',{{TabIndex}})

Select @SortID = SCOPE_IDENTITY()

SET @SqlQuery='SELECT {0} * FROM ( SELECT DISTINCT a.dropshipid ,a.subcode ,a.SoNumber ,a.CompanyCode ,a.Item ,a.SODate ,a.LargeItemFlag ,a.LocationType FROM dropship.dbo.sosorted2 A WITH(NOLOCK) INNER JOIN Dropship.dbo.SOSorted S WITH(NOLOCK) ON A.DropShipID = S.DropShipID AND A.SubCode=S.SubCode INNER JOIN DropShip.dbo.DropShipMaster b WITH(NOLOCK) ON a.DropShipID = b.DropShipID WHERE S.SortID ='+CAST(@SortID as varchar)+' AND b.Status NOT IN(''V'',''H'',''C'') AND NOT EXISTS ( SELECT DropShipID ,SubCode FROM DropShip.dbo.DownloadSOProcessLog B WITH(NOLOCK) WHERE B.DropShipID = A.DropShipID AND B.SubCode = A.SubCode AND NodeID > 1 ) ) z ORDER BY sodate'
Set @SqlCount='SELECT Count(*) FROM ( SELECT DISTINCT a.dropshipid ,a.subcode ,a.SoNumber ,a.CompanyCode ,a.Item ,a.SODate ,a.LargeItemFlag ,a.LocationType FROM dropship.dbo.sosorted2 A WITH(NOLOCK) INNER JOIN Dropship.dbo.SOSorted S WITH(NOLOCK) ON A.DropShipID = S.DropShipID AND A.SubCode=S.SubCode INNER JOIN DropShip.dbo.DropShipMaster b WITH(NOLOCK) ON a.DropShipID = b.DropShipID WHERE S.SortID ='+CAST(@SortID as varchar)+' AND b.Status NOT IN(''V'',''H'',''C'') AND NOT EXISTS ( SELECT DropShipID ,SubCode FROM DropShip.dbo.DownloadSOProcessLog B WITH(NOLOCK) WHERE B.DropShipID = A.DropShipID AND B.SubCode = A.SubCode AND NodeID > 1 ) ) z'

UPDATE TOP(1) dbo.DispatchSql Set sqlCount = @SqlCount,sqlQuery=@SqlQuery Where TransactionNumber=@SortID	
";
            }
        }
    }

    public class Catalog30 : CatalogBase
    {
        public override string Template
        {
            get
            {
                return @"
SET @SqlCatalog='INSERT INTO DropShip.dbo.SOSorted ( DropShipID ,SubCode ,SoNumber ,SoDate ,ShipVia ,SortID ,ZoneID ,Item ,LargeItemFlag) SELECT A.DropShipID ,A.SubCode ,A.SoNumber ,A.SoDate ,A.ShipVia ,SortID = {0} ,A.ZoneID ,A.Item ,A.LargeItemFlag FROM DropShip.dbo.SOSorted2 A WITH(NOLOCK) INNER JOIN DropShip.dbo.SOSortTemp B WITH(NOLOCK) ON A.DropShipID = B.DropShipID AND A.SubCode = B.SubCode {{OtherJoinTableAboutQueryCriteria}} WHERE NOT EXISTS (SELECT DropShipID FROM DropShip.dbo.SOSorted C WITH(NOLOCK) WHERE A.DropShipID = c.DropShipID AND A.SubCode = c.SubCode ) {{ShipviaCodes}} {{LocationTypes}} {{SpecificCriteria}}'

INSERT dbo.DispatchSql(sqlName,sqlCount,sqlQuery,sqlCatalog,DefaultQuantity,DefaultLine,Priority,TabName,TabIndex) VALUES('{{CatalogName}}','','',@SqlCatalog,200,0,{{Priority}},'{{TabName}}',{{TabIndex}})

Select @SortID = SCOPE_IDENTITY()

SET @SqlQuery='SELECT {0} * FROM ( SELECT DISTINCT a.dropshipid ,a.subcode ,a.SoNumber ,a.CompanyCode ,a.Item ,a.SODate ,a.LargeItemFlag ,a.LocationType FROM dropship.dbo.sosorted2 A WITH(NOLOCK) INNER JOIN Dropship.dbo.SOSorted S WITH(NOLOCK) ON A.DropShipID = S.DropShipID AND A.SubCode=S.SubCode INNER JOIN DropShip.dbo.DropShipMaster b WITH(NOLOCK) ON a.DropShipID = b.DropShipID WHERE S.SortID ='+CAST(@SortID as varchar)+' AND b.Status NOT IN(''V'',''H'',''C'') AND NOT EXISTS ( SELECT DropShipID ,SubCode FROM DropShip.dbo.DownloadSOProcessLog B WITH(NOLOCK) WHERE B.DropShipID = A.DropShipID AND B.SubCode = A.SubCode AND NodeID > 1 ) ) z ORDER BY sodate'
Set @SqlCount='SELECT Count(*) FROM ( SELECT DISTINCT a.dropshipid ,a.subcode ,a.SoNumber ,a.CompanyCode ,a.Item ,a.SODate ,a.LargeItemFlag ,a.LocationType FROM dropship.dbo.sosorted2 A WITH(NOLOCK) INNER JOIN Dropship.dbo.SOSorted S WITH(NOLOCK) ON A.DropShipID = S.DropShipID AND A.SubCode=S.SubCode INNER JOIN DropShip.dbo.DropShipMaster b WITH(NOLOCK) ON a.DropShipID = b.DropShipID WHERE S.SortID ='+CAST(@SortID as varchar)+' AND b.Status NOT IN(''V'',''H'',''C'') AND NOT EXISTS ( SELECT DropShipID ,SubCode FROM DropShip.dbo.DownloadSOProcessLog B WITH(NOLOCK) WHERE B.DropShipID = A.DropShipID AND B.SubCode = A.SubCode AND NodeID > 1 ) ) z'

UPDATE TOP(1) dbo.DispatchSql Set sqlCount = @SqlCount,sqlQuery=@SqlQuery Where TransactionNumber=@SortID	
";
            }
        }
    }

    public class Catalog08 : CatalogBase
    {
        public override string SqlScriptsHeader
        {
            get
            {
                return @"
Use DropShip
Go

Declare @SqlCount varchar(2000)
Declare @SqlQuery varchar(2000)
Declare @SqlCatalog varchar(2000)
Declare @LabelCount varchar(2000)
Declare @LabelQuery varchar(2000)
Declare @SortID int
";
            }
        }

        public override string Template
        {
            get
            {
                return @"
SET @SqlCatalog='INSERT INTO DropShip.dbo.SOSorted ( DropShipID ,SubCode ,SoNumber ,SoDate ,ShipVia ,SortID ,ZoneID ,Item ,LargeItemFlag ) SELECT a.DropShipID ,a.SubCode ,a.SoNumber ,a.SoDate ,a.ShipVia ,SortID={0} ,a.ZoneID ,a.Item ,a.LargeItemFlag FROM DropShip.dbo.SOSorted2 A WITH(NOLOCK) JOIN DropShip.dbo.SOSortTemp b WITH(NOLOCK) ON A.DropShipID=b.DropShipID AND A.SubCode=b.SubCode {{OtherJoinTableAboutQueryCriteria}} WHERE not exists ( SELECT DropShipID FROM DropShip.dbo.SOSorted c WITH(NOLOCK) WHERE A.DropShipID=c.DropShipID AND A.SubCode=c.SubCode ) {{ShipviaCodes}} {{LocationTypes}} {{SpecificCriteria}}'

INSERT dbo.DispatchSql(sqlName,sqlCount,sqlQuery,sqlCatalog,DefaultQuantity,DefaultLine,Priority,TabIndex) VALUES('{{CatalogName}}','','',@SqlCatalog,200,0,{{Priority}},{{TabIndex}})

Select @SortID = SCOPE_IDENTITY()

SET @SqlQuery='SELECT {0} * FROM ( SELECT DISTINCT a.dropshipid ,a.subcode ,a.SoNumber ,c.CompanyCode ,a.Item ,c.SODate ,a.LargeItemFlag ,a.zoneid FROM dropship.dbo.sosorted A WITH(NOLOCK) JOIN DropShip.dbo.DropShipMaster C WITH(NOLOCK) ON C.DropShipID=A.DropShipID WHERE a.SortID='+CAST(@SortID as varchar)+' AND C.Status<>''C'' and C.Status<>''V'' AND NOT EXISTS ( SELECT DropShipID ,SubCode FROM DropShip.dbo.DownloadSOProcessLog B WITH(NOLOCK) WHERE B.DropShipID =A.DropShipID AND B.SubCode=A.SubCode AND NodeID >1 ) )z LEFT OUTER JOIN dropship.dbo.zoneforprintmaster zz WITH(NOLOCK) ON z.zoneid=zz.zoneid ORDER BY zz.description,z.Item,z.sodate '
Set @SqlCount='SELECT COUNT(*) FROM ( SELECT DISTINCT a.dropshipid ,a.subcode FROM dropship.dbo.sosorted A WITH(NOLOCK) JOIN DropShip.dbo.DropShipMaster C WITH(NOLOCK) ON C.DropShipID=A.DropShipID WHERE C.AcceptDate>GETDATE()-7 AND C.Status<>''C'' and C.Status<>''V'' AND NOT EXISTS ( SELECT DropShipID ,SubCode FROM DropShip.dbo.DownloadSOProcessLog B WITH(NOLOCK) WHERE B.DropShipID =A.DropShipID AND B.SubCode=A.SubCode AND NodeID >1 ) AND a.SortID='+CAST(@SortID as varchar)+' )z'
SET @LabelCount='SELECT A.DropShipID,A.SubCode,B.CompanyCode,  B.ReferenceSONumber,B.ReferenceCustomerNumber,   B.OrderType,B.ShipViaCode,B.ShippingCompanyName,B.ShippingContactWith,  B.ShippingAddress1,B.ShippingAddress2,B.ShippingCity,B.ShippingState,   B.ShippingZipCode,B.ShippingCountry,B.ShippingPhone,   C.ItemNumber,C.UnitPrice,C.Quantity   FROM dropship.dbo.sosorted A  WITH(NOLOCK)  JOIN DropShip.dbo.DropShipMaster B  WITH(NOLOCK)    ON A.DropShipID=B.DropShipID  JOIN dropship.dbo.dropshipTransaction C  WITH(NOLOCK)  ON A.DropshipID=C.DropshipID AND A.SubCode=C.SubCode AND A.item=C.ItemNumber  WHERE B.Status<>''C'' AND B.Status<>''V''  AND A.SortID='+CAST(@SortID as varchar)+'  AND NOT EXISTS   (   SELECT DropShipID,SubCode FROM DropShip.dbo.DownloadSOProcessLog FB  WITH(NOLOCK)    WHERE  FB.DropShipID =A.DropShipID and FB.SubCode=A.SubCode and NodeID >1   )  ORDER BY  C.ItemNumber'
SET @LabelQuery='SELECT COUNT(A.DropShipID) FROM dropship.dbo.sosorted A  WITH(NOLOCK)  JOIN DropShip.dbo.DropShipMaster B  WITH(NOLOCK)  ON A.DropShipID=B.DropShipID  WHERE B.Status<>''C'' AND B.Status<>''V'' AND B.AcceptDate>getdate()-7   AND A.SortID='+CAST(@SortID as varchar)+'  AND NOT EXISTS   (   SELECT DropShipID,SubCode FROM DropShip.dbo.DownloadSOProcessLog FB  WITH(NOLOCK)    WHERE  FB.DropShipID =A.DropShipID and FB.SubCode=A.SubCode and NodeID >1      )  '

UPDATE top(1) dbo.DispatchSql
Set sqlCount = @SqlCount,sqlQuery=@SqlQuery,@labelCount=@LabelCount,@LabelQuery=@LabelQuery
Where TransactionNumber=@SortID
";
            }
        }
    }

    public class CatalogInstanceFactory
    {
        public static CatalogBase GetInstance(string warehouseNumber)
        {
            CatalogBase catalog = null;

            bool isSupport = false;
            var warehouses = ConfigurationManager.GetSection("WarehouseCatalogType") as Hashtable;
            foreach (string key in warehouses.Keys)
            {
                if (key.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries).Contains(warehouseNumber))
                {
                    isSupport = true;
                    string typeName = warehouses[key].ToString();
                    catalog = Activator.CreateInstance(Type.GetType(typeName)) as CatalogBase;
                    break;
                }
            }

            if (isSupport)
            {
                catalog.WarehouseNumber = warehouseNumber;
                return catalog;
            }
            else
            {
                throw new NotSupportedException("Can not support warehouse " + warehouseNumber);
            }
        }
    }
}
