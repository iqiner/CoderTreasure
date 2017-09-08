//------------------------------------------------------------------------------
// <copyright file="NugetPackTool.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NugetPackTool.Utils;

namespace NugetPackTool.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class PackCommand : CommandBase<PackCommand>
    {
        protected override Guid CommandSet { get; } = new Guid("abb2d384-6dc6-4d27-91dc-be219b5385ed");

        protected override void MenuItemCallback(object sender, EventArgs e)
        {
            var project = this.DTE2.SelectedProject();
            project.RemoveAllNugetPackage();
            var outputMessage = project.PackToNugetPackge();
            WriteToOutputWindow(outputMessage);
        }
    }
}
