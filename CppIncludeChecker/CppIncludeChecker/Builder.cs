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

        private readonly string _builderCommand;
        private readonly string _solutionFileFullPath;
        private readonly string _buildConfiguration;
        private readonly string _buildPlatform;
        private readonly string _workingDirectory;

        public Builder(string builderCommand, string solutionFilePath, string buildConfiguration, string buildPlatform)
        {
            _builderCommand = builderCommand;
            _buildConfiguration = buildConfiguration;
            _buildPlatform = buildPlatform;
            _solutionFileFullPath = Path.GetFullPath(solutionFilePath);
            _workingDirectory = Path.GetDirectoryName(_solutionFileFullPath);
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
            string msbuildArguments = string.Format("{0} /t:{1}", _solutionFileFullPath, buildType.ToString());
            if (_buildConfiguration != null)
            {
                msbuildArguments += string.Format(" /property:Configuration={0}", _buildConfiguration);
            }
            if (_buildPlatform != null)
            {
                msbuildArguments += string.Format(" /property:Platform={0}", _buildPlatform);
            }
            msbuildArguments += " /maxcpucount";
            var runResult = CommandExecutor.Run(_workingDirectory, _builderCommand, msbuildArguments);

            return new BuildResult(
                runResult.outputs,
                runResult.errors,
                buildDuration: DateTime.Now - buildStartTime);
        }
    }
}
