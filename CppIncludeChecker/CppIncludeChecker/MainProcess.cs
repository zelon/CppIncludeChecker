using System;
using System.Collections.Generic;

namespace CppIncludeChecker
{
    class MainProcess
    {
        private Builder _builder;
		private Logger _logger;
		private List<FileModifier> _appliedFileModifiers = new List<FileModifier>();
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
            int changedCount = TryRemoveIncludeAndCollectChanges(sourceFilenames);
            if (changedCount <= 0)
            {
				_logger.Log("There is no needless include. Good!!");
                return;
            }

            // Some build can break Rebuild. So check rebuild again
            Builder.BuildResult lastBuildResult = RebuildAtLast();
            if (lastBuildResult.IsSuccess)
            {
                PrintAppliedFileModifiers();
            }

            if (_config.ApplyChange == false)
            {
                RevertAll();
				_logger.Log("All test changes has been reverted");
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

        private int TryRemoveIncludeAndCollectChanges(List<string> filenames)
        {
            ChangeMaker changeMaker = new ChangeMaker();
            foreach (string filename in filenames)
            {
				_logger.Log("Checking " + filename);
                FileModifier fileModifier = new FileModifier(filename);
                List<string> changeCandidates = changeMaker.AnalyzeIncludeLines(fileModifier.OriginalContent);
                changeCandidates = Util.FilterOut(changeCandidates, _config.IncludeFilters);
                if (changeCandidates.Count <= 0)
                {
                    continue;
                }
                List<string> successfulChanges = new List<string>();
                foreach (var removeString in changeCandidates)
                {
                    fileModifier.RemoveAndWrite(removeString);
                    var testBuildResult = _builder.Build();
                    if (testBuildResult.IsSuccess)
                    {
                        successfulChanges.Add(removeString);
                    }
                    _logger.LogToFile(string.Format("=== {0}:{1} build result ===", filename, removeString), testBuildResult.outputs);
                    fileModifier.RevertAndWrite();
                }
                if (successfulChanges.Count > 0)
                {
                    foreach (string success in successfulChanges)
                    {
						_logger.Log(string.Format("MarkedInclude:{0}:{1}", filename, success));
                    }
                    fileModifier.RemoveAndWrite(successfulChanges);
                    _appliedFileModifiers.Add(fileModifier);
                }
            }
            return _appliedFileModifiers.Count;
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

        private void PrintAppliedFileModifiers()
        {
            foreach (FileModifier fileModifier in _appliedFileModifiers)
            {
                foreach (string removedString in fileModifier.RemovedStrings)
                {
					_logger.Log(string.Format("NeedlessInclude:{0}:{1}", fileModifier.Filename, removedString));
                }
            }
        }

        private void RevertAll()
        {
            foreach (var fileModifier in _appliedFileModifiers)
            {
                fileModifier.RevertAndWrite();
            }
        }
    }
}
