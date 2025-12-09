using System.IO;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

namespace CppIncludeChecker;

public class ProgressController
{
    public class FileNameAndIncludeLine
    {
        public string FileName { get; set; }
        public string IncludeLine { get; set; }
    }

    public const string CURRENT_MARK = "current-->";

    public List<FileNameAndIncludeLine> FileNameAndIncludeLines { get; set; } = [];
    public FileNameAndIncludeLine Current { get; set; }

    public void SaveToFile(string outputFileName)
    {
        if (string.IsNullOrEmpty(outputFileName))
        {
            return;
        }
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var fileNameAndIncludeLine in FileNameAndIncludeLines)
        {
            if (fileNameAndIncludeLine == Current)
            {
                stringBuilder.Append(CURRENT_MARK);
            }
            stringBuilder.AppendLine($"{fileNameAndIncludeLine.FileName},{fileNameAndIncludeLine.IncludeLine}");
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
        FileNameAndIncludeLines.Clear();
        Current = null;

        if (File.Exists(fileName) == false)
        {
            return false;
        }
        foreach (string lineInFile in File.ReadAllLines(fileName))
        {
            string line = lineInFile.Trim();
            bool isCurrent = false;
            if (line.StartsWith(CURRENT_MARK))
            {
                isCurrent = true;
                line = line.Substring(CURRENT_MARK.Length);
            }
            List<string> splitted = line.Split(",").ToList();
            if (splitted.Count != 2)
            {
                continue;
            }
            FileNameAndIncludeLine fileNameAndIncludeLine = new()
            {
                FileName = splitted[0],
                IncludeLine = splitted[1]
            };
            FileNameAndIncludeLines.Add(fileNameAndIncludeLine);
            if (isCurrent)
            {
                Current = fileNameAndIncludeLine;
            }
        }
        if (FileNameAndIncludeLines.Count <= 0)
        {
            return false;
        }
        if (Current == null)
        {
            return false;
        }
        return true;
    }

    public void LoadFromSetting(List<string> firstFileNamesToProcess, List<string> includeFileExtensions, List<string> includeFilters, List<string> excludeFilters)
    {
        List<string> fileListFromIncludeFilters = [];

        // 헤더 파일에서 먼저 include 를 제거하는게 좋기 때문에 filelist 에 넣는 순서를 중요하게 사용한다
        // 그래서 include file extensions 에 적힌 확장자 순서대로 filelist 에 넣는다
        List<string> searchPatterns = [];
        foreach (string firstFileNameToProcess in firstFileNamesToProcess)
        {
            searchPatterns.Add(firstFileNameToProcess); // stdafx.h 같은 파일을 먼저 찾고
        }
        foreach (string includeFileExtension in includeFileExtensions)
        {
            searchPatterns.Add($"*.{includeFileExtension}"); // 확장자 순서대로 찾는다
        }

        // 순서를 쉽게 맞추려면 중복파일을 넣을 수 있기 때문에 중복 체크를 위한 목록을 관리한다
        HashSet<string> insertedFileName = [];
        // 실제 디렉토리들에서 파일 찾기
        foreach (string searchPattern in searchPatterns)
        {
            foreach (string includeFilter in includeFilters)
            {
                try
                {
                    if (File.Exists(includeFilter))
                    {// 파일일 때
                        if (insertedFileName.Contains(includeFilter) == false)
                        {
                            insertedFileName.Add(includeFilter);
                            fileListFromIncludeFilters.Add(includeFilter);
                        }
                    }
                    else
                    {// 디렉토리일 때
                        foreach (string fileName in Directory.GetFiles(includeFilter, searchPattern, SearchOption.AllDirectories))
                        {
                            // exclude filter 적용
                            bool isMatchedExcludeFilter = false;
                            foreach (string excludeFilter in excludeFilters)
                            {
                                if (fileName.Contains(excludeFilter))
                                {
                                    isMatchedExcludeFilter = true;
                                    break;
                                }
                            }
                            if (isMatchedExcludeFilter)
                            {
                                continue;
                            }
                            if (insertedFileName.Contains(fileName) == false)
                            {
                                insertedFileName.Add(fileName);
                                fileListFromIncludeFilters.Add(fileName);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine($"Cannot get files from {includeFilter},exception:{exception.Message}");
                }
            }
        }
        // 모아진 파일로부터 (파일이름,include line) 형태 목록을 만든다
        foreach (string filename in fileListFromIncludeFilters)
        {
            var includeLines = IncludeLineAnalyzer.Analyze(File.ReadAllText(filename));
            includeLines = Util.FilterOut(includeLines, includeFilters);
            foreach (string includeLine in includeLines)
            {
                FileNameAndIncludeLines.Add(new FileNameAndIncludeLine()
                {
                    FileName = filename,
                    IncludeLine = includeLine
                });
            }
        }
        if (FileNameAndIncludeLines.Count == 0)
        {
            throw new Exception("Empty source file list");
        }
        Current = FileNameAndIncludeLines[0];
    }


    public FileNameAndIncludeLine Advance()
    {
        bool found = false;
        for (int i = 0; i < FileNameAndIncludeLines.Count; i++)
        {
            // 지난번에 찾았으니 이번이 다음 파일이다
            if (found)
            {
                Current = FileNameAndIncludeLines[i];
                return Current;
            }
            // 이번에 찾았으면 마킹
            if (FileNameAndIncludeLines[i] == Current)
            {
                found = true;
            }
        }
        // 진행해야할 파일은 없다
        Current = null;
        return Current;
    }

    public FileNameAndIncludeLine AdvanceWithSave(string progressFileName)
    {
        FileNameAndIncludeLine output = Advance();
        SaveToFile(progressFileName);
        return output;
    }
}
