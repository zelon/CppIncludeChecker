using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CppIncludeChecker;

public class Config
{
    public string MsBuildCmdPath { get; set; }
    public string SolutionFilePath { get; set; }
    public string BuildConfiguration { get; set; }
    public string BuildPlatform { get; set; }
    public string CheckingDirectory { get; set; }
    public bool ApplyChange { get; set; }
    public Encoding ApplyChangeEncoding { get; set; }
    public string ExecCmdPath { get; set; }
    public bool IgnoreSelfHeaderInclude { get; set; }
    public int? MaxCheckFileCount { get; set; }
    public int? MaxSuccessRemoveCount { get; set; }
    public List<string> FilenameFilters { get; set; }
    public List<string> IncludeFilters { get; set; }
    public bool RandomSequenceTest { get; set; }
    
    public Config()
    {
        ApplyChange = false;
        IgnoreSelfHeaderInclude = false;
        FilenameFilters = new List<string>();
        IncludeFilters = new List<string>();
        RandomSequenceTest = false;
    }

    public void Print(Logger logger)
    {
        logger.Log("SolutionFile: " + SolutionFilePath);
        if (BuildConfiguration != null)
        {
            logger.Log("BuildConfiguration: " + BuildConfiguration);
        }
        if (BuildPlatform != null)
        {
            logger.Log("BuildPlatform: " + BuildPlatform);
        }
        if (string.IsNullOrEmpty(CheckingDirectory) == false)
        {
            logger.Log("CheckingDirectory: " + CheckingDirectory);
        }
        logger.Log("MsBuildCmdPath: " + MsBuildCmdPath);
        logger.Log("ApplyChange: " + ApplyChange);
        if (ApplyChangeEncoding != null)
        {
            logger.Log("ApplyChangeEncoding: " + ApplyChangeEncoding.ToString());
        }
        logger.Log("ExecCmdPath: " + ExecCmdPath);
        logger.Log("IgnoreSelfHeaderInclude: " + IgnoreSelfHeaderInclude);
        if (MaxCheckFileCount != null)
        {
            logger.Log("MaxCheckFileCount: " + MaxCheckFileCount);
        }
        if (MaxSuccessRemoveCount != null)
        {
            logger.Log("MaxSuccessRemoveCount: " + MaxSuccessRemoveCount);
        }
        foreach (string filter in FilenameFilters)
        {
            logger.Log("IgnoreFileFilter: " + filter);
        }
        foreach (string filter in IncludeFilters)
        {
            logger.Log("IgnoreIncludeFilter: " + filter);
        }
        if (RandomSequenceTest)
        {
            logger.Log("RandomSequenceTest: True");
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
        string testString = "";
        foreach (string arg in args)
        {
            if (arg.StartsWith("--") == false)
            {
                continue;
            }
            testString = "--build_configuration:";
            if (arg.StartsWith(testString))
            {
                config.BuildConfiguration = arg.Substring(testString.Length);
                continue;
            }
            testString = "--build_platform:";
            if (arg.StartsWith(testString))
            {
                config.BuildPlatform = arg.Substring(testString.Length);
                continue;
            }
            testString = "--checking_directory:";
            if (arg.StartsWith(testString))
            {
                config.CheckingDirectory = arg.Substring(testString.Length);
                continue;
            }
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
            testString = "--apply_encoding:";
            if (arg.StartsWith(testString))
            {
                
                config.ApplyChangeEncoding = Encoding.GetEncoding(arg.Substring(testString.Length));
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
            if (arg == "--random_sequence")
            {
                config.RandomSequenceTest = true;
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
        Console.WriteLine(@"Usage: CppIncludeChecker.exe SolutionFilePath --msbuildenvpath:""C:\Program Files(x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsMSBuildCmd.bat"" [--build_configuration:Debug] [--build_platform:x64] [--checking_directory:some/where/dir] [--applychange] [--apply_encoding:utf-8] [--exec:""C:\Test\make_patch.bat""] [--ignoreselfheaderinclude] [--filenamefilter:xxxx.xxx]* [--includefilter:xxxx.h]*");
    }
}
