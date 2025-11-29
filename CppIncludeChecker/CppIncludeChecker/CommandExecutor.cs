using System.Collections.Generic;
using System.Diagnostics;

namespace CppIncludeChecker;

class CommandExecutor
{
    public class Result
    {
        public List<string> outputs = new List<string>();
        public List<string> errors = new List<string>();
    }

    public static Result Run(string workingDirectory, string executeFilename, string arguments)
    {
        Process process = new Process();
        process.StartInfo.FileName = executeFilename;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.EnvironmentVariables["DOTNET_CLI_UI_LANGUAGE"] = "en";

        Result result = new Result();
        process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
            if (e.Data != null)
            {
                result.outputs.Add(e.Data);
            }
        };
        process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => {
            if (e.Data != null)
            {
                result.errors.Add(e.Data);
            }
        };
        process.EnableRaisingEvents = true;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        return result;
    }
}
