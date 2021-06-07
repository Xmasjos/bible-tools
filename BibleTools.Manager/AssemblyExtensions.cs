using System;
using System.IO;
using System.Reflection;

namespace BibleTools.Manager
{
    public static class AssemblyExtensions
    {
        public static string GetProjectPath(this Assembly assembly)
        {
#if DEBUG
            var assemblyPath = Path.GetFullPath(new Uri(assembly.Location).AbsolutePath);
            var projectPath = assemblyPath;
            var indexOfBin = projectPath.IndexOf("bin");

            if (indexOfBin != -1)
                projectPath = projectPath.Remove(indexOfBin);

            if (string.IsNullOrWhiteSpace(projectPath))
                return null;

            return projectPath.EndsWith(Path.DirectorySeparatorChar) ? projectPath.Remove(projectPath.Length - 1) : projectPath;
#else
            return Path.GetFullPath("./");
#endif
        }

        public static string GetSolutionPath(this Assembly assembly)
        {
#if DEBUG
            var projectPath = assembly.GetProjectPath();
            var lastDirSepChar = projectPath.LastIndexOf(Path.DirectorySeparatorChar);

            return lastDirSepChar == -1 ? projectPath : projectPath.Remove(lastDirSepChar);
#else
            return Path.GetFullPath("./");
#endif
        }
    }
}