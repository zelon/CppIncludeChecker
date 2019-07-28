using System;
using System.IO;

namespace CppIncludeChecker
{
    class Builder
    {
        private enum BuildType
        {
            Build,
            Rebuild
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
            return DoBuild(BuildType.Build);
        }

        public BuildResult Rebuild()
        {
            return DoBuild(BuildType.Rebuild);
        }

        private BuildResult DoBuild(BuildType buildType)
        {
            DateTime buildStartTime = DateTime.Now;
            string msbuildArguments = string.Format("{0} /t:{1} /maxcpucount", _solutionFileFullPath, buildType.ToString());
            var runResult = CommandExecutor.Run(_workingDirectory, _builderCommand, msbuildArguments);

            return new BuildResult(
                runResult.outputs,
                runResult.errors,
                buildDuration: DateTime.Now - buildStartTime);
        }
    }
}
