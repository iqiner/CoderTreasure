//------------------------------------------------------------------------------
// <copyright file="GenerateNuSpecCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using EnvDTE;
using NugetPackTool.Utils;

namespace NugetPackTool.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GenerateNuSpecCommand : CommandBase<GenerateNuSpecCommand>
    {
        protected override Guid CommandSet { get; } = new Guid("8CF07AE5-AD0A-4DCF-B61C-9446D703E97A");

        protected override void MenuItemCallback(object sender, EventArgs e)
        {
            var project = this.DTE2.SelectedProject();
            var fullFilePath = project.GetNuspecFullFileName();
            if (!project.ContainsFile(fullFilePath))
            {
                string template = NugetHelper.ReadNuspecTemplate();
                using (var writer = File.CreateText(fullFilePath))
                {
                    writer.Write(template);
                }
            }

            this.DTE2.ItemOperations.OpenFile(fullFilePath, Constants.vsDocumentKindText);
        }
    }
}
