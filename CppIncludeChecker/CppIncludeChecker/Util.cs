using System.Collections.Generic;
using System.IO;

namespace CppIncludeChecker;

public class Util
{
    public static List<string> FilterOut(IEnumerable<string> sources, IEnumerable<string> by)
    {
        List<string> output = new List<string>();
        foreach (string include in sources)
        {
            bool isInFilter = false;
            foreach (string filter in by)
            {
                if (include.Contains(filter))
                {
                    isInFilter = true;
                    break;
                }
            }
            if (isInFilter == false)
            {
                output.Add(include);
            }
        }
        return output;
    }

    public static bool IsSelfHeader(string sourceFilename, string includeLine)
    {
        string sourceFilenameOnly = Path.GetFileNameWithoutExtension(sourceFilename).ToLower().Trim();

        includeLine = includeLine.Replace("#include", "");
        includeLine = includeLine.Replace("\"", "");
        string includeLineFilenameOnly = Path.GetFileNameWithoutExtension(includeLine).ToLower().Trim();


        return sourceFilenameOnly == includeLineFilenameOnly;
    }
}
