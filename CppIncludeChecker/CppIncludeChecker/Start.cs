using System;
using System.Collections.Generic;
using System.IO;

namespace CppIncludeChecker
{
	class Start
	{
		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Usage: {0} SolutionFilePath [--applychange] [--ignoreselfheaderinclude] [--filenamefilter:xxxx.xxx]* [--includefilter:xxxx.h]*", Environment.CommandLine);
				return;
			}
			string solutionFilePath = args[0];
			if (File.Exists(solutionFilePath) == false)
			{
				Console.WriteLine("Cannot find the solution file:{0}", solutionFilePath);
				return;
			}
			bool applyChange = false;
			bool ignoreSelfHeaderInclude = false;
			List<string> filenameFilters = new List<string>();
			List<string> includeFilters = new List<string>();
			foreach (string arg in args)
			{
				if (arg.StartsWith("--") == false)
				{
					continue;
				}
				if (arg == "--applychange")
				{
					applyChange = true;
					continue;
				}
				string testString = "";

				testString = "--filenamefilter:";
				if (arg.StartsWith(testString))
				{
					filenameFilters.Add(arg.Substring(testString.Length));
					continue;
				}
				testString = "--includefilter:";
				if (arg.StartsWith(testString))
				{
					includeFilters.Add(arg.Substring(testString.Length));
					continue;
				}
			}

			using (MainProcess program = new MainProcess(solutionFilePath, applyChange, filenameFilters, includeFilters))
			{
				program.Start();
			}
		}
	}
}
