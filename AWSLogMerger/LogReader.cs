using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSLogMerger
{
    internal abstract class LogReader
    {
        public IEnumerable<(DateTime dateTime, string entry)> Read(ICollection<string> paths)
        {
            var lines = new ConcurrentBag<(DateTime dateTime, string entry)>();

            Parallel.ForEach(paths, path =>
            {
                using ILogFileReader entryEnumerator = GetLogFileReader(path);
                foreach (string entry in entryEnumerator)
                    lines.Add((ExtractDateTime(entry), entry));
            });

            return lines.AsParallel()
                .OrderBy(entry => entry.dateTime);
        }

        protected abstract DateTime ExtractDateTime(string entry);

        protected abstract ILogFileReader GetLogFileReader(string path);

        protected interface ILogFileReader : IEnumerable<string>, IDisposable
        {
        }
    }
}