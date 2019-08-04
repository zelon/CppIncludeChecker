using System.Collections.Generic;

namespace CppIncludeChecker
{
    class MainProcess
    {
        private readonly Builder _builder;
		private readonly Logger _logger;
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
            BuildResult initialRebuildResult = RebuildAtStart();
            if (initialRebuildResult.IsSuccess == false)
            {
                _logger.Log("Failed to initial rebuild");
                return;
            }
            _logger.Log("Build configuration: " + initialRebuildResult.GetBuildSolutionConfiguration());
            _logger.LogSeperateLine();
            SortedSet<string> sourceFilenames = CompileFileListExtractor.GetFilenames(initialRebuildResult.Outputs);
            sourceFilenames = Util.FilterOut(sourceFilenames, _config.FilenameFilters);
            if (sourceFilenames.Count <= 0)
            {
				_logger.Log("Cannot extract any file");
                return;
            }
            _logger.Log("Collected source file count: " + sourceFilenames.Count);
            _logger.LogSeperateLine();
            NeedlessIncludeLines needlessIncludeLines = TryRemoveIncludeAndCollectChanges(sourceFilenames);
            _logger.LogSeperateLine();
            if (needlessIncludeLines.IncludeLineInfos.Count == 0)
            {
				_logger.Log("There is no needless include. Nice project!!!!!!!!!!!");
                return;
            }

            needlessIncludeLines.Print(_logger);

            _logger.LogSeperateLine();
            _logger.LogSeperateLine();
            _logger.Log(" Start of final integrated build test");
            _logger.LogSeperateLine();
            _logger.LogSeperateLine();

