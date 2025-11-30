using System.IO;
using System.Collections.Generic;
using System.Text;
using System;

namespace CppIncludeChecker;

public class ProgressController
{
    public const string CURRENT_MARK = "current-->";

    public List<string> SourceFileNames { get; set; } = new List<string>();
    public string CurrentProgressingFilename { get; set; }

    public void SaveToFile(string outputFileName)
    {
        if (string.IsNullOrEmpty(outputFileName))
        {
            return;
        }
        StringBuilder stringBuilder = new StringBuilder();
        foreach (string filename in SourceFileNames)
        {
            if (filename == CurrentProgressingFilename)
            {
                stringBuilder.Append(CURRENT_MARK);
            }
            stringBuilder.AppendLine(filename);
        }
        File.WriteAllText(outputFileName, stringBuilder.ToString());
    }

    /// <summary>
    /// 제대로 로딩이 안되거나, current 를 찾을 수 없으면 false 를 반환한다
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns>파일로부터 로딩이 잘되어서 이어서 진행할 수 있으면 true, 새롭게 리스트를 갱신해야 하면 false</returns>
    public bool LoadFromFile(string fileName)
    {
        SourceFileNames.Clear();
        CurrentProgressingFilename = "";

        if (File.Exists(fileName) == false)
        {
            return false;
        }
        foreach (string line in File.ReadAllLines(fileName))
        {
            if (line.StartsWith(CURRENT_MARK))
            {
                string realLine = line.Substring(CURRENT_MARK.Length).Trim();
                CurrentProgressingFilename = realLine;
                SourceFileNames.Add(realLine);
                continue;
            }
            SourceFileNames.Add(line.Trim());
        }
        if (SourceFileNames.Count <= 0)
        {
            return false;
        }
        if (string.IsNullOrEmpty(CurrentProgressingFilename))
        {
            return false;
        }
        return true;
    }

    public void LoadFromList(List<string> sourceFilenames)
    {
        SourceFileNames = sourceFilenames;
        if (SourceFileNames.Count == 0) {
            throw new Exception("Empty source file list");
        }
        CurrentProgressingFilename = SourceFileNames[0];
    }

    public string Advance()
    {
        bool found = false;
        for (int i = 0; i < SourceFileNames.Count; i++)
        {
            // 지난번에 찾았으니 이번이 다음 파일이다
            if (found)
            {
                CurrentProgressingFilename = SourceFileNames[i];
                return CurrentProgressingFilename;
            }
            // 이번에 찾았으면 마킹
            if (SourceFileNames[i] == CurrentProgressingFilename)
            {
                found = true;
            }
        }
        // 진행해야할 파일은 없다
        CurrentProgressingFilename = "";
        return "";
    }

    public string AdvanceWithSave(string progressFileName)
    {
        string output = Advance();
        SaveToFile(progressFileName);
        return output;
    }
}
