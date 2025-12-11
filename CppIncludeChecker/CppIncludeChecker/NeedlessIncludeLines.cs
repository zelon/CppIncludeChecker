using System.Collections.Generic;

namespace CppIncludeChecker;

public class NeedlessIncludeLines
{
    public struct IncludeLineInfo
    {
        public string Filename { get; set; }
        public string IncludeLine { get; set; }
    }

    public List<IncludeLineInfo> IncludeLineInfos { get; private set; }

    public NeedlessIncludeLines()
    {
        IncludeLineInfos = new List<IncludeLineInfo>();
    }

    public void Add(string filename, string includeLine)
    {
        IncludeLineInfos.Add(new IncludeLineInfo
        {
            Filename = filename,
            IncludeLine = includeLine
        });
    }

    public void Print(Logger logger)
    {
        foreach (var info in IncludeLineInfos)
        {
            logger.Log($" + Unused include info: {info.Filename},{info.IncludeLine}");
        }
    }
}
