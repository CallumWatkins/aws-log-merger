using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;

namespace AWSLogMerger
{
    /// <summary>
    /// Can parse CloudFront Web/RTMP Distribution log files.
    /// <see cref="https://docs.aws.amazon.com/AmazonCloudFront/latest/DeveloperGuide/AccessLogs.html#LogFileFormat"/>
    /// </summary>
    internal class CloudFrontLogParser : TemporalLogParser
    {
        protected override ILogFileReader GetLogFileReader(string path)
        {
            return new CloudFrontLogFileReader(path);
        }

        protected override DateTime ExtractDateTime(string entry)
        {
            ReadOnlySpan<char> dateTime = entry.AsSpan().Slice(0, 19);
            if (DateTime.TryParseExact(dateTime, "yyyy-MM-dd\tHH:mm:ss", null, DateTimeStyles.AssumeUniversal, out DateTime result))
                return result;
            else
                throw new ParseException($"Unable to find time in log entry: '{entry}'.");
        }

        private sealed class CloudFrontLogFileReader : ILogFileReader
        {
            private readonly StreamReader _sr;

            public CloudFrontLogFileReader(string path)
            {
                Stream reader = File.OpenRead(path);
                if (Path.GetExtension(path) == ".gz")
                {
                    // Decompress .gz file
                    reader = new GZipStream(reader, CompressionMode.Decompress, true);
                }
                _sr = new StreamReader(reader);
            }

            public IEnumerator<string> GetEnumerator()
            {
                // Skip first two lines (file format version and W3C fields)
                _sr.ReadLine();
                _sr.ReadLine();

                while (!_sr.EndOfStream)
                    yield return _sr.ReadLine();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void Dispose() => _sr.Dispose();
        }
    }
}