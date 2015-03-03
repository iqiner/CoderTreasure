using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DBExport
{
    public partial class Main : Form
    {
        private string dataSource;
        private string initialCatalog;
        private string userName;
        private string password;
        private string tableName;
        private string fields;
        private string exportPath;
        private string exportCondition;
        private string sqlStatements;
        
        public Main()
        {
            InitializeComponent();
            this.txtDataSource.Text = "S7SQL01";
            this.txtInitialCatalog.Text = "DropShip";
            this.txtTableName.Text = "DropshipMaster";
            this.txtUserName.Text = "WHDbo";
            this.txtPassword.Text = "2Dev4WH";
            this.txtFields.Text = "";
            this.txtCondition.Text = "";
            this.txtExportPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\ExportData.sql";

            this.saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        public string TableFullName
        {
            get
            {
                return this.initialCatalog + ".dbo." + this.tableName;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            this.InitialData();
            string message = string.Empty;
            if (!this.CheckData(out message))
            {
                MessageBox.Show(message);
                return;
            }

            try
            {
                Export entity = new Export(this.dataSource, this.initialCatalog, this.userName, this.password, this.fields, this.TableFullName, this.exportCondition);
                entity.NeedSeperate = this.ckbBatch.Checked;
                entity.BatchCount = int.Parse(this.txtBatchCount.Text);
                //Export entity = new ExportTableStruct(this.dataSource, this.initialCatalog, this.userName, this.password, this.fields, this.TableFullName, this.exportCondition);
                bool isSuccess = entity.ExportData(this.exportPath);
                MessageBox.Show(isSuccess ? "导出成功" : "导出失败");
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出失败，"+ex.Message);
            }
        }

        private void InitialData()
        {
            this.dataSource = this.txtDataSource.Text.Trim();
            this.initialCatalog = this.txtInitialCatalog.Text.Trim();
            this.userName = this.txtUserName.Text.Trim();
            this.password = this.txtPassword.Text.Trim();
            this.tableName = this.txtTableName.Text.Trim();
            this.fields = this.txtFields.Text.Trim();
            this.exportPath = this.txtExportPath.Text.Trim();
            this.exportCondition = this.txtCondition.Text.Trim();
        }

        private bool CheckData(out string message)
        {
            message = "OK";
            if (string.IsNullOrEmpty(this.userName))
            {
                message = "请输入用户名";
                return false;
            }
            if (string.IsNullOrEmpty(this.password))
            {
                message = "请输入密码";
                return false;
            }
            if (string.IsNullOrEmpty(this.dataSource))
            {
                message = "请输入服务器地址";
                return false;
            }
            if (string.IsNullOrEmpty(this.initialCatalog))
            {
                message = "请输入需要导出的数据库名";
                return false;
            }
            if (string.IsNullOrEmpty(this.tableName))
            {
                message = "请输入需要导出的表名";
                return false;
            }
            if (string.IsNullOrEmpty(this.fields))
            {
                message = "请输入需要导出的字段";
                return false;
            }
            return true;
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            this.saveFileDialog.FileName = string.IsNullOrEmpty(this.txtTableName.Text.Trim()) ? "export.sql" : this.txtTableName.Text.Trim() + ".sql";
            if (this.saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.exportPath = this.saveFileDialog.FileName;
                this.txtExportPath.Text = this.exportPath;
            }
        }

        private void ParseTableDataSource(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }
            
            string[] splitStr = (sender as TextBox).Text.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (splitStr.Length > 1)
            {
                this.txtInitialCatalog.Text = splitStr[0];
                this.txtTableName.Text =string.Join(".", splitStr.Skip(1).ToArray());
            }
        }

        private void ckbBatch_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = this.ckbBatch.Checked;
            this.txtBatchCount.Visible = isChecked;
            if (isChecked)
            {
                this.txtBatchCount.Text = "2000";
            }
        }
    }
}
