using System;
using System.Collections.Generic;
using System.IO;

namespace logrotate
{
    public class LogRotateOptionsBuilder
    {
        private readonly Dictionary<string, string> parameters = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        private readonly string root;
        private readonly List<string> preScripts = new List<string>();
        private readonly List<string> postScripts = new List<string>();

        public LogRotateOptionsBuilder(string root) { this.root = root; }

        public LogRotateOptionsBuilder AddBuildParameter(string name, string value)
        {
            parameters[name] = value;

            return this;
        }

        public virtual void AddPreScripts(string script) { preScripts.Add(script.TrimStart()); }

        public virtual void AddPostScripts(string script) { postScripts.Add(script.TrimStart()); }

        public virtual LogRotateOptions Build()
        {
            var options = new LogRotateOptions();

            BuildRotateType(options);
            BuildRotate(options);
            BuildCompress(options);
            BuildDelayCompress(options);
            BuildIncludeSubDirs(options);

            BuildRootFilter(options);

            options.PreScripts = preScripts.ToArray();
            options.PostScripts = postScripts.ToArray();

            return options;
        }

        private void BuildRotateType(LogRotateOptions options)
        {
            foreach (var name in Enum.GetNames(typeof(RotateType)))
            {
                string arguments;
                if (parameters.TryGetValue(name, out arguments))
                {
                    options.RotateType = (RotateType)Enum.Parse(typeof(RotateType), name);
                    options.RotateArguments = arguments;
                }
            }
        }

        private void BuildRotate(LogRotateOptions options)
        {
            string rotate;
            if (parameters.TryGetValue("rotate", out rotate))
            {
                int days;
                if (!int.TryParse(rotate, out days))
                {
                    throw new InvalidOperationException("invalid rotate value " + rotate);
                }
                options.Rotate = days;
            }
            else
            {
                options.Rotate = 90;
            }
        }

        private void BuildCompress(LogRotateOptions options)
        {
            string compress;
            options.Compress = true;
            if (parameters.TryGetValue("compress", out compress))
            {
                if (compress == "off") options.Compress = false;
                else if (!string.IsNullOrEmpty(compress) && compress != "on")
                    throw new InvalidOperationException("invalid compress value " + compress);
            }
        }

        private void BuildIncludeSubDirs(LogRotateOptions options)
        {
            options.IncludeSubDirs = parameters.ContainsKey("includesubdirs");
        }

        private void BuildDelayCompress(LogRotateOptions options)
        {
            string delaycompress;
            int days = 1;
            if (parameters.TryGetValue("delaycompress", out delaycompress))
            {
                if (!string.IsNullOrEmpty(delaycompress)
                    && !int.TryParse(delaycompress, out days))
                    throw new InvalidOperationException("invalid delaycompress value " + delaycompress);

                if (days < 0)
                    throw new InvalidOperationException("delaycompress out of range 0.");
            }
            options.DelayCompress = days;
        }

        private void BuildRootFilter(LogRotateOptions options)
        {
            options.Root = Path.GetDirectoryName(root);
            options.Filter = Path.GetFileName(root);

            if (string.IsNullOrEmpty(options.Root))
                options.Root = ".";

            if (string.IsNullOrEmpty(options.Filter))
                options.Filter = "*.log";
        }
    }
}
