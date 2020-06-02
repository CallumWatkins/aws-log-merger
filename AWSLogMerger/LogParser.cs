using System;
using System.Collections.Generic;

namespace AWSLogMerger
{
    internal abstract class LogParser
    {
        public abstract IEnumerable<string> Parse(ICollection<string> paths);

        protected abstract ILogFileReader GetLogFileReader(string path);

        protected interface ILogFileReader : IEnumerable<string>, IDisposable
        {
        }
    }
}