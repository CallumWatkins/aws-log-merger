using System;
using System.Collections.Generic;
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

        [Option('p', "period", Required = false, HelpText = "Set output period.")]
        public Period OutputPeriod { get; set; }
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

            if (error) Environment.Exit(1);
        }

        private static IEnumerable<(string name, IEnumerable<string> lines)> Split(IEnumerable<(DateTime dateTime, string entry)> entries, IEnumerable<string> headers, Period period)
        {
            throw new NotImplementedException();
        }

        private static void Run(Options options)
        {
            try
            {
                LogParser parser = options.Type switch
                {
                    LogType.S3 => new S3LogParser(),
                    LogType.CloudFront => new CloudFrontLogParser(),
                    _ => throw new ArgumentOutOfRangeException(nameof(options.Type), "Unrecognised log file type.")
                };

                string[] filePaths = Directory.GetFiles(options.SourceDirectory);
                if (filePaths.Length == 0) throw new Exception("Source directory contains no files.");

                IEnumerable<(DateTime dateTime, string entry)> entries = parser.Parse(filePaths);
                IEnumerable<(string name, IEnumerable<string> lines)> outputGroups = Split(entries, new[] { "#Test Header" }, options.OutputPeriod);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Environment.Exit(2);
            }
        }
    }
}
