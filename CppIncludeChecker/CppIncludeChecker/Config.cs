using System;
using System.Collections.Generic;
using System.IO;

namespace CppIncludeChecker
{
    public class Config
    {
        public string MsBuildCmdPath { get; set; }
        public string SolutionFilePath { get; set; }
        public bool ApplyChange { get; set; }
        public string ExecCmdPath { get; set; }
        public bool IgnoreSelfHeaderInclude { get; set; }
        public int? MaxCheckFileCount { get; set; }
        public int? MaxSuccessRemoveCount { get; set; }
        public List<string> FilenameFilters { get; set; }
        public List<string> IncludeFilters { get; set; }

        public Config()
        {
            ApplyChange = false;
            IgnoreSelfHeaderInclude = false;
            FilenameFilters = new List<string>();
            IncludeFilters = new List<string>();
        }

        public void Print(Logger logger)
        {
            logger.Log("SolutionFile: " + SolutionFilePath);
            logger.Log("MsBuildCmdPath: " + MsBuildCmdPath);
            logger.Log("ApplyChange: " + ApplyChange);
            logger.Log("ExecCmdPath: " + ExecCmdPath);
            logger.Log("IgnoreSelfHeaderInclude: " + IgnoreSelfHeaderInclude);
            logger.Log("MaxCheckFileCount: " + MaxCheckFileCount);
            logger.Log("MaxSuccessRemoveCount: " + MaxSuccessRemoveCount);
            foreach (string filter in FilenameFilters)
            {
                logger.Log("IgnoreFileFilter: " + filter);
            }
            foreach (string filter in IncludeFilters)
            {
                logger.Log("IgnoreIncludeFilter: " + filter);
            }
        }

        public static Config Parse(string[] args)
        {
            if (args.Length < 1)
            {
                PrintUsage();
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
                string testString = "";

                testString = "--msbuildenvpath:";
                if (arg.StartsWith(testString))
                {
                    config.MsBuildCmdPath = arg.Substring(testString.Length);
                    continue;
                }
                if (arg == "--applychange")
                {
                    config.ApplyChange = true;
                    continue;
                }
                testString = "--exec:";
                if (arg.StartsWith(testString))
                {
                    config.ExecCmdPath = arg.Substring(testString.Length);
                    continue;
                }
                if (arg == "--ignoreselfheaderinclude")
                {
                    config.IgnoreSelfHeaderInclude = true;
                    continue;
                }
                testString = "--maxcheckfilecount:";
                if (arg.StartsWith(testString))
                {
                    config.MaxCheckFileCount = int.Parse(arg.Substring(testString.Length));
                    continue;
                }
                testString = "--maxsucessremovecount:";
                if (arg.StartsWith(testString))
                {
                    config.MaxSuccessRemoveCount = int.Parse(arg.Substring(testString.Length));
                    continue;
                }
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
            if (config.MsBuildCmdPath == null)
            {
                Console.WriteLine("Cannot find msbuild path. Check --msbuildenvpath");
                PrintUsage();
                return null;
            }
            if (string.IsNullOrEmpty(config.ExecCmdPath) == false)
            {
                if (File.Exists(config.ExecCmdPath) == false)
                {
                    Console.WriteLine("Cannot find exec: " + config.ExecCmdPath);
                    return null;
                }
            }
            return config;
        }

        public static void PrintUsage()
        {
            Console.WriteLine(@"Usage: {0} SolutionFilePath --msbuildenvpath:""C:\Program Files(x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsMSBuildCmd.bat"" [--applychange] [--exec:""C:\Test\make_patch.bat""] [--ignoreselfheaderinclude] [--filenamefilter:xxxx.xxx]* [--includefilter:xxxx.h]*", Environment.CommandLine);
        }
    }
}
