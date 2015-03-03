#region FileHeader
/********************************************************************************
** Copyright (C) 2010 Newegg. All rights reserved.
**
**
** File Name:           Operations
** Creator:             Sure.J.Deng
** Create date:         10/16/2012 15:43:41 
** CLR Version:         3.5 
** NameSpace:           DataExport 
** Description:         TODO: the class description
** Latest Modifier:     sd45
** Latest Modify date:  10/16/2012 15:43:41 
**
**
** Version number:      1.0.0.0
*********************************************************************************/
#endregion

namespace DBExport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Data.SqlClient;
    using System.Data;

    public static class ConnectionString
    {
        private const string connectionString = @"USER ID={0};PASSWORD ={1};DATA SOURCE ={2};INITIAL CATALOG ={3}";

        public static string Get(string userID, string password, string dataSource, string initialCatalog)
        {
            return string.Format(connectionString, userID, password, dataSource, initialCatalog);
        }
    }

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Export
    {
        private readonly string connectionString = string.Empty;
        private string fields;
        private string tableFullName;
        private string condition;
        private bool needSeperate;
        private int batchCount = 2000;

        public string TableFullName
        {
            get { return this.tableFullName; }
        }


        public Export(string dataSource, string initialCatalog, string userID, string password, string fields, string tableFullName, string condition)
            : this(ConnectionString.Get(userID, password, dataSource, initialCatalog), fields, tableFullName, condition)
        {
        }

        public Export(string connectionString, string fields, string tableFullName, string condition)
        {
            this.connectionString = connectionString;
            this.fields = fields;
            this.tableFullName = tableFullName;
            this.condition = condition;
        }

        public bool NeedSeperate
        {
            get { return this.needSeperate; }
            set { this.needSeperate = value; }
        }

        public int BatchCount
        {
            get { return this.batchCount; }
            set { this.batchCount = value; }
        }

        public virtual List<string> GenerateBehavior(SqlDataReader reader)
        {
            List<string> list = new List<string>();

            int count = 1;
            StringBuilder sb = new StringBuilder();
            while (reader.Read())
            {
                string[] fieldName = this.fields.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string values = "";
                for (int i = 0; i < fieldName.Length; i++)
                {
                    string field = fieldName[i].Trim();
                    object value = reader.GetValue(reader.GetOrdinal(field));
                    if (value == DBNull.Value)
                    {
                        values += "Null,";
                    }
                    else
                    {
                        if (new string[] { "System.Data.SqlTypes.SqlString", "System.Data.SqlTypes.SqlDateTime" }
                                        .Contains(reader.GetProviderSpecificFieldType(reader.GetOrdinal(field)).FullName))
                        {
                            values += String.Format("'{0}',", value.ToString().Replace("'", "''"));
                        }
                        else
                        {
                            values += value.ToString() + ",";
                        }
                    }
                }
                values = values.TrimEnd(',');
                string insertSql = string.Format(@"insert {0}({1}) values({2})", this.tableFullName, this.fields, values);
                sb.AppendLine(insertSql);
                if (count % this.batchCount == 0 && this.needSeperate)
                {
                    list.Add(sb.ToString());
                    sb.Remove(0, sb.Length - 1);
                }
                count++;
            }

            if (sb.Length > 0)
            {
                list.Add(sb.ToString());
            }

            return list;
        }

        public virtual string QuerySqlTemplate()
        {
            return "select {0} from {1} {2}";
        }

        public List<string> GenerateContent()
        {
            List<string> contents = new List<string>();
            string sql = String.Format(this.QuerySqlTemplate(), this.fields, this.tableFullName, this.condition);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    contents = this.GenerateBehavior(reader);
                    
                }
                conn.Close();
            }
            
            return contents;
        }

        public virtual bool ExportData(string path)
        {
            bool isSuccess = true;
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                string fileExtension = Path.GetExtension(path);
                List<string> contents = this.GenerateContent();
                for (int i = 0; i < contents.Count; i++)
                {
                    string content = contents[i];
                    if (this.needSeperate)
                    {

                        path = fileName + "_" + i + fileExtension;
                    }
                    using (StreamWriter writer = new StreamWriter(path))
                    {
                        writer.WriteLine(content);
                        writer.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("导出操作发生异常", ex);
            }
            return isSuccess;
        }
    }

    public class ExportTableStruct : Export
    {
        public ExportTableStruct(string dataSource, string initialCatalog, string userID, string password, string fields, string tableFullName, string condition)
            : base(dataSource, initialCatalog, userID, password, fields, tableFullName, condition)
        {
        }

        public override string QuerySqlTemplate()
        {
            return "select top 1 {0} from {1} {2}";
        }

        public override List<string> GenerateBehavior(SqlDataReader reader)
        {
            List<string> list = new List<string>();
            
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"<{0}>{1}", this.TableFullName, Environment.NewLine);
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);
                    sb.AppendFormat(@"<{0}></{0}>{1}", columnName, Environment.NewLine);
                }
            }
            sb.AppendFormat(@"</{0}>{1}", this.TableFullName, Environment.NewLine);
            list.Add(sb.ToString());
            return list;
        }
    }
}
