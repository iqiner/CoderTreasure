using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace NugetPackTool.Commands
{
    interface IPackage
    {
        Package Package { get; set; }

        void SetMenuItemCallback(Package package);
    }

    public enum PackageKind
    {
        Alpha = 0,
        Stable = 1
    }

    internal abstract class CommandBase<T> : IPackage
        where T : IPackage, new()
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        protected virtual int CommandId { get; } = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        protected abstract Guid CommandSet { get; }

        public Package Package { get; set; }

        private IServiceProvider ServiceProvider => this.Package;

        protected DTE2 DTE2 => (DTE2)this.ServiceProvider.GetService(typeof(DTE));

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            var instance = new T() {Package = package};
            instance.SetMenuItemCallback(package);
        }

        public void SetMenuItemCallback(Package package)
        {
            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        protected abstract void MenuItemCallback(object sender, EventArgs e);

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        //public static T Instance { get; private set; }
        
        protected void ShowMessage(string message, string title)
        {
            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        protected void WriteToOutputWindow(string message)
        {
            var outputWin = this.DTE2.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
            outputWin.WindowState = vsWindowState.vsWindowStateNormal;
            outputWin.AutoHides = false;
            outputWin.Activate();
            
            var window = this.DTE2.ToolWindows.OutputWindow;
            var panel = window.OutputWindowPanes.Add("Nuget Pack Tool");
            panel.OutputString(message);
            panel.Activate();
        }
    }
}
