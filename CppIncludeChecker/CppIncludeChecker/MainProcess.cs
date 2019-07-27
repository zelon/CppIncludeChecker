using System;
using System.Collections.Generic;

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
            _builder = new Builder(_config.SolutionFilePath, builderCommand);
        }

		public void Start()
        {
            _logger.LogSeperateLine();
            Builder.BuildResult startBuildResult = RebuildAtStart();
            if (startBuildResult.IsSuccess == false)
            {
                return;
            }
            _logger.LogSeperateLine();
            SortedSet<string> sourceFilenames = CompileFileListExtractor.GetFilenames(startBuildResult.outputs);
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

            // Some build can break Rebuild. So check rebuild again
            Builder.BuildResult lastBuildResult = RebuildAtLast();
            if (lastBuildResult.IsSuccess == false)
            {
                _logger.Log("Final rebuild is failed!!!!!!!!!!!!!!!!!!!!!!!");
                return;
            }
            _logger.LogSeperateLine();

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
                    FileModifier fileModifier = new FileModifier(filename);
                    fileModifier.RemoveAndWrite(includeLine);
                }
            }
        }

        private Builder.BuildResult RebuildAtStart()
        {
			_logger.Log("Start of Initial Rebuild");
            Builder.BuildResult buildResult = _builder.Rebuild();
			_logger.Log("End of Initial Rebuild. BuildDuration: " + buildResult.GetBuildDurationString());
            if (buildResult.IsSuccess == false || buildResult.errors.Count > 0)
            {
				_logger.Log("There are errors of StartRebuild", buildResult.outputs, buildResult.errors);
                return buildResult;
            }
            _logger.LogToFile("=== Initial Rebuild result ===", buildResult.outputs);
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
                FileModifier fileModifier = new FileModifier(filename);
                List<string> includeLines = IncludeLineAnalyzer.Analyze(fileModifier.OriginalContent);
                includeLines = Util.FilterOut(includeLines, _config.IncludeFilters);
                if (includeLines.Count <= 0)
                {
                    _logger.Log(filename + " has no include line");
                    continue;
                }
                _logger.Log("Checking Filename: " + filename);
                List<string> successfulRemovingTestOkIncludeLines = new List<string>();
                foreach (string includeLine in includeLines)
                {
                    _logger.LogWithoutEndline(string.Format("  + testing remove line: {0} .... ", includeLine));
                    fileModifier.RemoveAndWrite(includeLine);
                    var testBuildResult = _builder.Build();
                    if (testBuildResult.IsSuccess)
                    {
                        _logger.Log(string.Format(" ({0} testing build time) ----> [[[[[[CAN BE REMOVED]]]]]]", testBuildResult.GetBuildDurationString()));
                        successfulRemovingTestOkIncludeLines.Add(includeLine);
                        needlessIncludeLines.Add(filename, includeLine);
                    }
                    else
                    {
                        _logger.Log(string.Format(" ({0} testing build time) ----> Cannot be removed", testBuildResult.GetBuildDurationString()));
                    }
                    _logger.LogToFile(string.Format("=== {0}:{1} build result ===", filename, includeLine), testBuildResult.outputs);
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

        private Builder.BuildResult RebuildAtLast()
        {
			_logger.Log("Start of Final Rebuild");
            var lastRebuildResult = _builder.Rebuild();
			_logger.Log("End of Final Rebuild. BuildDuration: " + lastRebuildResult.GetBuildDurationString());
            _logger.LogToFile("=== Final Rebuild result ===", lastRebuildResult.outputs);
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
