using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyRelativePathUnixStyle
{
    internal static class DTEHelper
    {
        public static DTE GetDTE()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return Package.GetGlobalService(typeof(DTE)) as DTE;
        }

        public static SelectedItems GetSelectedItems(DTE dte)
        {
            //var selectedItems = dte.ToolWindows.SolutionExplorer.SelectedItems as object[];
            //return selectedItems;
            ThreadHelper.ThrowIfNotOnUIThread();
            var selectedItems = dte.SelectedItems;
            return selectedItems;
        }

        public static string GetProjectFilePath(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return project.FullName;
        }
    }
}
