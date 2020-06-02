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

                IEnumerable<string> result = parser.Parse(filePaths);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Environment.Exit(2);
            }
        }
    }
}
