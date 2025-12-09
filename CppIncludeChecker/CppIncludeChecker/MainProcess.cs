using System.Collections.Generic;
using System.IO;

namespace CppIncludeChecker;

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

        var progressController = new ProgressController();

        if (string.IsNullOrEmpty(_config.ProgressFilePath))
        { // progress file 을 사용하지 않으면 실행 인자로부터 다시 읽는다
            progressController.LoadFromSetting(_config.FirstFileNamesToProcess, _config.IncludeFileExtensions, _config.IncludeFilters, _config.ExcludeFilters);
        }
        else
        { // progress file 을 사용하면 기존 목록을 복원시도하고, 복원에 실패하면 실행 인자로부터 다시 읽는다
            // 파일로부터 실패했거나, 리스트를 처음부터 갱신해야 하면 false
            if (progressController.LoadFromFile(_config.ProgressFilePath) == false)
            {
                progressController.LoadFromSetting(_config.FirstFileNamesToProcess, _config.IncludeFileExtensions, _config.IncludeFilters, _config.ExcludeFilters);
            }
        }

        if (progressController.FileNameAndIncludeLines.Count == 0)
        {
            _logger.Log("There is no files in include filters");
            return;
        }

        BuildResult initialRebuildResult = Build();
        if (initialRebuildResult.IsSuccess == false)
        {
            _logger.Log("Failed to initial rebuild");
            return;
        }
        if (StopMarker.StopRequested)
        {
            return;
        }
        _logger.Log("Build configuration: " + initialRebuildResult.GetBuildSolutionConfiguration());
        _logger.LogSeperateLine();
        _logger.Log("Collected source file count: " + progressController.FileNameAndIncludeLines.Count);
        _logger.LogSeperateLine();

        NeedlessIncludeLines needlessIncludeLines = TryRemoveIncludeAndCollectChanges(progressController);
        if (StopMarker.StopRequested)
        {
            return;
        }
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
        if (StopMarker.StopRequested)
        {
            return;
        }
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
        // Some changes can break the build. So build again
        BuildResult lastBuildResult = Build();
        if (lastBuildResult.IsSuccess)
        {
            _logger.Log("Final build is successful");
        }
        else
        {
            _logger.Log("Final build is failed!!!!!!!!!!!!!!!!!!!!!!!");
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

    private NeedlessIncludeLines TryRemoveIncludeAndCollectChanges(ProgressController progressController)
    {
        NeedlessIncludeLines needlessIncludeLines = new NeedlessIncludeLines();
        int checkedFileCount = 0;
        for (; progressController.Current != null; progressController.AdvanceWithSave(_config.ProgressFilePath))
        {
            ProgressController.FileNameAndIncludeLine filenameAndIncludeLine = progressController.Current;
            if (checkedFileCount > _config.MaxCheckFileCount)
            {
                _logger.Log("Reached maxcheckfilecount,count: " + _config.MaxCheckFileCount);
                break;
            }
            ++checkedFileCount;

            string checkingMsg = string.Format("[{0}/{1}]", checkedFileCount, progressController.FileNameAndIncludeLines.Count);
            if (_config.MaxCheckFileCount != null)
            {
                checkingMsg += string.Format("[max_limited {0}]", _config.MaxCheckFileCount);
            }
            _logger.Log(checkingMsg + $" Checking Filename: {filenameAndIncludeLine.FileName},{filenameAndIncludeLine.IncludeLine}");

            SortedSet<string> oneLineNeedlessIncludeLiness = new SortedSet<string>();
            // each line removing build test
            if (StopMarker.StopRequested)
            {
                return needlessIncludeLines;
            }
            if (_config.IgnoreSelfHeaderInclude && Util.IsSelfHeader(filenameAndIncludeLine.FileName, filenameAndIncludeLine.IncludeLine))
            {
                _logger.Log(string.Format("  + skipping: {0} by IgnoreSelfHeader", filenameAndIncludeLine.IncludeLine));
                continue;
            }
            _logger.LogWithoutEndline(string.Format("  + testing : {0} ...", filenameAndIncludeLine.IncludeLine));
            using (var fileModifier = new FileModifier(filenameAndIncludeLine.FileName, _config.ApplyChangeEncoding))
            {
                fileModifier.RemoveAndWrite(filenameAndIncludeLine.IncludeLine);
                BuildResult testBuildResult = _builder.Build();
                if (testBuildResult.IsSuccess)
                {
                    _logger.LogWithoutLogTime(string.Format(" ({0} build time) ----> removing candidate", testBuildResult.GetBuildDurationString()));
                    oneLineNeedlessIncludeLiness.Add(filenameAndIncludeLine.IncludeLine);
                }
                else
                {
                    _logger.LogWithoutLogTime(string.Format(" ({0} build time) ----> not removing candidate", testBuildResult.GetBuildDurationString()));
                }
            }
            if (oneLineNeedlessIncludeLiness.Count <= 0)
            {
                continue;
            }
            _logger.LogSeperateLine();
            _logger.Log($"| Checked Filename: {filenameAndIncludeLine.FileName},{filenameAndIncludeLine.IncludeLine}");
            foreach (string includeLine in oneLineNeedlessIncludeLiness)
            {
                _logger.Log("|   + found needless include: " + includeLine);
                needlessIncludeLines.Add(filenameAndIncludeLine.FileName, includeLine);
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
            if (StopMarker.StopRequested)
            {
                return null;
            }
            _logger.LogSeperateLine();
            _logger.Log("| Final integration test");
            List<FileModifier> fileModifiers = new List<FileModifier>();
            foreach (var filenameAndInclude in filenameAndIncludes)
            {
                string msg = string.Format("|  + {0}:{1}", filenameAndInclude.Key, string.Join(',', filenameAndInclude.Value));
                _logger.Log(msg);
                FileModifier fileModifier = new FileModifier(filenameAndInclude.Key, _config.ApplyChangeEncoding);
                fileModifier.RemoveAndWrite(filenameAndInclude.Value);
                fileModifiers.Add(fileModifier);
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
            else
            {
                _logger.Log("|  ----> Final Integration Test Build Failed");
            }
            SortedSet<string> buildErrorFilenames = BuildErrorFileListExtractor.Extract(buildResult.Outputs);
            foreach (var temp in filenameAndIncludes)
            {
                if (buildErrorFilenames.Count > 0 && filenameAndIncludes.ContainsKey(buildErrorFilenames.Min))
                {
                    string filename = buildErrorFilenames.Min;
                    _logger.Log(string.Format("|  ----> Remove {0} and retrying...", filename));
                    filenameAndIncludes.Remove(filename);
                    break;
                }
                else
                {
                    string filename = temp.Key;
                    _logger.Log(string.Format("|  ----> Remove {0} and retrying...", filename));
                    filenameAndIncludes.Remove(filename);
                    break;
                }
            }
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

    private BuildResult Build()
    {
        _logger.Log("Start of build");
        var lastRebuildResult = _builder.Rebuild();
        _logger.Log("End of build. BuildDuration: " + lastRebuildResult.GetBuildDurationString());
        _logger.LogToFile("=== Build result ===", lastRebuildResult.Outputs);
        return lastRebuildResult;
    }
}
