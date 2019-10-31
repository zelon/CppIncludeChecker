# CppIncludeChecker
Check needless includes of Visual Studio C++ project using MSBUILD

# Usage
 You should specify msbuildenvpath to use msbuild and use YourCmd.bat to automate the result.

`CppIncludeChecker.exe TestCppSolution.sln --msbuildenvpath:"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\Tools\VsMSBuildCmd.bat" --exec:"TestExecCmd.bat"`

