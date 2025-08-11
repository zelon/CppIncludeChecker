using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace CppIncludeChecker
{
    public class CompileFileListExtractor
    {
        public static List<string> GetFilenames(List<string> outputs)
        {
            SortedSet<string> filenames = new SortedSet<string>();
            var logsByNode = CollectLogByNode(outputs);
            foreach (List<string> lines in logsByNode.Values)
            {
                string projectFileFullPath = "";
                foreach (string line in lines)
                {
                    var match = Regex.Match(line, @"is building ""(.*.vcxproj)");
                    if (match.Success)
                    {
                        projectFileFullPath = match.Groups[1].Value;
                        Debug.Assert(File.Exists(projectFileFullPath));
                        string projectFileContent = File.ReadAllText(projectFileFullPath);

                        var filenamesFromProject = ExtractHeaderFilesFromProjectFileContent(projectFileContent);
                        filenames.UnionWith(MakeFullPathAndVerify(projectFileFullPath, filenamesFromProject));
                    }
                    if (string.IsNullOrEmpty(projectFileFullPath))
                    {
                        continue;
                    }
                    var filenamesFromLine = ExtractCompileFileFromLine(line);
                    filenames.UnionWith(MakeFullPathAndVerify(projectFileFullPath, filenamesFromLine));
                }
            }
            return new List<string>(filenames);
        }

        private static SortedSet<string> MakeFullPathAndVerify(string projectFileFullPath, List<string> filenames)
        {
            SortedSet<string> output = new SortedSet<string>();
            foreach (string filename in filenames)
            {
                string projectFileDirectoryPath = Path.GetDirectoryName(projectFileFullPath);
                string normalizedFilename = filename.Replace('\\', Path.DirectorySeparatorChar);
                string fullpath = Path.GetFullPath(Path.Combine(projectFileDirectoryPath, normalizedFilename));
                if (File.Exists(fullpath))
                {
                    output.Add(fullpath);
                }
                else
                {
                    string errorLog = "Cannot find file:" + fullpath;
                    Debug.WriteLine(errorLog);
                    Console.WriteLine(errorLog);
                }
            }
            return output;
        }

        private static List<string> ExtractCompileFileFromLine (string line)
        {
            List<string> output = new List<string>();
            var match = Regex.Match(line, @"CL\.exe.*errorReport:queue (.*)$");
            if (match.Success)
            {
                string files = match.Groups[1].Value.Trim();
                foreach (string file in files.Split(' '))
                {
                    if (file.StartsWith("/"))
                    {
                        continue;
                    }
                    output.Add(file);
                }
            }
            return output;
        }

        public static List<string> ExtractHeaderFilesFromProjectFileContent(string projectFileContent)
        {
            List<string> output = new List<string>();

            XmlDocument document = new XmlDocument();
            document.LoadXml(projectFileContent);

            XmlNodeList includeNodes = document.GetElementsByTagName("ClInclude");
            foreach (XmlNode node in includeNodes)
            {
               output.Add(node.Attributes["Include"].Value);
            }
            return output;
        }

        public static void AddNodeBuildLog(Dictionary<string, List<string>> result, string key, string log) {
            if (result.ContainsKey(key) == false)
            {
                result.Add(key, new List<string>());
            }
            result[key].Add(log);
        }

        public static Dictionary<string, List<string>> CollectLogByNode(List<string> outputs)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            result.Add("default", outputs);
            return result;
        }
    }
}
