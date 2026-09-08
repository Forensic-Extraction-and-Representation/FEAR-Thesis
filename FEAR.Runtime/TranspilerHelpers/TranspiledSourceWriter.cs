using FEAR.Domain.Arguments;
using FEAR.Runtime.Domain;
using System.IO;

namespace FEAR.Runtime.TranspilerHelpers
{
    public static class TranspiledSourceWriter
    {
        public static void WriteIfEnabled(string langFolder, string sourceFileName, string sourceCode, FEARCompilationRequest options)
        {
            try
            {
                var args = options?.ExecutionOptions?.Arguments as FearArguments;
                if (args == null || args.OutputTranspiledSource?.Value != true)
                    return;

                var root = args.GetTranspiledSourceOutputDirectory();
                var libName = Path.GetFileNameWithoutExtension(options?.OutFile ?? "library");
                var fileName = Path.GetFileNameWithoutExtension(sourceFileName ?? "transpiled") + ".cs";
                var targetDir = Path.Combine(root, langFolder, libName);
                Directory.CreateDirectory(targetDir);
                File.WriteAllText(Path.Combine(targetDir, fileName), sourceCode);
            }
            catch
            {
                // swallow to avoid breaking the pipeline
            }
        }
    }
}