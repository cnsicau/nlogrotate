using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace logrotate
{
    public class FileLogRotateOptionsBuilderProvider : LogRotateOptionsBuilderProvider
    {
        private readonly string file;

        class DefaultLogRotateOptionsBuilder : LogRotateOptionsBuilder
        {
            public DefaultLogRotateOptionsBuilder() : base(string.Empty) { }

            public override LogRotateOptions Build()
            {
                return new LogRotateOptions
                {
                    Compress = true,
                    DelayCompress = 1,
                    Filter = "*.log",
                    Root = @".",
                    IncludeSubDirs = true,
                    Rotate = 90,
                    RotateArguments = "0:00:00",
                    RotateType = RotateType.Daily
                };
            }
        }

        public FileLogRotateOptionsBuilderProvider(string file)
        {
            this.file = file;
        }

        private LogRotateOptionsBuilder ParseBuilder(StreamReader reader, string line)
        {
            var root = ReadRootLine(line);
            var builder = new LogRotateOptionsBuilder(root);
            bool eof = false, pre = false, post = false;
            while (!eof && !reader.EndOfStream)
            {
                line = reader.ReadLine();
                eof = Regex.IsMatch(line, @"^\s*}\s*$");
                if (!eof && !IsCommentOrEmpty(line))
                {
                    var instruction = line.Trim().ToLower();
                    switch (instruction)
                    {
                        case "endscript": pre = post = false; break;
                        case "prerotate": pre = true; break;
                        case "postrotate": post = true; break;
                        default:
                            if (pre) builder.AddPreScripts(line);
                            else if (post) builder.AddPostScripts(line);
                            else EmitBuildParameter(builder, line);
                            break;
                    }

                }
            }

            if (!eof) throw new InvalidOperationException("missing eof }");
            return builder;
        }

        public override LogRotateOptionsBuilder[] CreateBuilders()
        {
            var file = this.file;

            if (string.IsNullOrEmpty(file)) return new LogRotateOptionsBuilder[0];

            if (!File.Exists(file))
            {
                file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
                if (!File.Exists(file)) return new LogRotateOptionsBuilder[0];
            }

            var builders = new List<LogRotateOptionsBuilder>();

            using (var stream = File.OpenRead(file))
            {
                var reader = new StreamReader(stream);
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (IsCommentOrEmpty(line)) continue;

                    builders.Add(ParseBuilder(reader, line));
                }

                return builders.ToArray();
            }
        }

        string ReadRootLine(string line)
        {
            var match = Regex.Match(line, @"^\s*(?<root>.*?)\s*\{\s*$");
            if (!match.Success) throw new InvalidOperationException("invalid root " + line);
            return match.Groups["root"].Value;
        }

        void EmitBuildParameter(LogRotateOptionsBuilder builder, string line)
        {
            var match = Regex.Match(line, @"^\s*(?<name>\S+)(\s+(?<val>.*?))?\s*$");
            if (!match.Success)
                throw new InvalidOperationException("invalid config " + line);
            var name = match.Groups["name"].Value;
            var value = match.Groups["val"].Value.TrimEnd();
            builder.AddBuildParameter(name, string.IsNullOrEmpty(value) ? null : value);
        }

        bool IsCommentOrEmpty(string line) { return Regex.IsMatch(line, @"^\s*(#|\s*$)"); }
    }
}
