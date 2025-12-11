using static CppIncludeChecker.ProgressController;

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
            progressController.LoadFromSetting(_config.FirstFileNamesToProcess, _config.IncludeFileExtensions, _config.IncludeFilters, _config.ExcludeFilters, _config.ExcludeLineFilters, _config.IgnoreSelfHeaderInclude);
        }
        else
        { // progress file 을 사용하면 기존 목록을 복원시도하고, 복원에 실패하면 실행 인자로부터 다시 읽는다
            // 파일로부터 실패했거나, 리스트를 처음부터 갱신해야 하면 false
            if (progressController.LoadFromFile(_config.ProgressFilePath))
            {
                _logger.Log($"Continue previous progress from {_config.ProgressFilePath}");
            }
            else
            {
                _logger.Log($"Start from scrach without previous progress from {_config.ProgressFilePath}");
                progressController.LoadFromSetting(_config.FirstFileNamesToProcess, _config.IncludeFileExtensions, _config.IncludeFilters, _config.ExcludeFilters, _config.ExcludeLineFilters, _config.IgnoreSelfHeaderInclude);
            }
        }

        if (progressController.FileNameAndIncludeLines.Count == 0)
        {
            _logger.Log("There is no files in include filters");
            return;
        }
        _logger.Log($" + Collected file and include line count: {progressController.FileNameAndIncludeLines.Count}");
        _logger.LogSeperateLine();

        _logger.LogWithoutEndline($"Initial building {_builder.BuildPlatform}|{_builder.BuildConfiguration}... ");
        BuildResult initialRebuildResult = Build();
        _logger.LogWithoutLogTime(" build completed. BuildDuration: " + initialRebuildResult.GetBuildDurationString());
        if (initialRebuildResult.IsSuccess == false)
        {
            _logger.Log("Failed to initial build");
            return;
        }
        if (StopMarker.StopRequested)
        {
            return;
        }
        _logger.LogSeperateLine();
        if (progressController.Current == null)
        {
            _logger.Log("No current index");
            return;
        }
        NeedlessIncludeLines needlessIncludeLines = new();
        for (int executionCount = 1; executionCount <= _config.ExecutionCount; ++executionCount)
        {
            if (StopMarker.StopRequested)
            {
                return;
            }
            FileNameAndIncludeLine current = progressController.Current;
            if (current == null)
            {
                break;
            }
            string fileName = current.FileName;
            string includeLine = current.IncludeLine;
            _logger.LogWithoutEndline($"{executionCount}/{_config.ExecutionCount}:[{progressController.CurrentIndex + 1}/{progressController.FileNameAndIncludeLines.Count}] >> {fileName},{includeLine} ... ");
            bool hasUnusedIncludeLine = false;
            {
                try
                {
                    using var fileModifier = new FileModifier(fileName, _config.ApplyChangeEncoding);
                    fileModifier.RemoveAndWrite(includeLine);
                    BuildResult testBuildResult = _builder.Build();
                    if (testBuildResult.IsSuccess)
                    {
                        _logger.LogWithoutLogTime($"({testBuildResult.GetBuildDurationString()}) --> CAN BE REMOVED");
                        hasUnusedIncludeLine = true;
                        needlessIncludeLines.Add(fileName, includeLine);
                    }
                    else
                    {
                        _logger.LogWithoutLogTime($"({testBuildResult.GetBuildDurationString()}) --> must be remained");
                    }
                }
                catch (System.IO.FileNotFoundException)
                {
                    _logger.Log($" CANNOT FIND FILE!!!!!! file:{fileName}");
                }
            }
            progressController.AdvanceWithSave(_config.ProgressFilePath);

            if (hasUnusedIncludeLine)
            {
                if (string.IsNullOrEmpty(_config.ExecCmdPath) == false)
                {
                    var runResult = CommandExecutor.Run(".", _config.ExecCmdPath, string.Format(@"""{0}"" ""{1}""", fileName, includeLine));
                    _logger.Log("----------------------------------------------------",
                        runResult.outputs, runResult.errors);
                    _logger.Log("----------------------------------------------------");
                }
                if (_config.ApplyChange)
                {
                    _logger.Log(string.Format("Applying {0}:{1}", fileName, includeLine));
                    try
                    {
                        using (FileModifier fileModifier = new FileModifier(fileName, _config.ApplyChangeEncoding))
                        {
                            fileModifier.RemoveAndWrite(includeLine);
                            fileModifier.SetApplyPermanently();
                        }
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        _logger.Log($" CANNOT FIND FILE for writing file,file:{fileName}");
                    }
                }
            }
        } // end of execution count for loop

        _logger.LogSeperateLine();

        if (needlessIncludeLines.IncludeLineInfos.Count <= 0)
        {
            _logger.Log("There is no unused include line");
        }
        else
        {
            _logger.Log($"Detected unused include line count: {needlessIncludeLines.IncludeLineInfos.Count}");
            needlessIncludeLines.Print(_logger);
        }
        _logger.LogSeperateLine();
    }

    private BuildResult Build()
    {
        var lastRebuildResult = _builder.Build();
        _logger.LogToFile("=== Build result ===", lastRebuildResult.Outputs);
        return lastRebuildResult;
    }
}
