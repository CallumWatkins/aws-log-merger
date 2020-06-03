using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace AWSLogMerger
{
    /// <summary>
    /// Supports writing output logs to files.
    /// </summary>
    internal sealed class FileLogWriter : LogWriter
    {
        private readonly bool _overwrite;
        private readonly bool _gzip;

        public FileLogWriter(string basePath, bool overwrite, bool gzip) : base(basePath)
        {
            _overwrite = overwrite;
            _gzip = gzip;
        }

        public override bool SupportsParallelWriting => true;

        public override void Write(string name, IEnumerable<string> content)
        {
            string path = Path.Combine(NamePrefix, name);
            if (_gzip) path += ".gz";

            Stream file = _overwrite
                ? File.Open(path, FileMode.Create, FileAccess.Write)
                : File.Open(path, FileMode.CreateNew, FileAccess.Write);

            if (_gzip) file = new GZipStream(file, CompressionLevel.Optimal);
            using var sw = new StreamWriter(file);

            foreach (var line in content)
                sw.WriteLine(line);
        }
    }
}
