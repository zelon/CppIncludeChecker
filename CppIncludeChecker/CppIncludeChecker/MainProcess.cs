using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        _logger.Log($" + Current --> {progressController.Current.FileName},{progressController.Current.IncludeLine}");
        _logger.LogSeperateLine();

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
        _logger.LogSeperateLine();
        if (progressController.Current == null)
        {
            _logger.Log("No current index");
            return;
        }
        FileNameAndIncludeLine current = progressController.Current;
        _logger.Log($"[{progressController.CurrentIndex + 1}/{progressController.FileNameAndIncludeLines.Count}] Checking {current.FileName},{current.IncludeLine}");

        NeedlessIncludeLines needlessIncludeLines = TryRemoveIncludeAndCollectChanges(current.FileName, current.IncludeLine);
        progressController.AdvanceWithSave(_config.ProgressFilePath);
        _logger.LogSeperateLine();
        if (needlessIncludeLines.IncludeLineInfos.Count == 0)
        {
            _logger.Log("There is no needless include. Nice project!!!!!!!!!!!");
            return;
        }

        needlessIncludeLines.Print(_logger);

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
    }

    private NeedlessIncludeLines TryRemoveIncludeAndCollectChanges(string fileName, string includeLine)
    {
        NeedlessIncludeLines needlessIncludeLines = new NeedlessIncludeLines();

        SortedSet<string> oneLineNeedlessIncludeLiness = new SortedSet<string>();
        using (var fileModifier = new FileModifier(fileName, _config.ApplyChangeEncoding))
        {
            fileModifier.RemoveAndWrite(includeLine);
            BuildResult testBuildResult = _builder.Build();
            if (testBuildResult.IsSuccess)
            {
                _logger.Log(string.Format(" + BUILD SUCCESS ({0} build time) ----> can be removed", testBuildResult.GetBuildDurationString()));
                needlessIncludeLines.Add(fileName, includeLine);
            }
            else
            {
                _logger.Log(string.Format(" + BUILD FAILED ({0} build time) ----> must be remained", testBuildResult.GetBuildDurationString()));
            }
        }
        return needlessIncludeLines;
    }

    private BuildResult Build()
    {
        _logger.Log($"Start of build {_builder.BuildPlatform}|{_builder.BuildConfiguration}... ");
        var lastRebuildResult = _builder.Build();
        _logger.Log(" + End of build. BuildDuration: " + lastRebuildResult.GetBuildDurationString());
        _logger.LogToFile("=== Build result ===", lastRebuildResult.Outputs);
        return lastRebuildResult;
    }
}
