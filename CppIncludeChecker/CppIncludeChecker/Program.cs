﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CppIncludeChecker
{
    class Program : IDisposable
    {
        private Builder _builder;
        private TextWriter _fileLogger;
        private List<FileContent> _appliedFileContents = new List<FileContent>();

        public Program()
        {
            string solutionFilePath = @"E:\git\CppIncludeChecker\TestCppSolution\TestCppSolution.sln";
            _builder = new Builder(solutionFilePath);
            _fileLogger = File.CreateText("CppIncludeChecker.log");
        }

        public void Dispose()
        {
            _fileLogger.Close();
        }

        public void Check()
        {
            Builder.BuildResult startBuildResult = RebuildAtStart();
            if (startBuildResult.IsSuccess == false)
            {
                return;
            }
            List<string> filenames = ExtractFilenameList(startBuildResult.outputs);
            if (filenames.Count <= 0)
            {
                Log("Cannot extract any file");
                return;
            }
            int changedCount = TryRemoveIncludeAndCollectChanges(filenames);
            if (changedCount <= 0)
            {
                Log("There is no needless include. Good!!");
                return;
            }

            // Some build can break Rebuild. So check rebuild again
            Builder.BuildResult lastBuildResult = RebuildAtLast();
            if (lastBuildResult.IsSuccess)
            {
                PrintAppliedFileContents();
            }

            // Revert all changes to test builds
            RevertAll();
        }

        private Builder.BuildResult RebuildAtStart()
        {
            Log("Start of StartRebuild");
            Builder.BuildResult buildResult = _builder.Rebuild();
            Log("End of StartRebuild");
            if (buildResult.IsSuccess == false || buildResult.errors.Count > 0)
            {
                Log("There are errors of StartRebuild", buildResult.outputs, buildResult.errors);
                return buildResult;
            }
            LogToFile("=== StartRebuild result ===", buildResult.outputs);
            return buildResult;
        }

        private List<string> ExtractFilenameList(List<string> outputs)
        {
            CompileFileListExtractor compileFileListExtractor = new CompileFileListExtractor(outputs);
            return compileFileListExtractor.GetFilenames();
        }

        private int TryRemoveIncludeAndCollectChanges(List<string> filenames)
        {
            ChangeMaker changeMaker = new ChangeMaker();
            foreach (string filename in filenames)
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
                    var testBuildResult = _builder.Build();
                    if (testBuildResult.IsSuccess)
                    {
                        successfulChanges.Add(removeString);
                    }
                    LogToFile(string.Format("=== {0}:{1} build result ===", filename, removeString), testBuildResult.outputs);
                    fileContent.RevertAndWrite();
                }
                if (successfulChanges.Count > 0)
                {
                    foreach (string success in successfulChanges)
                    {
                        ChangeInfo changeInfo = new ChangeInfo() {
                            Filename = filename,
                            RemoveString = success
                        };
                        Log(string.Format("MarkedInclude:{0}:{1}", changeInfo.Filename, changeInfo.RemoveString));
                    }
                    fileContent.RemoveAndWrite(successfulChanges);
                    _appliedFileContents.Add(fileContent);
                }
            }
            return _appliedFileContents.Count;
        }

        private Builder.BuildResult RebuildAtLast()
        {
            Log("Start of LastRebuild");
            var lastRebuildResult = _builder.Rebuild();
            Log("End of LastRebuild");
            LogToFile("=== LastRebuild result ===", lastRebuildResult.outputs);
            if (lastRebuildResult.IsSuccess)
            {
                Log("LastRebuild is successful");
            }
            else
            {
                Log("LastRebuild is failed");
            }
            return lastRebuildResult;
        }

        private void PrintAppliedFileContents()
        {
            foreach (FileContent fileContent in _appliedFileContents)
            {
                foreach (string removedString in fileContent.RemovedStrings)
                {
                    Log(string.Format("NeedlessInclude:{0}:{1}", fileContent.Filename, removedString));
                }
            }
        }

        void Log(string text, List<string> outputs = null, List<string> errors = null)
        {
            Console.WriteLine(text);
            Debug.WriteLine(text);
            if (outputs != null)
            {
                foreach (string line in outputs)
                {
                    Console.WriteLine(line);
                    Debug.WriteLine(line);
                }
            }
            if (errors != null)
            {
                foreach (string line in errors)
                {
                    Console.WriteLine(line);
                    Debug.WriteLine(line);
                }
            }
        }

        void LogToFile(string text, List<string> outputs = null, List<string> errors = null)
        {
            _fileLogger.WriteLine(text);
            if (outputs != null)
            {
                foreach (string line in outputs)
                {
                    _fileLogger.WriteLine(line);
                }
            }
            if (errors != null)
            {
                foreach (string line in errors)
                {
                    _fileLogger.WriteLine(line);
                }
            }
        }

        class ChangeInfo
        {
            public string Filename { get; set; }
            public string RemoveString { get; set; }
        }

        private void RevertAll()
        {
            foreach (var fileContent in _appliedFileContents)
            {
                fileContent.RevertAndWrite();
            }
        }

        static void Main(string[] args)
        {
            using (Program program = new Program())
            {
                program.Check();
            }
        }
    }
}
