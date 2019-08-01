﻿using System.Collections.Generic;

namespace CppIncludeChecker
{
    class MainProcess
    {
        private Builder _builder;
		private Logger _logger;
        private readonly Config _config;

        public MainProcess(Config config, Logger logger, string builderCommand)
        {
            _config = config;
            _logger = logger;
            _builder = new Builder(builderCommand, _config.SolutionFilePath, _config.BuildConfiguration, _config.BuildPlatform);
        }

		public void Start()
        {
            _logger.LogSeperateLine();
            BuildResult startBuildResult = RebuildAtStart();
            if (startBuildResult.IsSuccess == false)
            {
                return;
            }
            _logger.LogSeperateLine();
            SortedSet<string> sourceFilenames = CompileFileListExtractor.GetFilenames(startBuildResult.Outputs);
            sourceFilenames = Util.FilterOut(sourceFilenames, _config.FilenameFilters);
            if (sourceFilenames.Count <= 0)
            {
				_logger.Log("Cannot extract any file");
                return;
            }
            NeedlessIncludeLines needlessIncludeLines = TryRemoveIncludeAndCollectChanges(sourceFilenames);
            _logger.LogSeperateLine();
            if (needlessIncludeLines.IncludeLineInfos.Count == 0)
            {
				_logger.Log("There is no needless include. Nice project!!!!!!!!!!!");
                return;
            }

            needlessIncludeLines.Print(_logger);

            _logger.LogSeperateLine();

            foreach (var info in needlessIncludeLines.IncludeLineInfos)
            {
                string filename = info.Filename;
                string includeLine = info.IncludeLine;

                if (string.IsNullOrEmpty(_config.ExecCmdPath) == false)
                {
                    _logger.Log(string.Format("Executing {0}:{1}", filename, includeLine));
                    string argument = string.Format(@"""{0}"" ""{1}""", filename, includeLine);
                    var runResult = CommandExecutor.Run(".", _config.ExecCmdPath, argument);
                    _logger.Log("----------------------------------------------------",
                        runResult.outputs, runResult.errors);
                }
                if (_config.ApplyChange)
                {
                    _logger.Log(string.Format("Applying {0}:{1}", filename, includeLine));
                    FileModifier fileModifier = new FileModifier(filename, _config.ApplyChangeEncoding);
                    fileModifier.RemoveAndWrite(includeLine);
                }
            }
            // Some changes can break the build. So rebuild again
            BuildResult lastBuildResult = RebuildAtLast();
            if (lastBuildResult.IsSuccess == false)
            {
                _logger.Log("Final rebuild is successful");
            }
            else
            {
                _logger.Log("Final rebuild is failed!!!!!!!!!!!!!!!!!!!!!!!");
            }
        }

        private BuildResult RebuildAtStart()
        {
			_logger.Log("Start of Initial Rebuild");
            BuildResult buildResult = _builder.Rebuild();
			_logger.Log("End of Initial Rebuild. BuildDuration: " + buildResult.GetBuildDurationString());
            if (buildResult.IsSuccess == false || buildResult.Errors.Count > 0)
            {
				_logger.Log("There are errors of StartRebuild", buildResult.Outputs, buildResult.Errors);
                return buildResult;
            }
            _logger.LogToFile("=== Initial Rebuild result ===", buildResult.Outputs);
            return buildResult;
        }

        private NeedlessIncludeLines TryRemoveIncludeAndCollectChanges(SortedSet<string> filenames)
        {
            NeedlessIncludeLines needlessIncludeLines = new NeedlessIncludeLines();
            int checkedFileCount = 0;
            foreach (string filename in filenames)
            {
                if (checkedFileCount > _config.MaxCheckFileCount)
                {
                    _logger.Log("Reached maxcheckfilecount,count: " + _config.MaxCheckFileCount);
                    break;
                }
                ++checkedFileCount;

                FileModifier fileModifier = new FileModifier(filename, _config.ApplyChangeEncoding);
                List<string> includeLines = IncludeLineAnalyzer.Analyze(fileModifier.OriginalContent);
                includeLines = Util.FilterOut(includeLines, _config.IncludeFilters);
                if (includeLines.Count <= 0)
                {
                    _logger.Log(filename + " has no include line");
                    continue;
                }
                _logger.Log("Checking Filename: " + filename);
                foreach (string includeLine in includeLines)
                {
                    if (_config.IgnoreSelfHeaderInclude && Util.IsSelfHeader(filename, includeLine))
                    {
                        _logger.Log(string.Format("  + skipping: {0} by IgnoreSelfHeader", includeLine));
                        continue;
                    }
                    _logger.LogWithoutEndline(string.Format("  + testing : {0} .... ", includeLine));
                    fileModifier.RemoveAndWrite(includeLine);
                    var testBuildResult = _builder.Build();
                    if (testBuildResult.IsSuccess)
                    {
                        _logger.Log(string.Format(" ({0} testing build time) ----> [[[[[[CAN BE REMOVED]]]]]]", testBuildResult.GetBuildDurationString()));
                        needlessIncludeLines.Add(filename, includeLine);
                    }
                    else
                    {
                        _logger.Log(string.Format(" ({0} testing build time) ----> Cannot be removed", testBuildResult.GetBuildDurationString()));
                    }
                    _logger.LogToFile(string.Format("=== {0}:{1} build result ===", filename, includeLine), testBuildResult.Outputs);
                    fileModifier.RevertAndWrite();
                    if (needlessIncludeLines.IncludeLineInfos.Count >= _config.MaxSuccessRemoveCount)
                    {
                        _logger.Log("Reached maxsuccessremovecount,count: " + _config.MaxSuccessRemoveCount);
                        return needlessIncludeLines;
                    }
                }
            }
            return needlessIncludeLines;
        }

        private BuildResult RebuildAtLast()
        {
			_logger.Log("Start of Final Rebuild");
            var lastRebuildResult = _builder.Rebuild();
			_logger.Log("End of Final Rebuild. BuildDuration: " + lastRebuildResult.GetBuildDurationString());
            _logger.LogToFile("=== Final Rebuild result ===", lastRebuildResult.Outputs);
            if (lastRebuildResult.IsSuccess)
            {
				_logger.Log("Final Rebuild is successful");
            }
            else
            {
				_logger.Log("Final Rebuild is failed");
            }
            return lastRebuildResult;
        }
    }
}
