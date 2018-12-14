using System;
using System.Collections.Generic;
using System.IO;

namespace logrotate
{
    public class DirectoryLogRotateOptionsBuilderProvider : LogRotateOptionsBuilderProvider
    {
        private readonly string directory;

        public DirectoryLogRotateOptionsBuilderProvider(string directory)
        {
            this.directory = directory;
        }

        public override LogRotateOptionsBuilder[] CreateBuilders()
        {
            var directory = this.directory;

            if (string.IsNullOrEmpty(directory)) return new LogRotateOptionsBuilder[0];

            if (!Directory.Exists(directory))
            {
                directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directory);
                if (!Directory.Exists(directory)) return new LogRotateOptionsBuilder[0];
            }

            var builders = new List<LogRotateOptionsBuilder>();
            foreach (var file in Directory.GetFiles(directory))
            {
                var fileProvider = new FileLogRotateOptionsBuilderProvider(file);
                builders.AddRange(fileProvider.CreateBuilders());
            }

            return builders.ToArray();
        }
    }
}
