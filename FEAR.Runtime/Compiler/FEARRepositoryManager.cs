using FEAR.Domain.Infrastructure;
using FEAR.Runtime.Domain;
using FEAR.Runtime.Execution;
using FEAR.Domain.Arguments;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using FEAR.Domain.Telemetry;

namespace FEAR.Runtime.Compiler
{
    /// <summary>
    /// Manages synchronization of FEAR repositories and precompiled libraries.
    /// Responsible for ensuring that required repository resources (such as modules, rulesets, or libraries)
    /// are up-to-date and available for the application, supporting both local file system and remote (web) sources.
    /// </summary>
    public class FEARRepositoryManager : IFEARRepositoryManager
    {
        /// <summary>
        /// Gets the compiler options used for repository and library management.
        /// </summary>
        public FEARCompilerOptions CompilerOptions { get; }

        /// <summary>
        /// Gets the execution options used for runtime configuration and argument resolution.
        /// </summary>
        public FearExecutionOptions ExecutionOptions { get; }

        public IFEARTelemetrySignalService TelemetryService { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FEARRepositoryManager"/> class.
        /// </summary>
        /// <param name="executionOptions">The execution options for runtime configuration.</param>
        /// <param name="compilerOptions">The compiler options for repository and library management.</param>
        public FEARRepositoryManager(FearExecutionOptions executionOptions, FEARCompilerOptions compilerOptions, IFEARTelemetrySignalService telemetryService)
        {
            CompilerOptions = compilerOptions;
            ExecutionOptions = executionOptions;
            TelemetryService = telemetryService;
        }

        private List<IRepositorySynchronizer> repositorySynchronizers = new List<IRepositorySynchronizer>()
        {
            new GitRepositorySynchronizer(),
            new WebRepositorySynchronizer()
        };

        private PathRepositorySynchronizer defaultPathRepositoryHandler = new PathRepositorySynchronizer();

        /// <summary>
        /// Synchronizes precompiled libraries for the specified type (e.g., "GFEAR").
        /// Copies all files from configured library paths to the local precompiled directory.
        /// Supports both local file system and remote (web) repositories (remote not yet implemented).
        /// </summary>
        /// <param name="type">The type or category of precompiled libraries to synchronize.</param>
        public void SynchronizePrecompiledLibraries(string type)
        {
            if (ExecutionOptions.Arguments.PrecompiledLibraryPaths != null)
            {
                string precompiledDirectory = ExecutionOptions.Arguments.GetPreCompiledDirectory();
                Parameter<List<string>> repoPaths = null;
                if (type == "GFEAR")
                {
                    repoPaths = ExecutionOptions.Arguments.PrecompiledLibraryPaths.GFEAR;
                }
                else if (type == "CFEAR")
                {
                    repoPaths = ExecutionOptions.Arguments.PrecompiledLibraryPaths.CFEAR;
                }
                else if (type == "RFEAR")
                {
                    repoPaths = ExecutionOptions.Arguments.PrecompiledLibraryPaths.RFEAR;
                }

                if (repoPaths != null && repoPaths.IsSet)
                {
                    foreach (var repoPath in repoPaths.Value)
                    {
                        var options = new FEARCompilerOption(false, new FEARSourceOption() { Directory = precompiledDirectory, FileExtensions = new List<string>() { ".dll" } });
                        var synchronizer = repositorySynchronizers.FirstOrDefault(s => s.DoesHandleRepositoryPath(repoPath));

                        TelemetryService.SendSignal(new GenericFEARTelemetrySignal("RepositorySynchronization", FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Trace)
                            .WithSignalData($"Synchronizing Precompiled Library: {repoPath} using {synchronizer?.HandlerName ?? "<unknown>"}"));

                        if (synchronizer != null)
                        {
                            synchronizer.SynchronizeRepositoryFiles(repoPath, options, false);
                        }
                        else
                        {
                            if (defaultPathRepositoryHandler.DoesHandleRepositoryPath(repoPath))
                                defaultPathRepositoryHandler.SynchronizeRepositoryFiles(repoPath, options, false);
                        }
                    }
                }
            }
        }

        public void SynchronizePackageLibraries()
        {
            if (ExecutionOptions.Arguments.PackagedSourcesPaths != null)
            {
                if (ExecutionOptions.Arguments.PackageDirectory.IsSet &&
                    !String.IsNullOrEmpty(ExecutionOptions.Arguments.PackageDirectory.Value))
                {
                    if (Directory.Exists(ExecutionOptions.Arguments.GetPackageDirectory()))
                        Directory.Delete(ExecutionOptions.Arguments.GetPackageDirectory(), true);

                    Directory.CreateDirectory(ExecutionOptions.Arguments.GetPackageDirectory());

                    foreach (var repoPath in ExecutionOptions.Arguments.PackagedSourcesPaths)
                    {
                        var options = new FEARCompilerOption(false, new FEARSourceOption() { Directory = ExecutionOptions.Arguments.GetPackageDirectory(), FileExtensions = new List<string>() { ".zip" } });
                        var synchronizer = repositorySynchronizers.FirstOrDefault(s => s.DoesHandleRepositoryPath(repoPath));

                        TelemetryService.SendSignal(new GenericFEARTelemetrySignal("RepositorySynchronization", FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Trace)
                            .WithSignalData($"Synchronizing Packaged Library: {repoPath} using {synchronizer?.HandlerName ?? "<unknown>"}"));

                        if (synchronizer != null)
                        {
                            synchronizer.SynchronizeRepositoryFiles(repoPath, options, false);
                        }
                        else
                        {
                            if (defaultPathRepositoryHandler.DoesHandleRepositoryPath(repoPath))
                                defaultPathRepositoryHandler.SynchronizeRepositoryFiles(repoPath, options, false);
                        }

                        var zipFilesInPackageDirectory = Directory.GetFiles(ExecutionOptions.Arguments.GetPackageDirectory(), "*.zip");
                        foreach (var zipFile in zipFilesInPackageDirectory)
                        {
                            var libName = Path.GetFileNameWithoutExtension(zipFile);
                            var extractPath = Path.Combine(ExecutionOptions.Arguments.GetPackageDirectory(), libName);
                            if (!Directory.Exists(extractPath))
                            {
                                System.IO.Compression.ZipFile.ExtractToDirectory(zipFile, extractPath);
                            }

                            var gfearSourceDir = Path.Combine(extractPath, "GFEAR");
                            var rfearSourceDir = Path.Combine(extractPath, "RFEAR");

                            var gfearDestinationDir = Path.Combine(ExecutionOptions.Arguments.GetScriptDirectory(), "GFEAR", libName);
                            var rfearDestinationDir = Path.Combine(ExecutionOptions.Arguments.GetScriptDirectory(), "RFEAR", libName);

                            if (Directory.Exists(gfearDestinationDir))
                                Directory.Delete(gfearDestinationDir, true);

                            if (Directory.Exists(rfearDestinationDir))
                                Directory.Delete(rfearDestinationDir, true);
                            
                            Directory.CreateDirectory(rfearDestinationDir);
                            Directory.CreateDirectory(gfearDestinationDir);

                            // Get all files in the gfearSourceDir and copy them to the gfearDestinationDir
                            if (Directory.Exists(gfearSourceDir))
                            {
                                var gfearFiles = Directory.GetFiles(gfearSourceDir, "*.*", SearchOption.AllDirectories);
                                foreach (var file in gfearFiles)
                                {
                                    var relativePath = Path.GetRelativePath(gfearSourceDir, file);
                                    var destinationPath = Path.Combine(gfearDestinationDir, relativePath);
                                    var destinationDir = Path.GetDirectoryName(destinationPath);
                                    if (!Directory.Exists(destinationDir))
                                        Directory.CreateDirectory(destinationDir);
                                    File.Copy(file, destinationPath, true);
                                }
                            }

                            // Get all files in the rfearSourceDir and copy them to the rfearDestinationDir
                            if (Directory.Exists(rfearSourceDir))
                            {
                                var rfearFiles = Directory.GetFiles(rfearSourceDir, "*.*", SearchOption.AllDirectories);
                                foreach (var file in rfearFiles)
                                {
                                    var relativePath = Path.GetRelativePath(rfearSourceDir, file);
                                    var destinationPath = Path.Combine(rfearDestinationDir, relativePath);
                                    var destinationDir = Path.GetDirectoryName(destinationPath);
                                    if (!Directory.Exists(destinationDir))
                                        Directory.CreateDirectory(destinationDir);
                                    File.Copy(file, destinationPath, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Synchronizes the specified repository type (e.g., "GFEAR").
        /// Copies all files from configured repository paths to the local source directory.
        /// Supports both local file system and remote (web) repositories (remote not yet implemented).
        /// </summary>
        /// <param name="type">The type or category of repository to synchronize.</param>
        public void SynchronizeRepository(string type)
        {
            if (ExecutionOptions.Arguments.SourceRepositoryPaths != null)
            {
                FEARCompilerOption compilerOption = null;
                Parameter<List<string>> repoPaths = null;

                if (type == "GFEAR")
                {
                    compilerOption = CompilerOptions.GFEAROption;
                    repoPaths = ExecutionOptions.Arguments.SourceRepositoryPaths.GFEAR;
                }
                else if (type == "CFEAR")
                {
                    compilerOption = CompilerOptions.CFEAROption;
                    repoPaths = ExecutionOptions.Arguments.SourceRepositoryPaths.CFEAR;
                }
                else if (type == "RFEAR")
                {
                    compilerOption = CompilerOptions.RFEAROption;
                    repoPaths = ExecutionOptions.Arguments.SourceRepositoryPaths.RFEAR;
                }

                if (repoPaths != null && repoPaths.IsSet)
                {
                    foreach (var repoPath in repoPaths.Value)
                    {
                        var synchronizer = repositorySynchronizers.FirstOrDefault(s => s.DoesHandleRepositoryPath(repoPath));
                        var libraryName = Path.GetDirectoryName(repoPath);

                        TelemetryService.SendSignal(new GenericFEARTelemetrySignal("RepositorySynchronization", FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Trace)
                            .WithSignalData($"Synchronizing Repository: {repoPath} using {synchronizer?.HandlerName ?? "<unknown>"}"));

                        if (synchronizer != null)
                        {
                            try
                            {
                                synchronizer.SynchronizeRepositoryFiles(repoPath, compilerOption, true);
                            }
                            catch (Exception ex)
                            {
                                TelemetryService.SendSignal(new GenericFEARTelemetrySignal("RepositorySynchronizationError", FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Error)
                                    .WithSignalData($"Error synchronizing repository: {repoPath}. Exception: {ex.Message}"));
                            }
                        }
                        else
                        {
                            if (defaultPathRepositoryHandler.DoesHandleRepositoryPath(repoPath))
                                defaultPathRepositoryHandler.SynchronizeRepositoryFiles(repoPath, compilerOption, true);
                        }
                    }
                }
            }
        }
    }
}