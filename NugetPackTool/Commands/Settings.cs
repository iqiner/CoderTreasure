using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using NugetPackTool.Nuget;
using NugetPackTool.Utils;

namespace NugetPackTool.Commands
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            var configs = NugetHelper.GetNugetSourceConfig();
            foreach (var config in configs)
            {
                if (config.PackageKind == PackageKind.Alpha)
                {
                    this.txtAlphaSource.Text = config.NugetSource;
                    this.txtAlphaPwd.Text = config.Password;
                }

                if (config.PackageKind == PackageKind.Stable)
                {
                    this.txtStableSource.Text = config.NugetSource;
                    this.txtStablePwd.Text = config.Password;
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            var configs = new List<NugetSourceInfo>
            {
                new NugetSourceInfo
                {
                    NugetSource = this.txtAlphaSource.Text.Trim(),
                    Password = this.txtAlphaPwd.Text.Trim(),
                    PackageKind = PackageKind.Alpha
                },
                new NugetSourceInfo
                {
                    NugetSource = this.txtStableSource.Text.Trim(),
                    Password = this.txtStablePwd.Text.Trim(),
                    PackageKind = PackageKind.Stable
                }
            };
            
            NugetHelper.SaveNugetSourceConfig(configs);

            this.Close();
        }
    }
}
