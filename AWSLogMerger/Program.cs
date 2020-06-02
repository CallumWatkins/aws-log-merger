using System;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace AWSLogMerger
{
    class Options
    {
        [Option('t', "type", Required = true, HelpText = "Set log file type.")]
        public LogType Type { get; set; }

        [Option('s', "source", Required = true, HelpText = "Set log file source directory.")]
        public string SourceDirectory { get; set; }

        [Option('p', "period", Required = true, HelpText = "Set output period.")]
        public Period OutputPeriod { get; set; }

        [Option('o', "output", Required = true, HelpText = "Set combined destination directory.")]
        public string OutputDirectory { get; set; }

        [Option('g', "gzip", Default = false, Required = false, HelpText = "GZip before writing output.")]
        public bool GZip { get; set; }
    }

    /// <summary>
    /// A period of time over which log files can be combined.
    /// </summary>
    internal enum Period
    {
                    // Example output group names (ISO 8601):
        Hourly,     // 2020-06-02T19
        Daily,      // 2020-06-02
        Weekly,     // 2020-W23
        Monthly,    // 2020-06
        Yearly,     // 2020
        All         // all
    }

    internal enum LogType
    {
        S3,
        CloudFront
    }

    class Program
    {
        private static void Main(string[] args)
        {
            ParserResult<Options> parserResult = new Parser(with => with.HelpWriter = null).ParseArguments<Options>(args);

            parserResult
                .WithParsed(CheckOptions)
                .WithParsed(Run)
                .WithNotParsed(errors =>
                {
                    var helpText = HelpText.AutoBuild(parserResult);
                    helpText.AddEnumValuesToHelpText = true;
                    helpText.AddOptions(parserResult);
                    Console.Error.Write(helpText);
                    Environment.Exit(1);
                });
        }

        private static void CheckOptions(Options options)
        {
            bool error = false;

            if (!Directory.Exists(options.SourceDirectory))
            {
                Console.Error.WriteLine("Source directory does not exist.");
                error = true;
            }

            if (!Directory.Exists(options.OutputDirectory))
            {
                Console.Error.WriteLine("Output directory does not exist.");
                error = true;
            }

            if (error) Environment.Exit(1);
        }

        private static void Run(Options options)
        {
            try
            {
                LogReader reader = options.Type switch
                {
                    LogType.S3 => new S3LogReader(),
                    LogType.CloudFront => new CloudFrontLogReader(),
                    _ => throw new ArgumentOutOfRangeException(nameof(options.Type), "Unrecognised log file type."),
                };
                LogWriter writer = new FileLogWriter(options.OutputDirectory, false, options.GZip);
                LogMerger logMerger = new LogMerger(reader, writer);

                logMerger.Merge(options.SourceDirectory, options.OutputPeriod);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Environment.Exit(2);
            }
        }
    }
}
