using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace AWSLogMerger
{
    internal sealed class FileLogWriter : LogWriter
    {
        private readonly bool _overwrite;
        private readonly bool _gzip;

        public FileLogWriter(string basePath, bool overwrite) : base(basePath)
        {
            _overwrite = overwrite;
            _gzip = gzip;
        }

        public override bool SupportsParallelWriting => true;

        public override void Write(string name, IEnumerable<string> content)
        {
            string path = Path.Combine(NamePrefix, name);
            if (_gzip) path += ".gz";

            if (!_overwrite && File.Exists(path)) throw new IOException($"File already exists: '{path}'");

            Stream file = File.OpenWrite(path);
            if (_gzip) file = new GZipStream(file, CompressionLevel.Optimal);
            using var sw = new StreamWriter(file);

            foreach (var line in content)
                sw.WriteLine(line);
        }
    }
}
