using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CppIncludeChecker;

public class BuildResult
{
    public bool IsSuccess { get; private set; }
    public List<string> Outputs { get; private set; }
    public List<string> Errors { get; private set; }
    public TimeSpan BuildDuration { get; private set; }
    private string _buildSolutionConfiguration;

    public BuildResult(List<string> outputs, List<string> errors, TimeSpan buildDuration)
    {
        Outputs = outputs;
        Errors = errors;
        BuildDuration = buildDuration;
        IsSuccess = ParseBuildSuccessfulness(Outputs);
    }

    public string GetBuildSolutionConfiguration()
    {
        if (string.IsNullOrEmpty(_buildSolutionConfiguration) == false)
        {
            return _buildSolutionConfiguration;
        }
        _buildSolutionConfiguration = ParseBuildSolutionConfiguration(Outputs);
        return _buildSolutionConfiguration;
    }

    private bool ParseBuildSuccessfulness(List<string> output)
    {
        foreach (string line in output)
        {
            if (line.StartsWith("Build succeeded"))
            {
                return true;
            }
        }
        return false;
    }

    private string ParseBuildSolutionConfiguration(List<string> buildOutput)
    {
        foreach (string line in buildOutput)
        {
            var match = Regex.Match(line, @"Building solution configuration ""(.*)""");
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }
        return "UnknownBuildConfiguration";
    }

    public string GetBuildDurationString()
    {
        if (BuildDuration.TotalMinutes > 1)
        {
            return string.Format("{0:0.00} min", BuildDuration.TotalMinutes);
        }
        return string.Format("{0:0.00} sec", BuildDuration.TotalSeconds);
    }
}
