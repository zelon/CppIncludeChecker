using Microsoft.VisualStudio.TestTools.UnitTesting;
using CppIncludeChecker;
using System.IO;
using System.Collections.Generic;

namespace UnitTest;

[TestClass]
public class TestProgressController
{
    [TestMethod]
    public void TestBasic()
    {
        string tempFileName = Path.GetTempFileName();
        List<string> sourceFileNames = new List<string>()
        {
            "A.h",
            "A.cpp"
        };
        ProgressController progressController = new ProgressController();
        progressController.LoadFromList(sourceFileNames);
        progressController.SaveToFile(tempFileName);

        string fileContent = File.ReadAllText(tempFileName);
        Assert.AreEqual(@$"
{ProgressController.CURRENT_MARK}A.h
A.cpp
".Trim(), fileContent.Trim());

        List<string> copiedFileNames = new List<string>();
        copiedFileNames.AddRange(progressController.SourceFileNames);

        progressController.LoadFromFile(tempFileName);

        CollectionAssert.AreEqual(copiedFileNames, progressController.SourceFileNames);
        Assert.AreEqual("A.h", progressController.CurrentProgressingFilename);

        // 한번 진행하면 A.cpp 로 넘어간다
        Assert.AreEqual("A.cpp", progressController.Advance());
        Assert.AreEqual("A.cpp", progressController.CurrentProgressingFilename);

        // 한번 더 진행하면 가르키는 파일이 없다
        Assert.AreEqual("", progressController.Advance());
        Assert.AreEqual("", progressController.CurrentProgressingFilename);
    }
}
