using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;

namespace AWSLogMerger
{
    /// <summary>
    /// Can read CloudFront Web/RTMP Distribution log files.
    /// <see cref="https://docs.aws.amazon.com/AmazonCloudFront/latest/DeveloperGuide/AccessLogs.html#LogFileFormat"/>
    /// </summary>
    internal class CloudFrontLogReader : LogReader
    {
        protected override ILogFileReader GetLogFileReader(string path)
        {
            return new CloudFrontLogFileReader(path);
        }

        protected override DateTime ExtractDateTime(string entry)
        {
            ReadOnlySpan<char> dateTime = entry.AsSpan().Slice(0, 19);
            if (DateTime.TryParseExact(dateTime, "yyyy-MM-dd\tHH:mm:ss", null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime result))
                return result;
            else
                throw new ParseException($"Unable to find time in log entry: '{entry}'.");
        }

        private sealed class CloudFrontLogFileReader : ILogFileReader
        {
            private readonly string _path;

            public CloudFrontLogFileReader(string path)
            {
                _path = path;
            }

            private StreamReader OpenRead() {

                Stream reader = File.OpenRead(_path);
                if (Path.GetExtension(_path) == ".gz")
                {
                    // Decompress .gz file
                    reader = new GZipStream(reader, CompressionMode.Decompress);
                }
                return new StreamReader(reader);
            }

            public IEnumerable<string> GetHeaders()
            {
                using StreamReader sr = OpenRead();

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (line.StartsWith('#')) yield return line;
                    else yield break;
                }
            }

            public IEnumerable<string> GetEntries()
            {
                using StreamReader sr = OpenRead();

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    // Skip any line starting with #
                    if (line.StartsWith('#')) continue;

                    yield return line;
                }
            }
        }
    }
}
