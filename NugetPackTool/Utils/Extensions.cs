using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using EnvDTE80;
using EnvDTE;
using NugetPackTool.Commands;

namespace NugetPackTool.Utils
{
    internal static class Extensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            return list == null || !list.Any();
        }

        public static string GetDirectoryName(this Project project)
        {
            return Path.GetDirectoryName(project.FullName);
        }

        public static string GetProjectName(this Project project)
        {
            return Path.GetFileNameWithoutExtension(project.FileName);
        }

        public static Project SelectedProject(this DTE2 dte2)
        {
            var items = (Array)dte2.ToolWindows.SolutionExplorer.SelectedItems;
            var projects = (from UIHierarchyItem selItem in items select selItem.Object).OfType<Project>();
            if (!projects.IsNullOrEmpty())
            {
                return projects.First();
            }
            return null;
        }
        
        public static string GetNuspecFullFileName(this Project project)
        {
            string directoryPath = project.GetDirectoryName();

            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return string.Empty;
            }

            return Path.Combine(directoryPath, Path.GetFileNameWithoutExtension(project.FileName) + ".nuspec");
        }

        public static string GetPackageFullFileName(this Project project)
        {
            string directoryPath = project.GetDirectoryName();

            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return string.Empty;
            }

            return Path.Combine(directoryPath, Path.GetFileNameWithoutExtension(project.FileName) + ".nuspec");
        }

        public static void RemoveAllNugetPackage(this Project project)
        {
            var diretory = new DirectoryInfo(project.GetDirectoryName());
            string search = $"{project.GetProjectName()}*.nupkg";
            var files = diretory.EnumerateFiles(search, SearchOption.TopDirectoryOnly);
            files.ToList().ForEach(it => it.Delete());
        }

        public static string GetNugetPackagePath(this Project project)
        {
            var diretory = new DirectoryInfo(project.GetDirectoryName());
            string search = $"{project.GetProjectName()}*.nupkg";
            var files = diretory.EnumerateFiles(search, SearchOption.TopDirectoryOnly);
            return files.FirstOrDefault()?.FullName ?? string.Empty;
        }

        public static string PackToNugetPackge(this Project project)
        {
            return NugetHelper.Pack(project.FullName, project.GetDirectoryName());
        }

        public static string PublishNugetPackage(this Project project, PackageKind kind)
        {
            return NugetHelper.Publish(project.GetNugetPackagePath(), kind);
        }
        
        public static bool ContainsFile(this Project project, string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) && File.Exists(fileName);
        }
    }
}
