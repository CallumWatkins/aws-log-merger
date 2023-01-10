# AWS Log Merger
A command line tool for merging AWS log files together.

## Build
Targets .NET Core 3.1.

    dotnet build -c release

## Interface

```console
> .\AWSLogMerger.exe --help

AWSLogMerger 1.0.0

  -t, --type         Required. Set log file type. Valid values: S3, CloudFront

  -s, --source       Required. Set log file source directory.

  -p, --period       Required. Set output period. Valid values: Hourly, Daily,
                     Weekly, Monthly, Yearly, All

  -o, --output       Required. Set combined destination directory.

  -g, --gzip         GZip before writing output logs.

  -h, --headers      Prepend headers found on input logs to all output logs.

  -v, --overwrite    Overwrite existing files when outputting logs.

  --help             Display this help screen.

  --version          Display version information.
```
