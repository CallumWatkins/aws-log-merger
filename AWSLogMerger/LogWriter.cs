using System.Collections.Generic;

namespace AWSLogMerger
{
    internal abstract class LogWriter
    {
        public LogWriter(string namePrefix)
        {
            NamePrefix = namePrefix;
        }

        /// <summary>
        /// Indicates if the concrete log writer class supports writing multiple logs in parallel.
        /// </summary>
        public abstract bool SupportsParallelWriting { get; }

        /// <summary>
        /// A value prepended to the name of logs before writing.
        /// </summary>
        public string NamePrefix { get; }

        /// <summary>
        /// Write a log with the given name and content.
        /// </summary>
        /// <param name="name">The name of the log group.</param>
        /// <param name="content">The content of the log group.</param>
        public abstract void Write(string name, IEnumerable<string> content);
    }
}
