using System;
using System.Collections.Generic;

namespace AWSLogMerger
{
    internal abstract class LogParser
    {
        public abstract IEnumerable<string> Parse(ICollection<string> fileNames);

        protected abstract IEntryEnumerator GetEntryEnumerator(string file);

        protected interface IEntryEnumerator : IEnumerable<string>, IDisposable
        {
        }
    }
}