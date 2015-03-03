#region FileHeader
/********************************************************************************
** Copyright (C) 2013 Newegg. All rights reserved.
**
**
** File Name:           DBMoveHelper
** Creator:             rc50
** Create date:         11/02/2013 09:08:56 AM 
** CLR Version:         3.5 
** NameSpace:           Newegg.Shipping.Tools 
** Description:         TODO...
** Latest Modifier:     rc50
** Latest Modify date:  11/02/2013 09:08:56 AM 
**
**
** Version number:      1.0.0.0
*********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeCenter.Common.Component
{
    public class DBForm
    {
        string ProjectName;
        string BaseName;
        public bool RemoveSQL
        {
            get
            {
                string config = ConfigurationManager.AppSettings["RemoveSqlFile"];
                if (string.IsNullOrEmpty(config))
                {
                    return false;
                }
                else
                {
                    return config.Trim().ToUpper() == "TRUE";
                }

            }
        }
        public string BaseFormTemplate { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DBMoveForm\\FormTemplate\\BaseFormTemplate.tmp"); } }
        public string BasePath
        {
            get
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), BaseName);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public DBForm(string baseName, string projectName)
        {
            this.BaseName = baseName;
            this.ProjectName = projectName;
        }

        public Action<string> MessageCallback { get; set; }

        public void ShowMessage(string msg, bool isRemoveSQL = false)
        {
            if (MessageCallback != null && !isRemoveSQL)
            {
                MessageCallback(msg);
            }
        }

        protected string GetFormName(string CRLNo, int number)
        {
            return string.Format("DBMPF_CRL{0}_{1:yyyyMMdd}_{2:00}", CRLNo, DateTime.Today, number);
        }

        public string GetTemplateContent(string filePath)
        {
            string str = string.Empty;
            using (StreamReader sr = File.OpenText(filePath))
            {
                str = sr.ReadToEnd();
            }
            return str;
        }

        public string GenerateForm(int formNumber, string CRLNo, List<DatabaseReq> databaseRequest)
        {
            string formName = GetFormName(CRLNo, formNumber);

            ShowMessage(formName);
            string content = GetTemplateContent(BaseFormTemplate);
            content = content.Replace("$$FormName$$", formName);
            content = content.Replace("$$ProjectName$$", this.ProjectName);
            content = content.Replace("$$CRLNumber$$", CRLNo);
            content = content.Replace("$$Date$$", DateTime.Today.ToString("MM/dd/yyyy"));
            content = content.Replace("$$UserName$$", Commons.GetUserName());
            CStringBuilder databaseBuilder = new CStringBuilder();
            databaseRequest.ForEach(dr =>
            {
                if (dr.ScriptFiles != null)
                {
                    CStringBuilder scriptBuilder = new CStringBuilder();
                    dr.ScriptFiles.ForEach(sf =>
                    {
                        scriptBuilder.CreateScriptFile(sf.FileName, sf.PType, sf.OType, sf.OName);
                        if (RemoveSQL)
                        {
                            File.Delete(sf.FileName);
                        }
                    });
                    databaseBuilder.CreateDatabaseRequest(dr.Server, dr.Database, scriptBuilder.ToString());
                }
            });

            content = content.Replace("$$DatabaseRequest$$", databaseBuilder.ToString());
            string formPath = Path.Combine(BasePath, formName) + ".xml";
            using (StreamWriter sw = File.CreateText(formPath))
            {
                sw.Write(content);
            }
            return formPath;
        }
    }

    public class DatabaseReq
    {
        public string Server;
        public string Database;
        public List<ScriptFile> ScriptFiles;
    }

    public class ScriptFile
    {
        public string FileName { get; set; }
        public ProcessType PType { get; set; }
        public ObjectType OType { get; set; }
        public string OName { get; set; }
    }

    public class CStringBuilder
    {
        StringBuilder builder;
        public CStringBuilder()
        {
            builder = new StringBuilder();
        }

        public CStringBuilder CreateScriptFile(string scriptFile, ProcessType pType, ObjectType oType, string objectName)
        {
            builder.AppendFormat(CStringBuilder.ScriptFile, new InfoPathAttachmentEncoder(scriptFile).ToBase64String(), pType, oType, objectName).AppendLine();
            return this;
        }

        public CStringBuilder CreateDatabaseRequest(string server, string database, string scriptFiles)
        {
            builder.AppendFormat(DatabaseRequest, server, database, scriptFiles);
            return this;
        }

        public override string ToString()
        {
            return builder.ToString();
        }

        #region StaticTemplate
        static string ScriptFile = @"<my:ScriptFile>
	<my:ScriptFileName></my:ScriptFileName>
	<my:ScriptFileContent>{0}</my:ScriptFileContent>
	<my:ObjectInfo>
		<my:ProcessType>{1}</my:ProcessType>
		<my:ObjectType>{2}</my:ObjectType>
		<my:ObjectName>{3}</my:ObjectName>
		<my:DatabaseChangeConfirmNo></my:DatabaseChangeConfirmNo>
		<my:Description></my:Description>
		<my:DatabaseChangeConfirmUrl>http://s7dbm05/DBCC/.xml</my:DatabaseChangeConfirmUrl>
	</my:ObjectInfo>
</my:ScriptFile>";

        static string DatabaseRequest = @"<my:DatabaseRequest>
    <my:Server>{0}</my:Server>
    <my:Database>{1}</my:Database>
    <my:ScriptLocation/>
    {2}
  </my:DatabaseRequest>";
        #endregion
    }

    public enum ProcessType
    {
        /// <summary>
        /// Alter
        /// </summary>
        A,
        /// <summary>
        /// Create
        /// </summary>
        C,
        /// <summary>
        /// Drop
        /// </summary>
        D,
        /// <summary>
        /// Data Insert
        /// </summary>
        DI,
        /// <summary>
        /// Data Update
        /// </summary>
        DU,
        /// <summary>
        /// Data Delete
        /// </summary>
        DD,
        /// <summary>
        /// Other
        /// </summary>
        Other
    }

    public enum ObjectType
    {
        /// <summary>
        /// Procedure
        /// </summary>
        P,
        /// <summary>
        /// View
        /// </summary>
        V,
        /// <summary>
        /// Table
        /// </summary>
        U,
        /// <summary>
        /// JOB
        /// </summary>
        JOB,
        /// <summary>
        /// DataBase
        /// </summary>
        DB,
        /// <summary>
        /// Index
        /// </summary>
        IX,
        /// <summary>
        /// Trigger
        /// </summary>
        TR
    }
}
