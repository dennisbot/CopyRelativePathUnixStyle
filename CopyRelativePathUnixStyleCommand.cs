using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using Task = System.Threading.Tasks.Task;

namespace CopyRelativePathUnixStyle
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CopyRelativePathUnixStyleCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("5A78A6F1-6134-44A8-BCCC-0E02F4CADA33");
        /// <summary>?
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyRelativePathUnixStyleCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CopyRelativePathUnixStyleCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CopyRelativePathUnixStyleCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in Command1's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CopyRelativePathUnixStyleCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IntPtr hierarchyPointer, selectionContainerPointer;
            Object selectedObject = null;
            IVsMultiItemSelect multiItemSelect;
            uint projectItemId;

            IVsMonitorSelection monitorSelection =
                (IVsMonitorSelection)Package.GetGlobalService(
                    typeof(SVsShellMonitorSelection));

            monitorSelection.GetCurrentSelection(out hierarchyPointer,
                out projectItemId,
                out multiItemSelect,
                out selectionContainerPointer);

            IVsHierarchy selectedHierarchy = Marshal.GetTypedObjectForIUnknown(
                hierarchyPointer,
                typeof(IVsHierarchy)) as IVsHierarchy;

            if (selectedHierarchy != null)
            {
                selectedHierarchy.GetProperty(projectItemId,
                    (int)__VSHPROPID.VSHPROPID_ExtObject,
                    out selectedObject);
                ProjectItem selectedProjectItem = selectedObject as ProjectItem;
                if (selectedProjectItem != null)
                {
                    Project selectedProject = selectedProjectItem.ContainingProject;
                    Solution solution = selectedProject.DTE.Solution;
                    string solutionFullName = solution.FullName;
                    //string projectFullName = selectedProject.FullName;
                    string filePath = selectedProjectItem.Properties.Item("FullPath").Value.ToString();
                    // Get the folder path of the csproj file
                    //string projectFolderPath = Path.GetDirectoryName(projectFullName);
                    // Get the folder path of the sln file
                    string solutionFolderPath = Path.GetDirectoryName(solutionFullName);
                    filePath = filePath.Replace(solutionFolderPath, "");
                    filePath = ReplaceBackslashesWithSlashes(filePath);
                    Clipboard.SetText(filePath);
                }
            }
        }
        private string ReplaceBackslashesWithSlashes(string text)
        {
            return text.Replace("\\", "/").TrimStart('/');
        }

        private static void ShowMessage(string message, AsyncPackage package)
        {
            
            VsShellUtilities.ShowMessageBox(
                package,
                message,
                null,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
