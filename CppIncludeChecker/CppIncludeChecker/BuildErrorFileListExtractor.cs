using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CppIncludeChecker;

public class BuildErrorFileListExtractor
{
    public static SortedSet<string> Extract(List<string> logs)
    {
        SortedSet<string> output = new SortedSet<string>();

        foreach (string line in logs)
        {
            var match = Regex.Match(line, @">(.*)\(.*\): error C");
            if (match.Success)
            {
                string filename = match.Groups[1].Value;
                output.Add(filename);
            }
        }
        return output;
    }
}
