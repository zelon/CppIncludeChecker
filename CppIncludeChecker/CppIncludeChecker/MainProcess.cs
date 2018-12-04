using System;
using System.Collections.Generic;

namespace CppIncludeChecker
{
    class MainProcess : IDisposable
    {
        private Builder _builder;
		private Logger _logger;
		private List<FileModifier> _appliedFileModifiers = new List<FileModifier>();
        private readonly List<string> _filenameFilters;
        private readonly List<string> _includeFilters;
        private readonly bool _applyChange;

        public MainProcess(string solutionFilePath, bool applyChange, List<string> filenameFilters, List<string> includeFilters)
        {
            _applyChange = applyChange;
            _filenameFilters = filenameFilters;
            _includeFilters = includeFilters;
            _builder = new Builder(solutionFilePath);
            _logger = new Logger("CppIncludeChecker.log");

			PrintSetting(solutionFilePath);
        }

        public void Dispose()
        {
            _logger.Dispose();
        }

		private void PrintSetting(string solutionFilePath)
		{
			_logger.Log("SolutionFile: " + solutionFilePath);
			_logger.Log("ApplyChange: " + _applyChange);
			foreach (string filter in _filenameFilters)
			{
				_logger.Log("Ignore file: " + filter);
			}
			foreach (string filter in _includeFilters)
			{
				_logger.Log("Ignore include: " + filter);
			}
		}

		public void Start()
        {
            Builder.BuildResult startBuildResult = RebuildAtStart();
            if (startBuildResult.IsSuccess == false)
            {
                return;
            }
            List<string> filenames = ExtractFilenameList(startBuildResult.outputs);
            filenames = FilterOutByFilename(filenames);
            if (filenames.Count <= 0)
            {
				_logger.Log("Cannot extract any file");
                return;
            }
            int changedCount = TryRemoveIncludeAndCollectChanges(filenames);
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

            if (_applyChange == false)
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

        private List<string> ExtractFilenameList(List<string> outputs)
        {
            CompileFileListExtractor compileFileListExtractor = new CompileFileListExtractor(outputs);
            return compileFileListExtractor.GetFilenames();
        }

        private List<string> FilterOutByFilename(List<string> filenames)
        {
            List<string> outFilenames = new List<string>();
            foreach (string filename in filenames)
            {
                bool isInFilter = false;
                foreach (string filterFilename in _filenameFilters)
                {
                    if (filename.Contains(filterFilename))
                    {
                        isInFilter = true;
                        break;
                    }
                }
                if (isInFilter == false)
                {
                    outFilenames.Add(filename);
                }
            }
            return outFilenames;
        }

        private int TryRemoveIncludeAndCollectChanges(List<string> filenames)
        {
            ChangeMaker changeMaker = new ChangeMaker();
            foreach (string filename in filenames)
            {
				_logger.Log("Checking " + filename);
                FileModifier fileModifier = new FileModifier(filename);
                List<string> changeCandidates = changeMaker.AnalyzeIncludeLines(fileModifier.OriginalContent);
                changeCandidates = FilterOutByInclude(changeCandidates);
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

        private List<string> FilterOutByInclude(List<string> includes)
        {
            List<string> outIncludeList = new List<string>();
            foreach (string include in includes)
            {
                bool isInFilter = false;
                foreach (string filter in _includeFilters)
                {
                    if (include.Contains(filter))
                    {
                        isInFilter = true;
                        break;
                    }
                }
                if (isInFilter == false)
                {
                    outIncludeList.Add(include);
                }
            }
            return outIncludeList;
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
