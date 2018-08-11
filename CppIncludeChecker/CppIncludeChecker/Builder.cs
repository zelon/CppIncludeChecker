using System.IO;
using System.Collections.Generic;

namespace CppIncludeChecker
{
    class Builder
    {
        public class BuildResult
        {
            public bool IsSuccess { get; set; }
            public List<string> outputs;
            public List<string> errors;
        }

        private readonly string _solutionFileFullPath;
        private readonly string _workingDirectory;
        private readonly string kMsBuildCommand = @"E:\git\CppIncludeChecker\CppIncludeChecker\CppIncludeChecker\msbuild.bat";

        public Builder(string solutionFilePath)
        {
            _solutionFileFullPath = Path.GetFullPath(solutionFilePath);
            _workingDirectory = Path.GetDirectoryName(_solutionFileFullPath);
        }

        public BuildResult Build()
        {
            string msbuildArguments = string.Format("{0} /t:Build /maxcpucount", _solutionFileFullPath);
            BuildResult buildResult = new BuildResult();
            var runResult = CommandExecutor.Run(_workingDirectory, kMsBuildCommand, msbuildArguments);
            buildResult.outputs = runResult.outputs;
            buildResult.errors = runResult.errors;
            buildResult.IsSuccess = IsBuildSuccess(buildResult.outputs);
            return buildResult;
        }

        public BuildResult Rebuild()
        {
            string msbuildArguments = string.Format("{0} /t:Rebuild /maxcpucount", _solutionFileFullPath);
            BuildResult buildResult = new BuildResult();
            var runResult = CommandExecutor.Run(_workingDirectory, kMsBuildCommand, msbuildArguments);
            buildResult.outputs = runResult.outputs;
            buildResult.errors = runResult.errors;
            buildResult.IsSuccess = IsBuildSuccess(buildResult.outputs);
            return buildResult;
        }

        private bool IsBuildSuccess(List<string> output)
        {
            foreach (string line in output)
            {
                if (line.StartsWith("Build succeeded"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
