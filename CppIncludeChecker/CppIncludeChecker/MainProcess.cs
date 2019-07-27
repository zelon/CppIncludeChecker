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
            Builder.BuildResult startBuildResult = RebuildAtStart();
            if (startBuildResult.IsSuccess == false)
            {
                return;
            }
            List<string> sourceFilenames = CompileFileListExtractor.GetFilenames(startBuildResult.outputs);
            sourceFilenames = Util.FilterOut(sourceFilenames, _config.FilenameFilters);
            if (sourceFilenames.Count <= 0)
            {
				_logger.Log("Cannot extract any file");
                return;
            }
            NeedlessIncludeLines needlessIncludeLines = TryRemoveIncludeAndCollectChanges(sourceFilenames);
            _logger.LogSeperateLine();
            if (needlessIncludeLines.Count <= 0)
            {
				_logger.Log("There is no needless include. Good!!");
                return;
            }

            // Some build can break Rebuild. So check rebuild again
            Builder.BuildResult lastBuildResult = RebuildAtLast();
            if (lastBuildResult.IsSuccess)
            {
                needlessIncludeLines.Print(_logger);
            }

            _logger.LogSeperateLine();
            if (_config.ApplyChange)
            {
				_logger.Log("Starting apply changes");
            }
            else
            {
				_logger.Log("All test changes has been applied");
            }
        }

        private Builder.BuildResult RebuildAtStart()
        {
			_logger.Log("Start of StartRebuild");
            Builder.BuildResult buildResult = _builder.Rebuild();
			_logger.Log("End of StartRebuild");
            if (buildResult.IsSuccess == false || buildResult.errors.Count > 0)
            {
				_logger.Log("There are errors of StartRebuild", buildResult.outputs, buildResult.errors);
                return buildResult;
            }
            _logger.LogToFile("=== StartRebuild result ===", buildResult.outputs);
            return buildResult;
        }

        private NeedlessIncludeLines TryRemoveIncludeAndCollectChanges(List<string> filenames)
        {
            NeedlessIncludeLines needlessIncludeLines = new NeedlessIncludeLines();
            foreach (string filename in filenames)
            {
				_logger.Log("Checking Filename: " + filename);
                FileModifier fileModifier = new FileModifier(filename);
                List<string> includeLines = IncludeLineAnalyzer.Analyze(fileModifier.OriginalContent);
                includeLines = Util.FilterOut(includeLines, _config.IncludeFilters);
                if (includeLines.Count <= 0)
                {
                    _logger.Log("  + not found include line");
                    continue;
                }
                List<string> successfulRemovingTestOkIncludeLines = new List<string>();
                foreach (string includeLine in includeLines)
                {
                       _logger.Log(string.Format("  + testing remove line: {0}", includeLine));
                    fileModifier.RemoveAndWrite(includeLine);
                    var testBuildResult = _builder.Build();
                    if (testBuildResult.IsSuccess)
                    {
                        _logger.Log(string.Format("  + testing remove line: {0}: {1}", includeLine, "[SUCCESS]"));
                        successfulRemovingTestOkIncludeLines.Add(includeLine);
                        needlessIncludeLines.Add(filename, includeLine);
                    }
                    else
                    {
                        _logger.Log(string.Format("  + testing remove line: {0}: {1}", includeLine, "[FAILED]"));
                    }
                    _logger.LogToFile(string.Format("=== {0}:{1} build result ===", filename, includeLine), testBuildResult.outputs);
                    fileModifier.RevertAndWrite();
                }
            }
            return needlessIncludeLines;
        }

        private Builder.BuildResult RebuildAtLast()
        {
			_logger.Log("Start of LastRebuild");
            var lastRebuildResult = _builder.Rebuild();
			_logger.Log("End of LastRebuild");
            _logger.LogToFile("=== LastRebuild result ===", lastRebuildResult.outputs);
            if (lastRebuildResult.IsSuccess)
            {
				_logger.Log("LastRebuild is successful");
            }
            else
            {
				_logger.Log("LastRebuild is failed");
            }
            return lastRebuildResult;
        }
    }
}
