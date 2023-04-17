using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.OLE.Interop;
using static System.Net.Mime.MediaTypeNames;
using EnvDTE80;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

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
        public static readonly Guid CommandSet = new Guid("C537077D-EF41-4871-815B-72075A045BF6");
        /// <summary>
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
            this.Logger = new FileLogger("C:\\Users\\dennisbot\\temp\\logs\\CopyRelativeFilePathCommand.log");
            Logger.Log("Starting extension...");

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

        public FileLogger Logger { get; }

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
            this.Logger.Log("executing Execute ...");
            var selectedItems = (Array)DTEHelper.GetSelectedItems(DTEHelper.GetDTE());
            this.Logger.Log("selectedItems array cast worked!!!");
            if (selectedItems != null && selectedItems.Length == 1)
            {
                var selectedItem = selectedItems.GetValue(0) as ProjectItem;
                if (selectedItem != null)
                {
                    var path = selectedItem.Properties.Item("FullPath").Value.ToString();
                    this.Logger.Log($"selectedItem path value: {path}");
                    var projectPath = Path.GetDirectoryName(DTEHelper.GetProjectFilePath(selectedItem.ContainingProject));
                    this.Logger.Log($"projectPath = {projectPath}");
                    var relativePath = GetRelativePath(projectPath, path);
                    this.Logger.Log($"relativePath = {relativePath}");
                    if (relativePath != null)
                    {
                        Clipboard.SetText(relativePath.Replace('\\', '/'));
                    }
                    else
                    {
                        ShowMessage("Cannot determine relative path.");
                    }
                }
                else
                {
                    Logger.Log("there is no selectedItem");
                }
            }
        }
        private static string GetRelativePath(string basePath, string fullPath)
        {
            var baseUri = new Uri(basePath);
            var fullUri = new Uri(fullPath);
            if (baseUri.Scheme == fullUri.Scheme && baseUri.Host == fullUri.Host)
            {
                var relativeUri = baseUri.MakeRelativeUri(fullUri);
                return Uri.UnescapeDataString(relativeUri.ToString());
            }
            else
            {
                return null;
            }
        }
        private static System.IServiceProvider GetServiceProvider(Microsoft.VisualStudio.Shell.IAsyncServiceProvider asyncServiceProvider)
        {
            // Get the service provider from the IAsyncServiceProvider.
            var serviceProvider = asyncServiceProvider
                .GetServiceAsync(typeof(System.IServiceProvider)) as System.IServiceProvider;

            return serviceProvider;
        }
        private static void ShowMessage(string message)
        {
            VsShellUtilities.ShowMessageBox(
                GetServiceProvider(Instance.ServiceProvider),
                message,
                null,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
