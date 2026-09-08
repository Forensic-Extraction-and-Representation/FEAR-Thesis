namespace FEAR.Runtime.Domain
{
    public interface IRepositorySynchronizer
    {
        string HandlerName { get; }
        bool DoesHandleRepositoryPath(string repoPath);
        void SynchronizeRepositoryFiles(string repoPath, FEARCompilerOption options, bool WithLibraryFolder = false);
    }
}