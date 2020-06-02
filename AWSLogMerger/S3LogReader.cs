using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace AWSLogMerger
{
    internal class S3LogReader : LogReader
    {
        protected override ILogFileReader GetLogFileReader(string file)
        {
            return new S3LogFileReader(file);
        }

        protected override DateTime ExtractDateTime(string entry)
        {
            int seenFields = 0;
            for (var i = 0; i < entry.Length; ++i)
            {
                char c = entry[i];
                if (c == ' ' && ++seenFields == 2)
                {
                    for (int j = i + 2; j < entry.Length; ++j)
                    {
                        c = entry[j];
                        if (c == ']')
                        {
                            ReadOnlySpan<char> dateTime = entry.AsSpan().Slice(i + 2, j - i - 2);
                            if (DateTime.TryParseExact(dateTime, "dd/MMM/yyyy:HH:mm:ss zzz", null, DateTimeStyles.AdjustToUniversal, out DateTime result))
                                return result;
                            else
                                throw new ParseException($"Unable to parse time in log entry: '{dateTime.ToString()}'.");
                        }
                    }
                }
            }
            throw new ParseException($"Unable to find time in log entry: '{entry}'.");
        }

        private sealed class S3LogFileReader : ILogFileReader
        {
            private readonly StreamReader _sr;

            public S3LogFileReader(string file)
            {
                FileStream reader = File.OpenRead(file);
                _sr = new StreamReader(reader);
            }

            public IEnumerator<string> GetEnumerator()
            {
                while (!_sr.EndOfStream)
                    yield return _sr.ReadLine();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void Dispose() => _sr.Dispose();
        }
    }
}