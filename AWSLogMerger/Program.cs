using System;
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
            ParserResult<Options> parserResult = Parser.Default.ParseArguments<Options>(args);

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

        }
    }
}
