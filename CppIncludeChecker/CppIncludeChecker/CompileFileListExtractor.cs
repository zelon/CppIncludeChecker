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
        private List<string> _outputs;

        public CompileFileListExtractor(List<string> outputs)
        {
            _outputs = outputs;
        }

        public List<string> GetFilenames()
        {
            List<string> filenames = new List<string>();
            var logsByNode = CollectLogByNode(_outputs);
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
                        filenames.AddRange(MakeFullPathAndVerify(projectFileFullPath, filenamesFromProject));
                    }
                    if (string.IsNullOrEmpty(projectFileFullPath))
                    {
                        continue;
                    }
                    var filenamesFromLine = ExtractCompileFileFromLine(line);
                    filenames.AddRange(MakeFullPathAndVerify(projectFileFullPath, filenamesFromLine));
                }
            }
            return filenames;
        }

        private List<string> MakeFullPathAndVerify(string projectFileFullPath, List<string> filenames)
        {
            List<string> output = new List<string>();
            foreach (string filename in filenames)
            {
                string projectFileDirectoryPath = Path.GetDirectoryName(projectFileFullPath);
                string fullpath = Path.GetFullPath(Path.Combine(projectFileDirectoryPath, filename));
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

        private List<string> ExtractCompileFileFromLine (string line)
        {
            List<string> output = new List<string>();
            var match = Regex.Match(line, "errorReport:queue (.*)$");
            if (match.Success)
            {
                string files = match.Groups[1].Value.Trim();
                foreach (string file in files.Split(' '))
                {
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
            string currentKey = "default";
            foreach (string line in outputs)
            {
                Match buildingMatch = Regex.Match(line, @"is building ""(.*.vcxproj)"" \((\d+)\) on node");
                if (buildingMatch.Success)
                {
                    string projectPathNode = buildingMatch.Groups[2].Value;
                    AddNodeBuildLog(result, projectPathNode, line);
                }
                Match match = Regex.Match(line, @"^\s*(.)>");
                if (match.Success)
                {
                    currentKey = match.Groups[1].Value;
                }
                AddNodeBuildLog(result, currentKey, line);
            }
            return result;
        }
    }
}
