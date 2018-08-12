using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CppIncludeChecker
{
    public class ChangeMaker
    {
        public ChangeMaker()
        {

        }

        public List<string> AnalyzeIncludeLines(string fileContent)
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
