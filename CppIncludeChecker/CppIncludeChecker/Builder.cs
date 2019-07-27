using System;
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
            public TimeSpan buildDuration { get; set; }

            public string GetBuildDurationString()
            {
                if (buildDuration.TotalMinutes > 1)
                {
                    return string.Format("{0:0.00} minutes", buildDuration.TotalMinutes);
                }
                return string.Format("{0:0.00} seconds", buildDuration.TotalSeconds);
            }
        }

        private readonly string _solutionFileFullPath;
        private readonly string _workingDirectory;
        private readonly string _builderCommand;

        public Builder(string solutionFilePath, string builderCommand)
        {
            _solutionFileFullPath = Path.GetFullPath(solutionFilePath);
            _workingDirectory = Path.GetDirectoryName(_solutionFileFullPath);
            _builderCommand = builderCommand;
        }

        public BuildResult Build()
        {
            DateTime buildStartTime = DateTime.Now;
            string msbuildArguments = string.Format("{0} /t:Build /maxcpucount", _solutionFileFullPath);
            BuildResult buildResult = new BuildResult();
            var runResult = CommandExecutor.Run(_workingDirectory, _builderCommand, msbuildArguments);
            buildResult.outputs = runResult.outputs;
            buildResult.errors = runResult.errors;
            buildResult.IsSuccess = IsBuildSuccess(buildResult.outputs);
            buildResult.buildDuration = DateTime.Now - buildStartTime;
            return buildResult;
        }

        public BuildResult Rebuild()
        {
            DateTime buildStartTime = DateTime.Now;
            string msbuildArguments = string.Format("{0} /t:Rebuild /maxcpucount", _solutionFileFullPath);
            BuildResult buildResult = new BuildResult();
            var runResult = CommandExecutor.Run(_workingDirectory, _builderCommand, msbuildArguments);
            buildResult.outputs = runResult.outputs;
            buildResult.errors = runResult.errors;
            buildResult.IsSuccess = IsBuildSuccess(buildResult.outputs);
            buildResult.buildDuration = DateTime.Now - buildStartTime;
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
