using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSLogMerger
{
    internal abstract class LogReader
    {
        /// <summary>
        /// Read multiple log files and pair each entry with its timestamp.
        /// </summary>
        /// <param name="paths">The paths to the log files to read.</param>
        /// <param name="headers">Headers from the first log file.</param>
        /// <returns>
        /// Returns all entries from the log files with their timestamps, sorted in ascending order.
        /// Also outputs the headers of the first log file.
        /// </returns>
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

        /// <summary>
        /// Extract the timestamp from a single log file entry.
        /// </summary>
        /// <param name="entry">The log file entry to parse.</param>
        /// <returns>Returns the timestamp stored in the entry as a UTC adjusted DateTime.</returns>
        protected abstract DateTime ExtractDateTime(string entry);

        /// <summary>
        /// Get a log file reader for a single log file.
        /// </summary>
        /// <param name="path">The path to the file to read.</param>
        /// <returns>Returns a new file reader for the given path.</returns>
        protected abstract ILogFileReader GetLogFileReader(string path);

        /// <summary>
        /// Represents an object that can read a log file and retrieve all headers or entries.
        /// </summary>
        protected interface ILogFileReader
        {
            /// <summary>
            /// Get all headers from the log file.
            /// </summary>
            /// <returns>Returns all headers of the log file.</returns>
            IEnumerable<string> GetHeaders();

            /// <summary>
            /// Get all entries from the log file.
            /// Will discard of any other information such as headers and comments.
            /// </summary>
            /// <returns>Returns all entries of the log file.</returns>
            IEnumerable<string> GetEntries();
        }
    }
}
