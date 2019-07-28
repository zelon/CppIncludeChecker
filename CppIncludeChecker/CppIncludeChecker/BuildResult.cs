using System;
using System.Collections.Generic;

namespace CppIncludeChecker
{
    public class BuildResult
    {
        public bool IsSuccess { get; set; }
        public List<string> Outputs { get; set; }
        public List<string> Errors { get; set; }
        public TimeSpan BuildDuration { get; set; }

        public BuildResult(List<string> outputs, List<string> errors, TimeSpan buildDuration)
        {
            Outputs = outputs;
            Errors = errors;
            BuildDuration = buildDuration;
            IsSuccess = IsBuildSuccess(Outputs);
        }

        private bool IsBuildSuccess(List<string> output)
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

        public string GetBuildDurationString()
        {
            if (BuildDuration.TotalMinutes > 1)
            {
                return string.Format("{0:0.00} minutes", BuildDuration.TotalMinutes);
            }
            return string.Format("{0:0.00} seconds", BuildDuration.TotalSeconds);
        }
    }
}
