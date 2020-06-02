using System.Collections.Generic;

namespace AWSLogMerger
{
    internal abstract class LogWriter
    {
        public LogWriter(string namePrefix)
        {
            NamePrefix = namePrefix;
        }

        public abstract bool SupportsParallelWriting { get; }

        public string NamePrefix { get; }

        public abstract void Write(string name, IEnumerable<string> content);
    }
}
