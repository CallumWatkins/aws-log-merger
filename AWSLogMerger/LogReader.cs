using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSLogMerger
{
    internal abstract class LogReader
    {
        public IEnumerable<(DateTime dateTime, string entry)> Read(ICollection<string> paths, out ICollection<string> headers)
        {
            var lines = new ConcurrentBag<(DateTime dateTime, string entry)>();

            Parallel.ForEach(paths, path =>
            {
                ILogFileReader entryEnumerator = GetLogFileReader(path);
                foreach (string entry in entryEnumerator.GetEntries())
                    lines.Add((ExtractDateTime(entry), entry));
            });

            headers = paths.Count == 0
                ? new string[0]
                : GetLogFileReader(paths.First()).GetHeaders().ToArray();

            return lines.AsParallel()
                .OrderBy(entry => entry.dateTime);
        }

        protected abstract DateTime ExtractDateTime(string entry);

        protected abstract ILogFileReader GetLogFileReader(string path);

        protected interface ILogFileReader
        {
            IEnumerable<string> GetHeaders();

            IEnumerable<string> GetEntries();
        }
    }
}
