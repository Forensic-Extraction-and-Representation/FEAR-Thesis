using FEAR.Runtime.Domain;

namespace FEAR.Runtime.Compiler
{
    public class PathRepositorySynchronizer : IRepositorySynchronizer
    {
        public string HandlerName => "PathRepositorySynchronizer";

        public bool DoesHandleRepositoryPath(string repoPath)
        {
            return Directory.Exists(repoPath) || File.Exists(repoPath);
        }

        public void SynchronizeRepositoryFiles(string repoPath, FEARCompilerOption options, bool withAssemblyFolder)
        {
            string fearSourceRootDirectory = options.SourceOption.Directory;

            if (File.Exists(repoPath)) {
                // If the repoPath is a file, copy it directly to the target directory
                CopyFileToLocalDirectory(options, fearSourceRootDirectory, repoPath);
            }
            else
            {
                var directories = Directory.GetDirectories(repoPath);
                if (directories.Length == 0)
                {
                    directories = new string[] { repoPath };
                }

                foreach (var dir in directories)
                {
                    // Get the last folder in the libraryPath.Location
                    string libraryName = dir.Split(Path.DirectorySeparatorChar).Last();

                    // Ensure the target directory exists
                    string fearSourceDirectory = Path.Combine(fearSourceRootDirectory, libraryName);
                    if (!Directory.Exists(fearSourceDirectory))
                        Directory.CreateDirectory(fearSourceDirectory);

                    try
                    {
                        // Copy all files from the repository path to the target directory
                        foreach (var file in Directory.GetFiles(dir).ToList())
                        {
                            CopyFileToLocalDirectory(options, fearSourceDirectory, file);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error if file copy fails
                        Console.WriteLine($"Error copying files from {dir} to {fearSourceDirectory}: {ex.Message}");
                    }
                }
            }
        }

        private static void CopyFileToLocalDirectory(FEARCompilerOption options, string sourceDirectory, string file)
        {
            string fileName = Path.GetFileName(file);
            string fileExt = Path.GetExtension(file);
            if (options.SourceOption.FileExtensions.Contains(fileExt))
            {
                string destFile = Path.Combine(sourceDirectory, fileName);
                using (var sourceFile = File.OpenRead(file))// Ensure the file can be read
                {
                    using (var destFileStream = File.OpenWrite(destFile))
                    {
                        // Ensure the destination file is created or overwritten
                        if (File.Exists(destFile))
                            destFileStream.SetLength(0); // Clear the file if it exists
                        sourceFile.CopyTo(destFileStream);
                    }
                }
            }
        }
    }
}
