//------------------------------------------------------------------------------
// <copyright file="GenerateNuSpecCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Windows.Forms;

namespace NugetPackTool.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class NugetSettingsCommand : CommandBase<NugetSettingsCommand>
    {
        protected override Guid CommandSet { get; } = new Guid("00A0E21F-707B-412E-84C1-77D628870CAD");

        protected override void MenuItemCallback(object sender, EventArgs e)
        {
            Form form = new Settings();
            form.ShowDialog();
        }
    }
}
