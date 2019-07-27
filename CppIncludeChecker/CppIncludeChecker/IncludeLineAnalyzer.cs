using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CppIncludeChecker
{
    public class IncludeLineAnalyzer
    {
        public static List<string> Analyze(string fileContent)
        {
            List<string> changes = new List<string>();

            var matches = Regex.Matches(fileContent, @"(#include.*)");
            foreach (var match in matches)
            {
                changes.Add(match.ToString().Trim());
            }
            return changes;
        }
    }
}
