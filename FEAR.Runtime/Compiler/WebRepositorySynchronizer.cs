using AngleSharp.Dom;
using FEAR.Runtime.Domain;

namespace FEAR.Runtime.Compiler
{
    public class WebRepositorySynchronizer : IRepositorySynchronizer
    {
        public class WebRepositoryManifest
        {
            public string Ontology { get; set; }
            public string Version { get; set; }
            public string RepositoryUrl { get; set; }
            public List<string> Files { get; set; }
        }

        public string HandlerName => "WebRepositorySynchronizer";

        public bool DoesHandleRepositoryPath(string repoPath)
        {
            return Uri.IsWellFormedUriString(repoPath, UriKind.Absolute) && (repoPath.StartsWith("http://") || repoPath.StartsWith("https://"));
        }

        private void SynchronizeFromManifest(string manifestUrl, string destinationDirectory, bool WithLibraryFolder)
        {
            // Download the manifest file
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(manifestUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var manifestContent = response.Content.ReadAsStringAsync().Result;
                    // Parse the manifest content and download each file listed in it
                    // This is a placeholder for actual manifest parsing logic
                    // For now, we assume the manifest contains a list of file URLs to download
                    var manifestFile = System.Text.Json.JsonSerializer.Deserialize<WebRepositoryManifest>(manifestContent);

                    // Remove "/manifest.json" from the manifest URL to get the base URL for the files
                    var baseUrl = new Uri(manifestUrl.Substring(0, manifestUrl.LastIndexOf("/manifest.json")));
                    var libraryName = WithLibraryFolder ? baseUrl.Segments.LastOrDefault()?.TrimEnd('/') ?? "" : "";
                    string sourceDirectory = Path.Combine(destinationDirectory, libraryName);

                    if (!Directory.Exists(sourceDirectory))
                        Directory.CreateDirectory(sourceDirectory);

                    // Write the manifest file to the source directory
                    var manifestFilePath = Path.Combine(sourceDirectory, "manifest.json");
                    File.WriteAllText(manifestFilePath, manifestContent);

                    string absolutePath = baseUrl.AbsolutePath;
                    absolutePath = absolutePath.EndsWith('/') ? absolutePath : absolutePath + "/";
                    string baseUrlString = baseUrl.ToString();
                    baseUrl = new Uri(baseUrlString.Substring(0, baseUrlString.Length - absolutePath.Length+1));
                    using (var fileClient = new HttpClient())
                    {
                        fileClient.BaseAddress = baseUrl;
                        foreach (var fileUrl in manifestFile.Files)
                        {
                            var fileName = Path.GetFileName(fileUrl);
                            var destinationPath = Path.Combine(sourceDirectory, fileName);
                            var fileResponse = fileClient.GetAsync(absolutePath+fileUrl).Result;
                            if (fileResponse.IsSuccessStatusCode)
                            {
                                File.WriteAllBytes(destinationPath, fileResponse.Content.ReadAsByteArrayAsync().Result);
                            }
                        }
                    }
                }
            }
        }

        private void SynchronizeFromPackage(string packageUrl, string destinationDirectory, bool withAssemblyFolder)
        {
            var packageUri = new Uri(packageUrl);
            var libraryName = packageUri.Segments.LastOrDefault()?.TrimEnd('/') ?? "";
            using (var fileClient = new HttpClient())
            {
                var response = fileClient.GetAsync(packageUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    File.WriteAllBytes(Path.Combine(destinationDirectory, libraryName), response.Content.ReadAsByteArrayAsync().Result);
                }
            }
        }

        public void SynchronizeRepositoryFiles(string repoPath, FEARCompilerOption options, bool withAssemblyFolder)
        {
            string destinationDirectory = options.SourceOption.Directory;

            // Create a web request to the repo path + "/manifest.json", if "manifest.json" is not at the end of the URL
            if (repoPath.EndsWith("/manifest.json"))
            {
                SynchronizeFromManifest(repoPath, destinationDirectory, withAssemblyFolder);
            }
            else if (repoPath.EndsWith(".zip"))
            {
                SynchronizeFromPackage(repoPath, destinationDirectory, withAssemblyFolder);
            }
            else
            {
                SynchronizeFromManifest(repoPath + "/manifest.json", destinationDirectory, withAssemblyFolder);
            }
        }

    }
}
