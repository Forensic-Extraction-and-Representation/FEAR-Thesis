using FEAR.Runtime.Domain;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace FEAR.Runtime.Compiler
{
    /// <summary>
    /// Synchronizes a FEAR repository from a remote Git host (GitHub, GitLab, etc.).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Repository paths must use the <c>git+http://</c> or <c>git+https://</c> scheme.
    /// An optional <c>#ref</c> fragment can name a specific branch, tag, or commit SHA:
    /// </para>
    /// <code>
    ///   git+https://github.com/example/my-repo.git
    ///   git+https://github.com/example/my-repo.git#main
    ///   git+https://github.com/example/my-repo.git#v2.1.0
    /// </code>
    /// <para>
    /// On first use the repository is shallow-cloned into a local cache directory
    /// (<c>%TEMP%/fear-git-cache/&lt;url-hash&gt;</c>). Subsequent calls fetch and reset
    /// to the latest state of the target ref without re-cloning.
    /// </para>
    /// </remarks>
    public class GitRepositorySynchronizer : IRepositorySynchronizer
    {
        private const string SchemePrefix = "git+";

        public string HandlerName => "GitRepositorySynchronizer";

        /// <inheritdoc />
        public bool DoesHandleRepositoryPath(string repoPath) =>
            repoPath.StartsWith("git+https://", StringComparison.OrdinalIgnoreCase) ||
            repoPath.StartsWith("git+http://",  StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Strips the <c>git+</c> prefix, extracts an optional sub-path after the <c>.git</c>
        /// extension, and separates the optional <c>#ref</c> fragment.
        /// </summary>
        /// <returns>
        /// A tuple of the clean repository URL (up to and including <c>.git</c>), an optional
        /// sub-path within the repository (the segment after <c>.git/</c>), and an optional
        /// Git ref from the <c>#ref</c> fragment.
        /// </returns>
        private static (string repoUrl, string? subPath, string? gitRef) ParseRepoPath(string repoPath)
        {
            var rawUrl = repoPath[SchemePrefix.Length..];

            // Extract the optional #ref fragment.
            string? gitRef = null;
            var hashIndex = rawUrl.LastIndexOf('#');
            if (hashIndex >= 0)
            {
                gitRef = rawUrl[(hashIndex + 1)..];
                rawUrl = rawUrl[..hashIndex];
            }

            // Extract an optional sub-path that follows the ".git/" segment.
            string? subPath = null;
            var dotGitSlash = rawUrl.IndexOf(".git/", StringComparison.OrdinalIgnoreCase);
            if (dotGitSlash >= 0)
            {
                var candidate = rawUrl[(dotGitSlash + ".git/".Length)..].Trim('/');
                if (!string.IsNullOrEmpty(candidate))
                {
                    subPath = candidate;
                    rawUrl  = rawUrl[..(dotGitSlash + ".git".Length)];
                }
            }

            return (rawUrl, subPath, gitRef);
        }

        /// <summary>
        /// Returns a stable local directory path for caching the clone of <paramref name="url"/>.
        /// The directory name is derived from the first 16 hex characters of SHA-256(<paramref name="url"/>).
        /// </summary>
        private static string GetCacheDirectory(string url)
        {
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url)));
            return Path.Combine(Path.GetTempPath(), "fear-git-cache", hash[..16].ToLowerInvariant());
        }

        /// <summary>
        /// Runs a <c>git</c> command in <paramref name="workingDirectory"/> and throws if it fails.
        /// </summary>
        private static void RunGit(string arguments, string workingDirectory)
        {
            var psi = new ProcessStartInfo("git", arguments)
            {
                WorkingDirectory    = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute  = false,
                CreateNoWindow   = true
            };

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start the git process. Ensure git is installed and available on PATH.");

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var stderr = process.StandardError.ReadToEnd().Trim();
                throw new InvalidOperationException(
                    $"git {arguments} failed (exit code {process.ExitCode}){(string.IsNullOrEmpty(stderr) ? "." : $": {stderr}")}");
            }
        }

        /// <summary>
        /// Ensures the cache directory contains an up-to-date shallow clone of
        /// <paramref name="url"/> at the specified <paramref name="gitRef"/>.
        /// </summary>
        private static void EnsureCloneUpToDate(string url, string? gitRef, string cacheDir)
        {
            if (!Directory.Exists(cacheDir) || !Directory.Exists(Path.Combine(cacheDir, ".git")))
            {
                // First time: shallow clone directly into the cache directory.
                if (Directory.Exists(cacheDir))
                    Directory.Delete(cacheDir, recursive: true);
                Directory.CreateDirectory(cacheDir);

                var cloneArgs = gitRef != null
                    ? $"clone --depth 1 --branch \"{gitRef}\" \"{url}\" ."
                    : $"clone --depth 1 \"{url}\" .";

                RunGit(cloneArgs, cacheDir);
            }
            else
            {
                // Subsequent runs: fetch and hard-reset to keep the working tree clean.
                var fetchArgs = gitRef != null
                    ? $"fetch --depth 1 origin \"{gitRef}\""
                    : "fetch --depth 1";

                RunGit(fetchArgs, cacheDir);
                RunGit("reset --hard FETCH_HEAD", cacheDir);
            }
        }

        /// <summary>
        /// Copies files matching <see cref="FEARSourceOption.FileExtensions"/> from
        /// <paramref name="sourceDir"/> into <paramref name="options"/>.SourceOption.Directory.
        /// </summary>
        /// <remarks>
        /// When <paramref name="subPath"/> is provided the named sub-folder of the cloned
        /// repository is treated as a single named library: files are copied directly from that
        /// directory and <paramref name="subPath"/> is used as the library folder name (when
        /// <paramref name="withLibraryFolder"/> is <see langword="true"/>).
        /// Without a sub-path the original behaviour applies: the subdirectory structure one
        /// level deep is mirrored (skipping <c>.git</c>).
        /// </remarks>
        private static void CopyMatchingFiles(string sourceDir, FEARCompilerOption options, bool withLibraryFolder, string? subPath = null)
        {
            string destinationRoot = options.SourceOption.Directory;
            var extensions = options.SourceOption.FileExtensions;

            if (subPath != null)
            {
                // A specific sub-folder was requested; treat it as a single named library.
                var actualSourceDir = Path.Combine(sourceDir, subPath);
                if (!Directory.Exists(actualSourceDir))
                    throw new DirectoryNotFoundException(
                        $"Sub-path '{subPath}' was not found in the cloned repository at '{sourceDir}'.");

                string libraryName = withLibraryFolder ? subPath : string.Empty;
                string targetDir = string.IsNullOrEmpty(libraryName)
                    ? destinationRoot
                    : Path.Combine(destinationRoot, libraryName);

                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                foreach (var file in Directory.GetFiles(actualSourceDir))
                {
                    if (extensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                        File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), overwrite: true);
                }

                return;
            }

            var subDirectories = Directory
                .GetDirectories(sourceDir)
                .Where(d => !Path.GetFileName(d).Equals(".git", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // If the repository has no subdirectories, treat the root itself as the source.
            var sourceDirs = subDirectories.Length > 0 ? subDirectories : new[] { sourceDir };

            foreach (var dir in sourceDirs)
            {
                string libraryName = withLibraryFolder ? Path.GetFileName(dir) : string.Empty;
                string targetDir = string.IsNullOrEmpty(libraryName)
                    ? destinationRoot
                    : Path.Combine(destinationRoot, libraryName);

                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                foreach (var file in Directory.GetFiles(dir))
                {
                    if (extensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                    {
                        File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), overwrite: true);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void SynchronizeRepositoryFiles(string repoPath, FEARCompilerOption options, bool withAssemblyFolder = false)
        {
            var (url, subPath, gitRef) = ParseRepoPath(repoPath);
            var cacheDir = GetCacheDirectory(url);

            EnsureCloneUpToDate(url, gitRef, cacheDir);
            CopyMatchingFiles(cacheDir, options, withAssemblyFolder, subPath);
        }
    }
}
