using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
            if (!Directory.Exists(options.OutputDirectory))
            {
                Console.Error.WriteLine("Output directory does not exist.");
                error = true;
            }

        /// <summary>
        /// Group log file entries into distinct groups based on their timestamp and the size of the period.
        /// </summary>
        /// <param name="entries">The log file entries to group.</param>
        /// <param name="period">The size of the period over which to combine entries.</param>
        /// <returns>Returns groupings of entries with keys indicating the unique periods they represent.</returns>
        private static IEnumerable<IGrouping<string, string>> GroupByPeriod(IEnumerable<(DateTime dateTime, string entry)> entries, Period period)
        {
            return entries
                // Group entries by the ISO 8601 string denoting the period they are in
                .GroupBy<(DateTime dateTime, string entry), string, string>(
                // Select key
                period switch
                {
                    Period.Hourly => x => x.dateTime.ToString("yyyy-MM-ddTHH"),
                    Period.Daily => x => x.dateTime.ToString("yyyy-MM-dd"),
                    Period.Weekly => x => $"{ISOWeek.GetYear(x.dateTime):D4}-W{ISOWeek.GetWeekOfYear(x.dateTime):D2}",
                    Period.Monthly => x => x.dateTime.ToString("yyyy-MM"),
                    Period.Yearly => x => x.dateTime.ToString("yyyy"),
                    Period.All => _ => "all",
                    _ => throw new ArgumentOutOfRangeException(nameof(period)),
                },
                // Select element
                x => x.entry);
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

                string[] filePaths = Directory.GetFiles(options.SourceDirectory);
                if (filePaths.Length == 0) throw new Exception("Source directory contains no files.");

                IEnumerable<(DateTime dateTime, string entry)> entries = parser.Parse(filePaths);
                IEnumerable<IGrouping<string, string>> outputGroups = GroupByPeriod(entries, options.OutputPeriod);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Environment.Exit(2);
            }
        }
    }
}
