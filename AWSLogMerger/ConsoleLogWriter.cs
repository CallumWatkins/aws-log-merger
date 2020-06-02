using System;
using System.Collections.Generic;
using System.IO;

namespace AWSLogMerger
{
    internal sealed class ConsoleLogWriter : LogWriter
    {
        public ConsoleLogWriter(string namePrefix) : base(namePrefix)
        {
        }

        public override bool SupportsParallelWriting => false;

        public override void Write(string name, IEnumerable<string> content)
        {
            Console.WriteLine("");
            Console.WriteLine($"Output log '{Path.Combine(NamePrefix, name)}':");
            foreach (string line in content)
                Console.WriteLine(line);
        }
    }
}