            needlessIncludeLines = FinalIntegrationTest(needlessIncludeLines);

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
                    using (FileModifier fileModifier = new FileModifier(filename, _config.ApplyChangeEncoding))
                    {
                        fileModifier.RemoveAndWrite(includeLine);
                        fileModifier.SetApplyPermanently();
                    }
                }
            }
            // Some changes can break the rebuild. So rebuild again
            BuildResult lastBuildResult = RebuildAtLast();
            if (lastBuildResult.IsSuccess)
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

                string checkingMsg = string.Format("[{0}/{1}]", checkedFileCount, filenames.Count);
                if (_config.MaxCheckFileCount != null)
                {
                    checkingMsg += string.Format("[max_limited {0}]", _config.MaxCheckFileCount);
                }
                _logger.Log(checkingMsg + string.Format(" Checking Filename: {0}", filename));

                List<string> includeLines = null;
                using (FileModifier fileModifier = new FileModifier(filename, _config.ApplyChangeEncoding))
                {
                    includeLines = IncludeLineAnalyzer.Analyze(fileModifier.OriginalContent);
                }
                includeLines = Util.FilterOut(includeLines, _config.IncludeFilters);
                if (includeLines.Count <= 0)
                {
                    _logger.Log(string.Format("  + {0} has no include line", filename));
                    continue;
                }

                SortedSet<string> oneLineNeedlessIncludeLiness = new SortedSet<string>();
                // each line removing build test
                foreach (string includeLine in includeLines)
                {
                    if (_config.IgnoreSelfHeaderInclude && Util.IsSelfHeader(filename, includeLine))
                    {
                        _logger.Log(string.Format("  + skipping: {0} by IgnoreSelfHeader", includeLine));
                        continue;
                    }
                    _logger.LogWithoutEndline(string.Format("  + testing : {0} ...", includeLine));
                    using (var fileModifier = new FileModifier(filename, _config.ApplyChangeEncoding))
                    {
                        fileModifier.RemoveAndWrite(includeLine);
                        BuildResult testBuildResult = _builder.Build();
                        if (testBuildResult.IsSuccess)
                        {
                            _logger.LogWithoutLogTime(string.Format(" ({0} build time) ----> removing candidate", testBuildResult.GetBuildDurationString()));
                            oneLineNeedlessIncludeLiness.Add(includeLine);
                        }
                        else
                        {
                            _logger.LogWithoutLogTime(string.Format(" ({0} build time) ----> not removing candidate", testBuildResult.GetBuildDurationString()));
                        }
                    }
                }
                // integrate build test because removing two line together cause build error
                while (oneLineNeedlessIncludeLiness.Count > 0)
                {
                    using (FileModifier fileModifier = new FileModifier(filename, _config.ApplyChangeEncoding))
                    {
                        List<string> integratedIncludeLines = new List<string>();
                        foreach (string includeLine in oneLineNeedlessIncludeLiness)
                        {
                            integratedIncludeLines.Add(includeLine);
                        }
                        fileModifier.RemoveAndWrite(integratedIncludeLines);
                        _logger.LogWithoutEndline(string.Format("  + testing integrated : {0} ...", string.Join(',', integratedIncludeLines)));
                        BuildResult integrateTestBuildResult = _builder.Build();
                        if (integrateTestBuildResult.IsSuccess)
                        {
                            _logger.LogWithoutLogTime(string.Format(" ({0} build time) ----> BUILD SUCCESS", integrateTestBuildResult.GetBuildDurationString()));
                            break;
                        }
                        string removingIncludeLine = oneLineNeedlessIncludeLiness.Min;
                        _logger.LogWithoutLogTime(string.Format(" ({0} build time) ----> build failed,revert {1} and retry", integrateTestBuildResult.GetBuildDurationString(), removingIncludeLine));
                        oneLineNeedlessIncludeLiness.Remove(removingIncludeLine);
                    }
                }
                if (oneLineNeedlessIncludeLiness.Count <= 0)
                {
                    continue;
                }
                _logger.LogSeperateLine();
                _logger.Log(string.Format("| Checked Filename: {0}", filename));
                foreach (string includeLine in oneLineNeedlessIncludeLiness)
                {
                    _logger.Log("|   + found needless include: " + includeLine);
                    needlessIncludeLines.Add(filename, includeLine);
                }
                string foundCountMsg = "|   ----> Found needless count: " + needlessIncludeLines.IncludeLineInfos.Count;
                if (_config.MaxSuccessRemoveCount != null)
                {
                    foundCountMsg += string.Format("/{0}", _config.MaxSuccessRemoveCount);
                    if (needlessIncludeLines.IncludeLineInfos.Count >= _config.MaxSuccessRemoveCount)
                    {
                        _logger.Log("Reached maxsuccessremovecount,count: " + _config.MaxSuccessRemoveCount);
                        return needlessIncludeLines;
                    }
                }
                _logger.Log(foundCountMsg);
                _logger.LogSeperateLine();
            }
            return needlessIncludeLines;
        }

        private NeedlessIncludeLines FinalIntegrationTest(NeedlessIncludeLines needlessIncludeLines)
        {
            Dictionary<string, List<string>> filenameAndIncludes = new Dictionary<string, List<string>>();
            foreach (var info in needlessIncludeLines.IncludeLineInfos)
            {
                if (filenameAndIncludes.ContainsKey(info.Filename) == false)
                {
                    filenameAndIncludes.Add(info.Filename, new List<string>());
                }
                filenameAndIncludes[info.Filename].Add(info.IncludeLine);
            }
            while (filenameAndIncludes.Count > 0)
            {
                _logger.LogSeperateLine();
                _logger.Log("| Final integration test");
                List<FileModifier> fileModifiers = new List<FileModifier>();
                foreach (var filenameAndInclude in filenameAndIncludes)
                {
                    string msg = string.Format("|  + {0}:{1}", filenameAndInclude.Key, string.Join(',', filenameAndInclude.Value));
                    _logger.Log(msg);
                    FileModifier fileModifier = new FileModifier(filenameAndInclude.Key, _config.ApplyChangeEncoding);
                    fileModifier.RemoveAndWrite(filenameAndInclude.Value);
                }
                var buildResult = _builder.Build();
                _logger.Log("|  : Build Duration: " + buildResult.GetBuildDurationString());
                foreach (var fileModifier in fileModifiers)
                {
                    fileModifier.RevertAndWrite();
                }
                if (buildResult.IsSuccess)
                {
                    _logger.Log("|  ----> Final Integration Test Build Success");
                    break;
                }
                filenameAndIncludes.Remove(filenameAndIncludes.Keys.GetEnumerator().Current);
            }
            NeedlessIncludeLines output = new NeedlessIncludeLines();
            if (filenameAndIncludes.Count <= 0)
            {
                return output;
            }
            foreach (var result in filenameAndIncludes)
            {
                foreach (var line in result.Value)
                {
                    output.Add(result.Key, line);
                }
            }
            return output;
        }
        private BuildResult RebuildAtLast()
        {
			_logger.Log("Start of Final Rebuild");
            var lastRebuildResult = _builder.Rebuild();
			_logger.Log("End of Final Rebuild. BuildDuration: " + lastRebuildResult.GetBuildDurationString());
            _logger.LogToFile("=== Final Rebuild result ===", lastRebuildResult.Outputs);
            return lastRebuildResult;
        }
    }
}
