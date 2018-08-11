using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CppIncludeChecker;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestCompiledFilelist()
        {
            string log = @"
Build started 2018-08-10 ?? 12:15:47.
     1>Project ""E:\git\CppIncludeChecker\TestCppSolution\TestCppSolution.sln"" on node 1 (Rebuild target(s)).
     1> ValidateSolutionConfiguration:
         Building solution configuration ""Debug|x64"".
     1> Project ""E:\git\CppIncludeChecker\TestCppSolution\TestCppSolution.sln""(1) is building ""E:\git\CppIncludeChecker\TestCppSolution\StaticLib1\StaticLib1.vcxproj"" (2) on node 2 (Rebuild target(s)).
     2> _PrepareForClean:
         Deleting file ""x64\Debug\StaticLib1.tlog\StaticLib1.lastbuildstate"".
       InitializeBuildStatus:
         Creating ""x64\Debug\StaticLib1.tlog\unsuccessfulbuild"" because ""AlwaysCreate"" was specified.
       VcpkgTripletSelection:
            Using triplet ""x64-windows"" from ""E:\git\vcpkg\installed\x64-windows\""
       ClCompile:
C:\Program Files(x86)\Microsoft Visual Studio\2017\Professional\VC\Tools\MSVC\14.14.26428\bin\HostX86\x64\CL.exe / c / I""E:\git\vcpkg\installed\x64-windows\include"" / ZI / nologo / W3 / WX - / diagnostics:classic / sdl / Od / D _DEBUG / D _LIB / D _UNICODE / D UNICODE / Gm - / EHsc / RTC1 / MDd / GS / fp:precise / permissive - / Zc:wchar_t / Zc:forScope / Zc:inline / Yc""stdafx.h"" / Fp""x64\Debug\StaticLib1.pch"" / Fo""x64\Debug\\"" / Fd""x64\Debug\StaticLib1.pdb"" / Gd / TP / FC / errorReport:queue stdafx.cpp
     1> Project ""E:\git\CppIncludeChecker\TestCppSolution\TestCppSolution.sln""(1) is building ""E:\git\CppIncludeChecker\TestCppSolution\TestCppSolution\TestCppSolution.vcxproj"" (3) on node 1 (Rebuild target(s)).
     3> _PrepareForClean:
         Deleting file ""x64\Debug\TestCppSolution.tlog\TestCppSolution.lastbuildstate"".
     2> ClCompile:
         stdafx.cpp
       Lib:
         C:\Program Files(x86)\Microsoft Visual Studio\2017\Professional\VC\Tools\MSVC\14.14.26428\bin\HostX86\x64\Lib.exe / OUT:""E:\git\CppIncludeChecker\TestCppSolution\x64\Debug\StaticLib1.lib"" / NOLOGO / MACHINE:X64 x64\Debug\stdafx.obj
     3> InitializeBuildStatus:
         Touching ""x64\Debug\TestCppSolution.tlog\unsuccessfulbuild"".
       VcpkgTripletSelection:
         Using triplet ""x64-windows"" from ""E:\git\vcpkg\installed\x64-windows\""
     2> Lib:
         StaticLib1.vcxproj->E:\git\CppIncludeChecker\TestCppSolution\x64\Debug\StaticLib1.lib
     AppLocalFromInstalled:
         C:\WINDOWS\System32\WindowsPowerShell\v1.0\powershell.exe - ExecutionPolicy Bypass - noprofile - File ""E:\git\vcpkg\scripts\buildsystems\msbuild\applocal.ps1"" ""E:\git\CppIncludeChecker\TestCppSolution\x64\Debug\StaticLib1.lib"" ""E:\git\vcpkg\installed\x64-windows\debug\bin"" ""x64\Debug\StaticLib1.tlog\StaticLib1.write.1u.tlog"" ""x64\Debug\vcpkg.applocal.log""
     3> ClCompile:
         C:\Program Files(x86)\Microsoft Visual Studio\2017\Professional\VC\Tools\MSVC\14.14.26428\bin\HostX86\x64\CL.exe / c / I""E:\git\vcpkg\installed\x64-windows\include"" / ZI / nologo / W3 / WX - / diagnostics:classic / sdl / Od / D _DEBUG / D _CONSOLE / D _UNICODE / D UNICODE / Gm - / EHsc / RTC1 / MDd / GS / fp:precise / permissive - / Zc:wchar_t / Zc:forScope / Zc:inline / Yc""stdafx.h"" / Fp""x64\Debug\TestCppSolution.pch"" / Fo""x64\Debug\\"" / Fd""x64\Debug\vc141.pdb"" / Gd / TP / FC / errorReport:queue stdafx.cpp
         stdafx.cpp
         C:\Program Files(x86)\Microsoft Visual Studio\2017\Professional\VC\Tools\MSVC\14.14.26428\bin\HostX86\x64\CL.exe / c / I""E:\git\vcpkg\installed\x64-windows\include"" / ZI / nologo / W3 / WX - / diagnostics:classic / sdl / Od / D _DEBUG / D _CONSOLE / D _UNICODE / D UNICODE / Gm - / EHsc / RTC1 / MDd / GS / fp:precise / permissive - / Zc:wchar_t / Zc:forScope / Zc:inline / Yu""stdafx.h"" / Fp""x64\Debug\TestCppSolution.pch"" / Fo""x64\Debug\\"" / Fd""x64\Debug\vc141.pdb"" / Gd / TP / FC / errorReport:queue ..\Module2.cpp SubDirectory\Module1.cpp TestCppSolution.cpp
         Module2.cpp
     2> CopyFilesToOutputDirectory:
         Copying file from ""x64\Debug\StaticLib1.pdb"" to ""E:\git\CppIncludeChecker\TestCppSolution\x64\Debug\StaticLib1.pdb"".
       FinalizeBuildStatus:
         Deleting file ""x64\Debug\StaticLib1.tlog\unsuccessfulbuild"".
         Touching ""x64\Debug\StaticLib1.tlog\StaticLib1.lastbuildstate"".
     2> Done Building Project ""E:\git\CppIncludeChecker\TestCppSolution\StaticLib1\StaticLib1.vcxproj""(Rebuild target(s)).
     3> ClCompile:
         Module1.cpp
         TestCppSolution.cpp
         Generating Code...
       Link:
         C:\Program Files(x86)\Microsoft Visual Studio\2017\Professional\VC\Tools\MSVC\14.14.26428\bin\HostX86\x64\link.exe / ERRORREPORT:QUEUE / OUT:""E:\git\CppIncludeChecker\TestCppSolution\x64\Debug\TestCppSolution.exe"" / INCREMENTAL / NOLOGO / LIBPATH:""E:\git\vcpkg\installed\x64-windows\debug\lib"" / LIBPATH:""E:\git\vcpkg\installed\x64-windows\debug\lib\manual-link"" kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib ""E:\git\vcpkg\installed\x64-windows\debug\lib\*.lib"" / MANIFEST / MANIFESTUAC:""level='asInvoker' uiAccess='false'"" / manifest:embed / DEBUG:FASTLINK / PDB:""E:\git\CppIncludeChecker\TestCppSolution\x64\Debug\TestCppSolution.pdb"" / SUBSYSTEM:CONSOLE / TLBID:1 / DYNAMICBASE / NXCOMPAT / IMPLIB:""E:\git\CppIncludeChecker\TestCppSolution\x64\Debug\TestCppSolution.lib"" / MACHINE:X64 x64\Debug\Module2.obj
                         x64\Debug\stdafx.obj
                         x64\Debug\Module1.obj
                         x64\Debug\TestCppSolution.obj
         TestCppSolution.vcxproj->E:\git\CppIncludeChecker\TestCppSolution\x64\Debug\TestCppSolution.exe
     AppLocalFromInstalled:
         C:\WINDOWS\System32\WindowsPowerShell\v1.0\powershell.exe - ExecutionPolicy Bypass - noprofile - File ""E:\git\vcpkg\scripts\buildsystems\msbuild\applocal.ps1"" ""E:\git\CppIncludeChecker\TestCppSolution\x64\Debug\TestCppSolution.exe"" ""E:\git\vcpkg\installed\x64-windows\debug\bin"" ""x64\Debug\TestCppSolution.tlog\TestCppSolution.write.1u.tlog"" ""x64\Debug\vcpkg.applocal.log""
       FinalizeBuildStatus:
            Deleting file ""x64\Debug\TestCppSolution.tlog\unsuccessfulbuild"".
            Touching ""x64\Debug\TestCppSolution.tlog\TestCppSolution.lastbuildstate"".
     3> Done Building Project ""E:\git\CppIncludeChecker\TestCppSolution\TestCppSolution\TestCppSolution.vcxproj""(Rebuild target(s)).
     1> Done Building Project ""E:\git\CppIncludeChecker\TestCppSolution\TestCppSolution.sln""(Rebuild target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)
";

            List<string> outputs = new List<string>();
            foreach (string line in log.Split("\n"))
            {
                outputs.Add(line);
            }
            var fileList = new CompileFileListExtractor(outputs).GetFilenames();
            Assert.IsTrue(fileList.Exists((filename) => filename == @"E:\git\CppIncludeChecker\TestCppSolution\TestCppSolution\SubDirectory\Module1.cpp"));
            Assert.IsTrue(fileList.Exists((filename) => filename == @"E:\git\CppIncludeChecker\TestCppSolution\TestCppSolution\..\Module2.cpp"));
            Assert.IsTrue(fileList.Exists((filename) => filename == @"E:\git\CppIncludeChecker\TestCppSolution\TestCppSolution\SubDirectory\Module1.cpp"));
            Assert.IsTrue(fileList.Exists((filename) => filename == @"E:\git\CppIncludeChecker\TestCppSolution\TestCppSolution\TestCppSolution.cpp"));
        }

        [TestMethod]
        public void TestExtractInclude()
        {
            string fileContent = @"
#include ""a.h""
#include ""b.h""
#include ""MyClass.h""

int main() {
return 0;
}

";

            ChangeMaker changeMaker = new ChangeMaker();
            List<string> includes = changeMaker.Analyze(fileContent);
            Assert.IsTrue(includes.Exists((include) => include == @"#include ""a.h"""));
            Assert.IsTrue(includes.Exists((include) => include == @"#include ""b.h"""));
            Assert.IsTrue(includes.Exists((include) => include == @"#include ""MyClass.h"""));
        }
    }
}
