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
            Assert.IsTrue(fileList.Exists((filename) => filename == @"E:\git\CppIncludeChecker\TestCppSolution\Module2.cpp"));
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
            List<string> includes = changeMaker.AnalyzeIncludeLines(fileContent);
            Assert.IsTrue(includes.Exists((include) => include == @"#include ""a.h"""));
            Assert.IsTrue(includes.Exists((include) => include == @"#include ""b.h"""));
            Assert.IsTrue(includes.Exists((include) => include == @"#include ""MyClass.h"""));
        }

        [TestMethod]
        public void TestRemoveInclude1()
        {
            string input = "#include <string>\n#include <map>";
            string result = FileModifier.RemoveIncludeLine(input, "#include <string>");
            Assert.IsTrue(result == "#include <map>");
        }

        [TestMethod]
        public void TestRemoveInclude2()
        {
            string input = "#include <string>\r\n#include <map>";
            string result = FileModifier.RemoveIncludeLine(input, "#include <string>");
            Assert.IsTrue(result == "#include <map>");
        }

        [TestMethod]
        public void TestRemoveInclude3()
        {
            string input = "#include <string>\n#include <test>\n#include <map>";
            string result = FileModifier.RemoveIncludeLine(input, "#include <string>");
            Assert.IsTrue(result == "#include <test>\n#include <map>");
        }

        [TestMethod]
        public void TestRemoveInclude4()
        {
            string input = "#include <before>\n#include <string>\n#include <test>\n#include <map>";
            string result = FileModifier.RemoveIncludeLine(input, "#include <string>");
            Assert.IsTrue(result == "#include <before>\n#include <test>\n#include <map>");
        }

        [TestMethod]
        public void TestExtractFromProjectfile()
        {
            string input = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project DefaultTargets=""Build"" ToolsVersion=""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ItemGroup Label=""ProjectConfigurations"">
    <ProjectConfiguration Include=""Debug|Win32"">
      <Configuration>Debug</Configuration>
      <Platform>Win32</Platform>
    </ProjectConfiguration>
    <ProjectConfiguration Include=""Release|Win32"">
      <Configuration>Release</Configuration>
      <Platform>Win32</Platform>
    </ProjectConfiguration>
    <ProjectConfiguration Include=""Debug|x64"">
      <Configuration>Debug</Configuration>
      <Platform>x64</Platform>
    </ProjectConfiguration>
    <ProjectConfiguration Include=""Release|x64"">
      <Configuration>Release</Configuration>
      <Platform>x64</Platform>
    </ProjectConfiguration>
  </ItemGroup>
  <PropertyGroup Label=""Globals"">
    <VCProjectVersion>15.0</VCProjectVersion>
    <ProjectGuid>{3F8A18B8-0F18-4D23-A227-4F2B274C7207}</ProjectGuid>
    <Keyword>Win32Proj</Keyword>
    <RootNamespace>TestCppSolution</RootNamespace>
    <WindowsTargetPlatformVersion>10.0.17134.0</WindowsTargetPlatformVersion>
  </PropertyGroup>
  <Import Project=""$(VCTargetsPath)\Microsoft.Cpp.Default.props"" />
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|Win32'"" Label=""Configuration"">
    <ConfigurationType>Application</ConfigurationType>
    <UseDebugLibraries>true</UseDebugLibraries>
    <PlatformToolset>v141</PlatformToolset>
    <CharacterSet>Unicode</CharacterSet>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release|Win32'"" Label=""Configuration"">
    <ConfigurationType>Application</ConfigurationType>
    <UseDebugLibraries>false</UseDebugLibraries>
    <PlatformToolset>v141</PlatformToolset>
    <WholeProgramOptimization>true</WholeProgramOptimization>
    <CharacterSet>Unicode</CharacterSet>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|x64'"" Label=""Configuration"">
    <ConfigurationType>Application</ConfigurationType>
    <UseDebugLibraries>true</UseDebugLibraries>
    <PlatformToolset>v141</PlatformToolset>
    <CharacterSet>Unicode</CharacterSet>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release|x64'"" Label=""Configuration"">
    <ConfigurationType>Application</ConfigurationType>
    <UseDebugLibraries>false</UseDebugLibraries>
    <PlatformToolset>v141</PlatformToolset>
    <WholeProgramOptimization>true</WholeProgramOptimization>
    <CharacterSet>Unicode</CharacterSet>
  </PropertyGroup>
  <Import Project=""$(VCTargetsPath)\Microsoft.Cpp.props"" />
  <ImportGroup Label=""ExtensionSettings"">
  </ImportGroup>
  <ImportGroup Label=""Shared"">
  </ImportGroup>
  <ImportGroup Label=""PropertySheets"" Condition=""'$(Configuration)|$(Platform)'=='Debug|Win32'"">
    <Import Project=""$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props"" Condition=""exists('$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props')"" Label=""LocalAppDataPlatform"" />
  </ImportGroup>
  <ImportGroup Label=""PropertySheets"" Condition=""'$(Configuration)|$(Platform)'=='Release|Win32'"">
    <Import Project=""$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props"" Condition=""exists('$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props')"" Label=""LocalAppDataPlatform"" />
  </ImportGroup>
  <ImportGroup Label=""PropertySheets"" Condition=""'$(Configuration)|$(Platform)'=='Debug|x64'"">
    <Import Project=""$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props"" Condition=""exists('$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props')"" Label=""LocalAppDataPlatform"" />
  </ImportGroup>
  <ImportGroup Label=""PropertySheets"" Condition=""'$(Configuration)|$(Platform)'=='Release|x64'"">
    <Import Project=""$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props"" Condition=""exists('$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props')"" Label=""LocalAppDataPlatform"" />
  </ImportGroup>
  <PropertyGroup Label=""UserMacros"" />
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|Win32'"">
    <LinkIncremental>true</LinkIncremental>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|x64'"">
    <LinkIncremental>true</LinkIncremental>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release|Win32'"">
    <LinkIncremental>false</LinkIncremental>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release|x64'"">
    <LinkIncremental>false</LinkIncremental>
  </PropertyGroup>
  <ItemDefinitionGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|Win32'"">
    <ClCompile>
      <PrecompiledHeader>Use</PrecompiledHeader>
      <WarningLevel>Level3</WarningLevel>
      <Optimization>Disabled</Optimization>
      <SDLCheck>true</SDLCheck>
      <PreprocessorDefinitions>WIN32;_DEBUG;_CONSOLE;%(PreprocessorDefinitions)</PreprocessorDefinitions>
      <ConformanceMode>true</ConformanceMode>
    </ClCompile>
    <Link>
      <SubSystem>Console</SubSystem>
      <GenerateDebugInformation>true</GenerateDebugInformation>
    </Link>
  </ItemDefinitionGroup>
  <ItemDefinitionGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|x64'"">
    <ClCompile>
      <PrecompiledHeader>Use</PrecompiledHeader>
      <WarningLevel>Level3</WarningLevel>
      <Optimization>Disabled</Optimization>
      <SDLCheck>true</SDLCheck>
      <PreprocessorDefinitions>_DEBUG;_CONSOLE;%(PreprocessorDefinitions)</PreprocessorDefinitions>
      <ConformanceMode>true</ConformanceMode>
    </ClCompile>
    <Link>
      <SubSystem>Console</SubSystem>
      <GenerateDebugInformation>true</GenerateDebugInformation>
    </Link>
  </ItemDefinitionGroup>
  <ItemDefinitionGroup Condition=""'$(Configuration)|$(Platform)'=='Release|Win32'"">
    <ClCompile>
      <PrecompiledHeader>Use</PrecompiledHeader>
      <WarningLevel>Level3</WarningLevel>
      <Optimization>MaxSpeed</Optimization>
      <FunctionLevelLinking>true</FunctionLevelLinking>
      <IntrinsicFunctions>true</IntrinsicFunctions>
      <SDLCheck>true</SDLCheck>
      <PreprocessorDefinitions>WIN32;NDEBUG;_CONSOLE;%(PreprocessorDefinitions)</PreprocessorDefinitions>
      <ConformanceMode>true</ConformanceMode>
    </ClCompile>
    <Link>
      <SubSystem>Console</SubSystem>
      <EnableCOMDATFolding>true</EnableCOMDATFolding>
      <OptimizeReferences>true</OptimizeReferences>
      <GenerateDebugInformation>true</GenerateDebugInformation>
    </Link>
  </ItemDefinitionGroup>
  <ItemDefinitionGroup Condition=""'$(Configuration)|$(Platform)'=='Release|x64'"">
    <ClCompile>
      <PrecompiledHeader>Use</PrecompiledHeader>
      <WarningLevel>Level3</WarningLevel>
      <Optimization>MaxSpeed</Optimization>
      <FunctionLevelLinking>true</FunctionLevelLinking>
      <IntrinsicFunctions>true</IntrinsicFunctions>
      <SDLCheck>true</SDLCheck>
      <PreprocessorDefinitions>NDEBUG;_CONSOLE;%(PreprocessorDefinitions)</PreprocessorDefinitions>
      <ConformanceMode>true</ConformanceMode>
    </ClCompile>
    <Link>
      <SubSystem>Console</SubSystem>
      <EnableCOMDATFolding>true</EnableCOMDATFolding>
      <OptimizeReferences>true</OptimizeReferences>
      <GenerateDebugInformation>true</GenerateDebugInformation>
    </Link>
  </ItemDefinitionGroup>
  <ItemGroup>
    <ClInclude Include=""..\Module2.h"" />
    <ClInclude Include=""stdafx.h"" />
    <ClInclude Include=""SubDirectory\Module1.h"" />
    <ClInclude Include=""targetver.h"" />
  </ItemGroup>
  <ItemGroup>
    <ClCompile Include=""..\Module2.cpp"" />
    <ClCompile Include=""stdafx.cpp"">
      <PrecompiledHeader Condition=""'$(Configuration)|$(Platform)'=='Debug|Win32'"">Create</PrecompiledHeader>
      <PrecompiledHeader Condition=""'$(Configuration)|$(Platform)'=='Debug|x64'"">Create</PrecompiledHeader>
      <PrecompiledHeader Condition=""'$(Configuration)|$(Platform)'=='Release|Win32'"">Create</PrecompiledHeader>
      <PrecompiledHeader Condition=""'$(Configuration)|$(Platform)'=='Release|x64'"">Create</PrecompiledHeader>
    </ClCompile>
    <ClCompile Include=""SubDirectory\Module1.cpp"" />
    <ClCompile Include=""TestCppSolution.cpp"" />
  </ItemGroup>
  <Import Project=""$(VCTargetsPath)\Microsoft.Cpp.targets"" />
  <ImportGroup Label=""ExtensionTargets"">
  </ImportGroup>
</Project>


";
            List<string> filenames = CompileFileListExtractor.ExtractHeaderFilesFromProjectFileContent(input);
            Assert.IsTrue(filenames.Contains(@"..\Module2.h"));
            Assert.IsTrue(filenames.Contains(@"stdafx.h"));
            Assert.IsTrue(filenames.Contains(@"SubDirectory\Module1.h"));
            Assert.IsTrue(filenames.Contains(@"targetver.h"));
        }
    }
}
