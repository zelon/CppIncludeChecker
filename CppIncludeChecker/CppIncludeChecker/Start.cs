using System;

namespace CppIncludeChecker
{
	class Start
	{
		static void Main(string[] args)
		{
            string version = "1.0";
            Console.WriteLine($"CppIncludeChecker version:{version}");
            Config config = Config.Parse(args);
            if (config == null)
            {
                return;
            }
            string builderCommand = MsBuildBatchMaker.MakeAndGetPath(config.MsBuildCmdPath);
            if (builderCommand == null)
            {
                Console.WriteLine("Cannot find builderCommand");
                return;
            }
            using (Logger logger = new Logger("CppIncludeChecker.log"))
            {
                config.Print(logger);
                MainProcess mainProcess = new MainProcess(config, logger, builderCommand);
                mainProcess.Start();
            }

        }
	}
}
