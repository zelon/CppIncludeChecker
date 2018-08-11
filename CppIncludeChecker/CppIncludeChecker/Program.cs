using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CppIncludeChecker
{
    class Program
    {
        static void Log(string text)
        {
            Console.WriteLine(text);
            Debug.WriteLine(text);
        }

        class ChangeInfo
        {
            public string Filename { get; set; }
            public string RemoveString { get; set; }
        }

        static void Main(string[] args)
        {
            var fileLogger = File.CreateText("CppIncludeChecker.log");
            Log("Start of StartRebuild");
            string solutionFilePath = @"E:\git\CppIncludeChecker\TestCppSolution\TestCppSolution.sln";
            Builder builder = new Builder(solutionFilePath);
            var buildResult = builder.Rebuild();
            Log("End of StartRebuild");
            if (buildResult.IsSuccess == false || buildResult.errors.Count > 0)
            {
                Log("There are errors");
                foreach (string line in buildResult.outputs)
                {
                    Log(line);
                }
                foreach (string line in buildResult.errors)
                {
                    Log(line);
                }
                return;
            }

            fileLogger.WriteLine("=== StartRebuild result ===");
            foreach (string line in buildResult.outputs)
            {
                fileLogger.WriteLine(line);
            }

            CompileFileListExtractor compileFileListExtractor = new CompileFileListExtractor(buildResult.outputs);
            var fileList = compileFileListExtractor.GetFilenames();
            if (fileList.Count <= 0)
            {
                Log("Cannot extract any file");
                return;
            }

            bool hasChangedFile = false;
            List<FileContent> appliedFileContents = new List<FileContent>();
            ChangeMaker changeMaker = new ChangeMaker();
            List<ChangeInfo> changeInfos = new List<ChangeInfo>();
            foreach (string filename in fileList)
            {
                Log("Checking " + filename);
                FileContent fileContent = new FileContent(filename);
                List<string> changeCandidates = changeMaker.Analyze(fileContent.OriginalContent);
                if (changeCandidates.Count <= 0)
                {
                    continue;
                }
                List<string> successfulChanges = new List<string>();
                foreach (var removeString in changeCandidates)
                {
                    fileContent.RemoveAndWrite(removeString);
                    var testBuildResult = builder.Build();
                    if (testBuildResult.IsSuccess)
                    {
                        successfulChanges.Add(removeString);
                    }
                    fileLogger.WriteLine("=== {0}:{1} build result ===", filename, removeString);
                    fileContent.RevertWrite();
                }
                if (successfulChanges.Count > 0)
                {
                    hasChangedFile = true;
                    foreach (string success in successfulChanges)
                    {
                        ChangeInfo changeInfo = new ChangeInfo() {
                            Filename = filename,
                            RemoveString = success
                        };
                        Log(string.Format("CheckedInclude:{0}:{1}", changeInfo.Filename, changeInfo.RemoveString));
                        changeInfos.Add(changeInfo);
                    }
                    fileContent.RemoveAllAndWrite(successfulChanges);
                    appliedFileContents.Add(fileContent);
                }
            }
            if (hasChangedFile == false)
            {
                Log("There is no needless include. Good!!");
                return;
            }
            // Some build can break Rebuild. So check rebuild again
            Log("Start of LastRebuild");
            var lastRebuildResult = builder.Rebuild();
            Log("End of LastRebuild");
            fileLogger.WriteLine("=== LastRebuild result ===");
            foreach (string line in lastRebuildResult.outputs)
            {
                fileLogger.WriteLine(line);
            }
            if (lastRebuildResult.IsSuccess)
            {
                Log("LastRebuild is successful");
                foreach (var changeInfo in changeInfos)
                {
                    Log(string.Format("CheckInclude:{0}:{1}", changeInfo.Filename, changeInfo.RemoveString));
                }
            }
            else
            {
                Log("LastRebuild is failed");
            }

            // Revert all files
            foreach (var fileContent in appliedFileContents)
            {
                fileContent.RevertWrite();
            }
        }
    }
}
