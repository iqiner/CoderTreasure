//------------------------------------------------------------------------------
// <copyright file="PublishStableCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using NugetPackTool.Utils;

namespace NugetPackTool.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class PublishStableCommand : CommandBase<PublishStableCommand>
    {
        protected override Guid CommandSet { get; } = new Guid("d5436e9a-4d02-445c-a805-e567afd5304e");
        
        protected override void MenuItemCallback(object sender, EventArgs e)
        {
            var project = this.DTE2.SelectedProject();
            var outputMessage = project.PublishNugetPackage(PackageKind.Stable);
            WriteToOutputWindow(outputMessage);
        }
    }
}
