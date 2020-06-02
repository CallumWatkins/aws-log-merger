using System.Collections.Generic;
using System.IO;

namespace AWSLogMerger
{
    internal sealed class FileLogWriter : LogWriter
    {
        private readonly bool _overwrite;

        public FileLogWriter(string basePath, bool overwrite) : base(basePath)
        {
            _overwrite = overwrite;
        }

        public override bool SupportsParallelWriting => true;

        public override void Write(string name, IEnumerable<string> content)
        {
            string path = Path.Combine(NamePrefix, name);

            if (!_overwrite && File.Exists(path)) throw new IOException($"File already exists: '{path}'");

            Stream file = File.OpenWrite(path);
            using var sw = new StreamWriter(file);

            foreach (var line in content)
                sw.WriteLine(line);
        }
    }
}
