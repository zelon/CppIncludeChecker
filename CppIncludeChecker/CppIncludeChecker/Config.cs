using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

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
    public string ProgressFilePath { get; set; } = "";

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
        logger.Log($"ProgressFilePath: {ProgressFilePath}");
    }

    private static void LoadFromAppSettings(Config config, string configFilePath)
    {
        if (!File.Exists(configFilePath))
        {
            return; // 설정 파일이 없으면 스킵
        }

        string directory = Path.GetDirectoryName(configFilePath);
        string fileName = Path.GetFileName(configFilePath);

        if (string.IsNullOrEmpty(directory))
        {
            directory = AppContext.BaseDirectory;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(directory)
            .AddJsonFile(fileName, optional: true, reloadOnChange: false)
            .Build();

        var section = configuration.GetSection("CppIncludeChecker");

        config.SolutionFilePath = section["SolutionFilePath"];
        config.MsBuildCmdPath = section["MsBuildEnvPath"];
        config.BuildConfiguration = section["BuildConfiguration"];
        config.BuildPlatform = section["BuildPlatform"];
        config.CheckingDirectory = section["CheckingDirectory"];

        if (bool.TryParse(section["ApplyChange"], out bool applyChange))
        {
            config.ApplyChange = applyChange;
        }

        string encodingName = section["ApplyEncoding"];
        if (!string.IsNullOrEmpty(encodingName))
        {
            try
            {
                config.ApplyChangeEncoding = Encoding.GetEncoding(encodingName);
            }
            catch
            {
                // 인코딩 파싱 실패 시 무시
            }
        }

        config.ExecCmdPath = section["ExecCmdPath"];

        if (bool.TryParse(section["IgnoreSelfHeaderInclude"], out bool ignoreSelfHeader))
        {
            config.IgnoreSelfHeaderInclude = ignoreSelfHeader;
        }

        if (int.TryParse(section["MaxCheckFileCount"], out int maxCheckFileCount))
        {
            config.MaxCheckFileCount = maxCheckFileCount;
        }

        if (int.TryParse(section["MaxSuccessRemoveCount"], out int maxSuccessRemoveCount))
        {
            config.MaxSuccessRemoveCount = maxSuccessRemoveCount;
        }

        if (bool.TryParse(section["RandomSequence"], out bool randomSequence))
        {
            config.RandomSequenceTest = randomSequence;
        }

        config.ProgressFilePath = section["ProgressFilePath"];

        // 배열 처리
        var filenameFilters = section.GetSection("FilenameFilters").GetChildren();
        foreach (var filter in filenameFilters)
        {
            if (!string.IsNullOrEmpty(filter.Value))
            {
                config.FilenameFilters.Add(filter.Value);
            }
        }

        var includeFilters = section.GetSection("IncludeFilters").GetChildren();
        foreach (var filter in includeFilters)
        {
            if (!string.IsNullOrEmpty(filter.Value))
            {
                config.IncludeFilters.Add(filter.Value);
            }
        }
    }

    public static Config Parse(string[] args)
    {
        Config config = new Config();

        // 먼저 --config-file 인자가 있는지 확인
        string configFilePath = null;
        bool hasConfigFile = false;
        string testString = "--config-file:";
        foreach (string arg in args)
        {
            if (arg.StartsWith(testString))
            {
                configFilePath = arg.Substring(testString.Length);
                hasConfigFile = true;
                break;
            }
        }

        // config-file이 지정되지 않았으면 기본 appsettings.json 사용
        if (configFilePath == null)
        {
            configFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        }

        // 설정 파일에서 설정 로드
        LoadFromAppSettings(config, configFilePath);

        // 명령줄 인자 처리
        bool hasSolutionFileArg = false;
        if (args.Length >= 1 && !args[0].StartsWith("--"))
        {
            // 첫 번째 인자가 --로 시작하지 않으면 솔루션 파일 경로
            config.SolutionFilePath = args[0];
            hasSolutionFileArg = true;
        }

        // --config-file이 없고 솔루션 파일 경로도 없으면 에러
        if (!hasConfigFile && !hasSolutionFileArg)
        {
            if (string.IsNullOrEmpty(config.SolutionFilePath))
            {
                PrintUsage();
                return null;
            }
        }

        // 솔루션 파일 존재 확인
        if (!string.IsNullOrEmpty(config.SolutionFilePath) && File.Exists(config.SolutionFilePath) == false)
        {
            Console.WriteLine("Cannot find the solution file:{0}", config.SolutionFilePath);
            return null;
        }

        testString = "";
        foreach (string arg in args)
        {
            if (arg.StartsWith("--") == false)
            {
                continue;
            }
            // --config-file은 이미 처리했으므로 스킵
            testString = "--config-file:";
            if (arg.StartsWith(testString))
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
            testString = "--progressfilepath:";
            if (arg.StartsWith(testString))
            {
                config.ProgressFilePath = arg.Substring(testString.Length);
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
        Console.WriteLine("Usage:");
        Console.WriteLine(@"  CppIncludeChecker.exe SolutionFilePath --msbuildenvpath:""C:\Program Files(x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsMSBuildCmd.bat"" [options]");
        Console.WriteLine(@"  CppIncludeChecker.exe --config-file:appsettings.json [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine(@"  --config-file:<path>              Path to configuration file (e.g., appsettings.json)");
        Console.WriteLine(@"  --msbuildenvpath:<path>           Path to MSBuild environment batch file");
        Console.WriteLine(@"  --build_configuration:<config>    Build configuration (e.g., Debug, Release)");
        Console.WriteLine(@"  --build_platform:<platform>       Build platform (e.g., x64, Win32)");
        Console.WriteLine(@"  --checking_directory:<dir>        Directory to check");
        Console.WriteLine(@"  --applychange                     Apply changes to files");
        Console.WriteLine(@"  --apply_encoding:<encoding>       Encoding for file changes (e.g., utf-8)");
        Console.WriteLine(@"  --exec:<path>                     Execute batch file for each found needless include");
        Console.WriteLine(@"  --ignoreselfheaderinclude         Ignore self header includes");
        Console.WriteLine(@"  --maxcheckfilecount:<count>       Maximum number of files to check");
        Console.WriteLine(@"  --maxsucessremovecount:<count>    Maximum number of successful removals");
        Console.WriteLine(@"  --filenamefilter:<filter>         Filter for filenames (can be specified multiple times)");
        Console.WriteLine(@"  --includefilter:<filter>          Filter for includes (can be specified multiple times)");
        Console.WriteLine(@"  --random_sequence                 Check files in random order");
    }
}
