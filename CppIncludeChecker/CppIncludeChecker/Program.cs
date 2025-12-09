using System;

namespace CppIncludeChecker;

class Program
{
    static void OnCancelKeyPressed(Object sender, ConsoleCancelEventArgs args)
    {
        Console.WriteLine("Stop requested. Waiting to stop......");
        StopMarker.StopRequested = true;
        args.Cancel = true;
    }

    static void Main(string[] args)
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

        using Logger logger = new Logger("CppIncludeChecker.log");
        string version = "1.1";
        logger.Log($"CppIncludeChecker version:{version}");
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

        Console.CancelKeyPress += OnCancelKeyPressed;

        config.Print(logger);
        MainProcess mainProcess = new MainProcess(config, logger, builderCommand);
        mainProcess.Start();
    }
}
