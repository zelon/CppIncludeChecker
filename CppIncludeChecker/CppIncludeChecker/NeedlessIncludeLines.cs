using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CppIncludeChecker
{
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

        public void ApplyAll(Logger logger, string execCmdPath)
        {

        }

        public void ExecAll(Logger logger, string execCmdPath)
        {
            Debug.Assert(string.IsNullOrEmpty(execCmdPath) == false);
            Debug.Assert(File.Exists(execCmdPath));

            foreach (var info in IncludeLineInfos)
            {
                string filename = info.Filename;
                string includeLine = info.IncludeLine;
                string argument = string.Format(@"""{0}"" ""{1}""", filename, includeLine);
                var runResult = CommandExecutor.Run(".", execCmdPath, argument);
                logger.Log("----------------------------------------------------",
                    runResult.outputs, runResult.errors);
            }
        }

        public bool IsEmpty()
        {
            return IncludeLineInfos.Count == 0;
        }

        public void Print(Logger logger)
        {
            foreach (var info in IncludeLineInfos)
            {
                logger.Log(string.Format("Found needless include line:{0}:{1}", info.Filename, info.IncludeLine));
            }
        }
    }
}
