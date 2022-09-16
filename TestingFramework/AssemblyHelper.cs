using System;
using System.IO;
using System.Reflection;

namespace ModelSync.App.Helpers
{
    public static class AssemblyHelper
    {
        public static Assembly LoadReflectionOnlyDependencies(object sender, ResolveEventArgs args)
        {
            if (FindNugetPackageFromAssemblyName(args.Name, out string fileName))
            {
                return Assembly.ReflectionOnlyLoadFrom(fileName);
            }
            else
            {
                try
                {
                    return Assembly.ReflectionOnlyLoad(args.Name);
                }
                catch (Exception exc)
                {
                    throw new Exception($"Couldn't find dependency info for {args.Name}", exc);
                }
            }
        }

        public static bool FindNugetPackageFromAssemblyName(string assemblyName, out string fileName)
        {
            var name = new AssemblyName(assemblyName);

            try
            {
                fileName = GetNugetPackageDllPath(name);
                return true;
            }
            catch
            {
                fileName = null;
                return false;
            }
        }

        private static string GetNugetPackageDllPath(AssemblyName assemblyName)
        {
            string versionFolder(Version version)
            {
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }

            string searchPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget", "packages", assemblyName.Name, versionFolder(assemblyName.Version));

            string searchName = $"{assemblyName.Name}.dll";

            return FileSearchR(searchPath, searchName);
        }

        private static string FileSearchR(string path, string fileName)
        {
            string[] files = Directory.GetFiles(path);
            foreach (string fullPath in files)
            {
                if (Path.GetFileName(fullPath).Equals(fileName)) return fullPath;
            }

            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                return FileSearchR(folder, fileName);
            }

            throw new FileNotFoundException($"Couldn't find {fileName} in {path}");
        }
    }


}
