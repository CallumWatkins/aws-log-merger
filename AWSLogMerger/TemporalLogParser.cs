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
        public override IEnumerable<string> Parse(ICollection<string> paths)
        {
            var lines = new ConcurrentBag<(DateTime dateTime, string entry)>();

            Parallel.ForEach(paths, path =>
            {
                using ILogFileReader entryEnumerator = GetLogFileReader(path);
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