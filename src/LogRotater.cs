﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace logrotate
{
    public abstract class LogRotater
    {
        delegate void RotateFileAction(string file);

        protected readonly LogRotateOptions options;

        protected DateTime rotateTime;

        public LogRotater(RotateType supportedType, LogRotateOptions options)
        {
            if (options.RotateType != supportedType)
                throw new NotSupportedException("only support " + supportedType + " but encounter " + options.RotateType);
            this.options = options;
        }

        protected abstract bool IsMatch(DateTime dateTime);

        public virtual void Rotate(DateTime dateTime)
        {
            if (!IsMatch(dateTime)) return;

            rotateTime = dateTime;

            var root = options.Root;
            if (!Directory.Exists(root))
            {
                root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.Root);
                if (!Directory.Exists(root)) return;
            }

            var sourceFiles = new List<string>();
            DetectRotateSourceFiles(root, sourceFiles);
            if (sourceFiles.Count == 0) return;

            RotateFiles(sourceFiles, ArchiveFile);

            //NginxDaemon.Run("-s reopen").WaitForExit();   // reopen

            RotateFiles(sourceFiles, CleanFile);
            if (options.Compress) RotateFiles(sourceFiles, CompressFile);
        }

        void DetectRotateSourceFiles(string root, List<string> sourceFilesContainer)
        {
            var files = Directory.GetFiles(root, options.Filter);
            var regex = new Regex("^" + options.Filter.Replace(".", "\\.").Replace("*", ".*") + "$");

            foreach (var file in files)
            {
                if (!regex.IsMatch(Path.GetFileName(file))) continue;

                sourceFilesContainer.Add(file);
            }

            if (options.IncludeSubDirs)
            {
                foreach (var subDir in Directory.GetDirectories(root))
                {
                    DetectRotateSourceFiles(subDir, sourceFilesContainer);
                }
            }
        }

        void RotateFiles(IEnumerable<string> sourceFiles, RotateFileAction rotate)
        {
            foreach (var sourceFile in sourceFiles)
            {
                try { rotate(sourceFile); }
                catch (Exception e) { Trace.TraceError("rotate file " + sourceFile + " failed: " + e); }
            }
        }

        protected static void Compress(string file)
        {
            var gzFileName = file + ".gz";
            using (var source = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                if (source.Length == 0) // empty clean file 
                {
                    File.Delete(file);
                    return;
                }
                using (var gzFileStream = File.Create(gzFileName))
                {
                    using (var gzStream = new GZipStream(gzFileStream, CompressionMode.Compress))
                    {
                        var buffer = new byte[8192];
                        int size;
                        while ((size = source.Read(buffer, 0, 8192)) > 0)
                        {
                            gzStream.Write(buffer, 0, size);
                        }
                    }
                }
            }

            File.SetAccessControl(gzFileName, File.GetAccessControl(file));
            File.SetCreationTimeUtc(gzFileName, File.GetCreationTimeUtc(file));
            File.SetLastWriteTimeUtc(gzFileName, File.GetLastWriteTimeUtc(file));

            File.Delete(file);
        }

        protected abstract string GetRotateSuffix(int rotateSize);

        protected virtual void CleanFile(string file)
        {
            var fileInfo = new FileInfo(file);
            var suffix = "-" + GetRotateSuffix(-options.Rotate);
            var expires = fileInfo.Name + suffix;
            File.Delete(file + suffix);
            File.Delete(file + suffix + ".gz");
            var files = fileInfo.Directory.GetFiles(fileInfo.Name + "-*");
            foreach (var rotateFile in files)
            {
                if (string.Compare(rotateFile.Name, expires) <= 0) rotateFile.Delete();
            }
        }

        protected virtual void ArchiveFile(string file)
        {
            var fileInfo = new FileInfo(file);
            if (fileInfo.Exists && fileInfo.Length != 0)
            {
                fileInfo.MoveTo(file + "-" + GetRotateSuffix(0));
            }
        }

        protected virtual void CompressFile(string file)
        {
            for (var i = -options.Rotate; i <= -options.DelayCompress; i++)
            {
                var compressFile = file + "-" + GetRotateSuffix(i);
                if (File.Exists(compressFile))
                    try { Compress(compressFile); } catch { }
            }
        }

        public static LogRotater Create(LogRotateOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            switch (options.RotateType)
            {
                case RotateType.Minutely: return new MinutelyLogRotater(options);
                case RotateType.Hourly: return new HourlyLogRotater(options);
                case RotateType.Daily: return new DailyLogRotater(options);
                case RotateType.Weekly: return new WeeklyLogRotater(options);
                case RotateType.Monthly: return new MonthlyLogRotater(options);
                case RotateType.Yearly: return new YearlyLogRotater(options);
            }
            throw new NotSupportedException(options.RotateType.ToString());
        }
    }
}