﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CppIncludeChecker
{
    public class MsBuildBatchMaker
    {
        private static readonly string kExecutorBatchFileTemplate =
            "@call \"{0}" + Environment.NewLine +
            "@chcp 437" + Environment.NewLine + // terminal codepage
            "msbuild.exe %*" + Environment.NewLine;

        public static string MakeAndGetPath(string vsMSBuildCmdPathFromConfig)
        {
            string programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");

            if (vsMSBuildCmdPathFromConfig != null)
            {
                if (File.Exists(vsMSBuildCmdPathFromConfig) == false)
                {
                    throw new Exception(string.Format("Cannot find {0}", vsMSBuildCmdPathFromConfig));
                }
                return CreateExecutorBatchFile(vsMSBuildCmdPathFromConfig);
            }
            return null;
        }

        private static string CreateExecutorBatchFile(string vsbuildBatchFilePath)
        {
            string fileContent = string.Format(kExecutorBatchFileTemplate, vsbuildBatchFilePath);
            string filename = "msbuild.bat";
            using (var stream = File.CreateText(filename))
            {
                stream.WriteLine(fileContent);
            }
            return System.IO.Path.GetFullPath(filename);
        }
    }

}
