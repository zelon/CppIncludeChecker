using CppIncludeChecker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using static CppIncludeChecker.ProgressController;

namespace UnitTest;

[TestClass]
public class TestProgressController
{
    [TestMethod]
    public void TestBasic()
    {
        string progressFileName = Path.GetTempFileName();

        string sourceFileName1 = Path.GetTempFileName();
        File.WriteAllText(sourceFileName1, @"
#include ""test1.h""
#include ""test2.h""
");

        string sourceFileName2 = Path.GetTempFileName();
        File.WriteAllText(sourceFileName2, @"
#include ""testA.h""
#include ""testB.h""
");
        List<string> sourceFileNames = new List<string>()
        {
            sourceFileName1,
            sourceFileName2
        };

        ProgressController progressController = new ProgressController();
        progressController.LoadFromSetting(firstFileNamesToProcess: [], includeFileExtensions: ["h", "cpp"], includeFilters: sourceFileNames, [], [], skipSelfHeader: false);
        progressController.SaveToFile(progressFileName);

        string fileContent = File.ReadAllText(progressFileName);
        Assert.AreEqual(@$"
{ProgressController.CURRENT_MARK}{sourceFileName1},#include ""test1.h""
{sourceFileName1},#include ""test2.h""
{sourceFileName2},#include ""testA.h""
{sourceFileName2},#include ""testB.h""
".Trim(), fileContent.Trim());

        Assert.IsNotNull(progressController.Current);

        Assert.AreEqual(sourceFileName1, progressController.Current.FileName);
        Assert.AreEqual("#include \"test1.h\"", progressController.Current.IncludeLine);

        // 한번 진행
        progressController.Advance();
        {
            // 한번 진행하면 다음 include line 으로 넘어간다
            Assert.AreEqual(sourceFileName1, progressController.Current.FileName);
            Assert.AreEqual("#include \"test2.h\"", progressController.Current.IncludeLine);
        }

        // 한번 진행
        progressController.Advance();
        {
            // 한번 진행하면 다음 include line 으로 넘어간다
            Assert.AreEqual(sourceFileName2, progressController.Current.FileName);
            Assert.AreEqual("#include \"testA.h\"", progressController.Current.IncludeLine);
        }

        // 한번 진행
        progressController.Advance();
        {
            // 한번 진행하면 다음 include line 으로 넘어간다
            Assert.AreEqual(sourceFileName2, progressController.Current.FileName);
            Assert.AreEqual("#include \"testB.h\"", progressController.Current.IncludeLine);
        }

        // 한번 더 진행하면 가르키는 파일이 없다
        Assert.IsNull(progressController.Advance());
        Assert.IsNull(progressController.Current);
    }

    [TestMethod]
    public void TestSelfHeaderInclude()
    {
        string sourceFileName = Path.GetTempFileName();
        string selfHeaderFileName = Path.Combine(Path.GetFileNameWithoutExtension(sourceFileName) + ".h");
        File.WriteAllText(sourceFileName, @$"
#include ""{selfHeaderFileName}""
#include ""test1.h""
#include ""test2.h""
");

        List<string> sourceFileNames = new List<string>()
        {
            sourceFileName,
        };

        ProgressController progressController = new ProgressController();
        progressController.LoadFromSetting(firstFileNamesToProcess: [],
            includeFileExtensions: ["h", "cpp"],
            includeFilters: sourceFileNames, [],
            excludeLineFilters: [],
            skipSelfHeader: true);
        string progressFileName = Path.GetTempFileName();
        progressController.SaveToFile(progressFileName);

        string fileContent = File.ReadAllText(progressFileName);
        Assert.AreEqual(@$"
{ProgressController.CURRENT_MARK}{sourceFileName},#include ""test1.h""
{sourceFileName},#include ""test2.h""
".Trim(), fileContent.Trim());

        Assert.IsNotNull(progressController.Current);
    }

    [TestMethod]
    public void TestExcludeLine()
    {
        string sourceFileName = Path.GetTempFileName();
        string selfHeaderFileName = Path.Combine(Path.GetFileNameWithoutExtension(sourceFileName) + ".h");
        File.WriteAllText(sourceFileName, @$"
#include ""stdafx.h""
#include ""{selfHeaderFileName}""
#include ""test1.h""
#include ""test2.h""
");

        List<string> sourceFileNames = new List<string>()
        {
            sourceFileName,
        };

        ProgressController progressController = new ProgressController();
        progressController.LoadFromSetting(firstFileNamesToProcess: [],
            includeFileExtensions: ["h", "cpp"],
            includeFilters: sourceFileNames, [],
            excludeLineFilters: ["stdafx.h"],
            skipSelfHeader: true);
        string progressFileName = Path.GetTempFileName();
        progressController.SaveToFile(progressFileName);

        string fileContent = File.ReadAllText(progressFileName);
        Assert.AreEqual(@$"
{ProgressController.CURRENT_MARK}{sourceFileName},#include ""test1.h""
{sourceFileName},#include ""test2.h""
".Trim(), fileContent.Trim());

        Assert.IsNotNull(progressController.Current);
    }
}
