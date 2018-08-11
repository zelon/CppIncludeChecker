using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CppIncludeChecker
{
    class Program
    {
        static void DualWriteLine(string text)
        {
            Console.WriteLine(text);
            Debug.WriteLine(text);
        }

        static void SuperWriteLine(string text)
        {
            DualWriteLine("################################################");
            DualWriteLine("# " + text);
            DualWriteLine("################################################");
        }

        static void Main(string[] args)
        {
            SuperWriteLine("Start of StartRebuild");
            string solutionFilePath = @"E:\git\CppIncludeChecker\TestCppSolution\TestCppSolution.sln";
            Builder builder = new Builder(solutionFilePath);
            var buildResult = builder.Rebuild();
            SuperWriteLine("End of StartRebuild");
            if (buildResult.IsSuccess == false || buildResult.errors.Count > 0)
            {
                DualWriteLine("There are errors");
                foreach (string line in buildResult.outputs)
                {
                    DualWriteLine(line);
                }
                foreach (string line in buildResult.errors)
                {
                    DualWriteLine(line);
                }
                return;
            }
            foreach (string line in buildResult.outputs)
            {
                DualWriteLine(line);
            }

            CompileFileListExtractor compileFileListExtractor = new CompileFileListExtractor(buildResult.outputs);
            var fileList = compileFileListExtractor.GetFilenames();
            if (fileList.Count <= 0)
            {
                SuperWriteLine("Cannot extract any file");
                return;
            }

            bool hasChangedFile = false;
            List<FileContent> appliedFileContents = new List<FileContent>();
            ChangeMaker changeMaker = new ChangeMaker();
            foreach (string filename in fileList)
            {
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
                    fileContent.RevertWrite();
                }
                if (successfulChanges.Count > 0)
                {
                    hasChangedFile = true;
                    foreach (string success in successfulChanges)
                    {
                        DualWriteLine(string.Format("CheckInclude=>Filename:{0},include:{1}", filename, success));
                    }
                    fileContent.RemoveAllAndWrite(successfulChanges);
                    appliedFileContents.Add(fileContent);
                }
            }
            if (hasChangedFile == false)
            {
                SuperWriteLine("There is no needless include. Good!!");
                return;
            }
            // Some build can break Rebuild. So check rebuild again
            var lastRebuildResult = builder.Rebuild();
            if (lastRebuildResult.IsSuccess)
            {
                SuperWriteLine("LastRebuild is successful");
            }
            else
            {
                SuperWriteLine("LastRebuild is failed");
            }

            // Revert all files
            foreach (var fileContent in appliedFileContents)
            {
                fileContent.RevertWrite();
            }
        }
    }
}
