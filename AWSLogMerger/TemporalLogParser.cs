using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AWSLogMerger
{
    internal abstract class TemporalLogParser : LogParser
    {
        public override IEnumerable<string> Parse(ICollection<string> fileNames)
        {
            if (fileNames.Count == 0) throw new ParseException("Source directory contains no files.");

            var lines = new ConcurrentBag<(DateTime dateTime, string entry)>();

            Parallel.ForEach(fileNames, file =>
            {
                using IEntryEnumerator entryEnumerator = GetEntryEnumerator(file);
                foreach (string entry in entryEnumerator)
                    lines.Add((ExtractDateTime(entry), entry));
            });

            return lines.AsParallel()
                .OrderBy(entry => entry.dateTime)
                .Select(entry => entry.entry);
        }

        protected abstract DateTime ExtractDateTime(string entry);
    }
}