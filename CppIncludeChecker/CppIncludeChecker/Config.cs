using System;
using System.Collections.Generic;
using System.IO;

namespace CppIncludeChecker
{
    public class Config
    {
        public string SolutionFilePath { get; set; }
        public bool ApplyChange { get; set; }
        public bool IgnoreSelfHeaderInclude { get; set; }
        public List<string> FilenameFilters { get; set; }
        public List<string> IncludeFilters { get; set; }

        public Config()
        {
            ApplyChange = false;
            IgnoreSelfHeaderInclude = false;
            FilenameFilters = new List<string>();
            IncludeFilters = new List<string>();
        }

        public static Config Parse(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: {0} SolutionFilePath [--applychange] [--ignoreselfheaderinclude] [--filenamefilter:xxxx.xxx]* [--includefilter:xxxx.h]*", Environment.CommandLine);
                return null;
            }
            Config config = new Config
            {
                SolutionFilePath = args[0]
            };
            if (File.Exists(config.SolutionFilePath) == false)
            {
                Console.WriteLine("Cannot find the solution file:{0}", config.SolutionFilePath);
                return null;
            }
            foreach (string arg in args)
            {
                if (arg.StartsWith("--") == false)
                {
                    continue;
                }
                if (arg == "--applychange")
                {
                    config.ApplyChange = true;
                    continue;
                }
                string testString = "";

                testString = "--filenamefilter:";
                if (arg.StartsWith(testString))
                {
                    config.FilenameFilters.Add(arg.Substring(testString.Length));
                    continue;
                }
                testString = "--includefilter:";
                if (arg.StartsWith(testString))
                {
                    config.IncludeFilters.Add(arg.Substring(testString.Length));
                    continue;
                }
            }
            return config;
        }

        public void Print(Logger logger)
        {
            logger.Log("SolutionFile: " + SolutionFilePath);
            logger.Log("ApplyChange: " + ApplyChange);
            foreach (string filter in FilenameFilters)
            {
                logger.Log("Ignore file: " + filter);
            }
            foreach (string filter in IncludeFilters)
            {
                logger.Log("Ignore include: " + filter);
            }
        }
    }
}
