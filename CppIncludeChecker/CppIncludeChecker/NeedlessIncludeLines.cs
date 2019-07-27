using System;
using System.Collections.Generic;
using System.Text;

namespace CppIncludeChecker
{
    public class NeedlessIncludeLines
    {
        struct IncludeLineInfo
        {
            public string Filename { get; set; }
            public string IncludeLine { get; set; }
        }

        private List<IncludeLineInfo> includeLineInfos;

        public NeedlessIncludeLines()
        {
            includeLineInfos = new List<IncludeLineInfo>();
        }

        public void Add(string filename, string includeLine)
        {
            includeLineInfos.Add(new IncludeLineInfo
            {
                Filename = filename
            });
        }

        public int Count {
            get
            {
                return includeLineInfos.Count;
            }
        }

        public void Print(Logger logger)
        {
            foreach (var info in includeLineInfos)
            {
                logger.Log(string.Format("Found needless include line:{0}:{1}", info.Filename, info.IncludeLine));
            }
        }
    }
}
