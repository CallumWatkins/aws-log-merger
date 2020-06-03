using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;

namespace AWSLogMerger
{
    internal sealed class LogMerger
    {
        private readonly LogReader _reader;
        private readonly LogWriter _writer;

        public LogMerger(LogReader reader, LogWriter writer)
        {
            _reader = reader;
            _writer = writer;
        }

        /// <summary>
        /// Read, merge, and write log files.
        /// </summary>
        /// <param name="sourceDirectory">Path to the directory containing the log files.</param>
        /// <param name="period">The size of the period over which to combine entries.</param>
        public void Merge(string sourceDirectory, Period period)
        {
            string[] filePaths = Directory.GetFiles(sourceDirectory);
            if (filePaths.Length == 0) throw new Exception("Source directory contains no files.");

            IEnumerable<(DateTime dateTime, string entry)> entries = _reader.Read(filePaths, out ICollection<string> headers);
            IEnumerable<IGrouping<string, string>> outputGroups = GroupByPeriod(entries, period);
            WriteOutput(outputGroups, headers);
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
                    x => x.entry
                );
        }

        /// <summary>
        /// Write log groups with headers prepended.
        /// </summary>
        /// <param name="outputGroups">The groups to output.</param>
        /// <param name="headers">Headers to prepend to each group output.</param>
        private void WriteOutput(IEnumerable<IGrouping<string, string>> outputGroups, IEnumerable<string> headers)
        {
            if (_writer.SupportsParallelWriting)
            {
                Parallel.ForEach(outputGroups, outputGroup =>
                {
                    _writer.Write(outputGroup.Key, headers.Concat(outputGroup));
                });
            }
            else
            {
                foreach (var outputGroup in outputGroups)
                {
                    _writer.Write(outputGroup.Key, headers.Concat(outputGroup));
                }
            }
        }
    }
}
