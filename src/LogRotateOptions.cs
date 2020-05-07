using System;
using System.IO;

namespace logrotate
{
    public class LogRotateOptions
    {
        /// <summary>root directory, like logs\</summary>
        public string Root { get; set; }

        /// <summary>file filter, like *.logs
        /// </summary>
        public string Filter { get; set; }

        public RotateType RotateType { get; set; }

        /// <summary>rotate type parameter, 1:00:00</summary>
        public string RotateArguments { get; set; }

        /// <summary>rotate days</summary>
        public int Rotate { get; set; }

        public bool Compress { get; set; }

        /// <summary>compress delay days</summary>
        public int DelayCompress { get; set; }

        /// <summary>include sub directories</summary>
        public bool IncludeSubDirs { get; set; }

        /// <summary> shell scripts to be executed before logrotate</summary>
        public string[] PreScripts { get; set; }

        /// <summary> shell scripts to be executed after logrotate</summary>
        public string[] PostScripts { get; set; }

        /// <summary> shell before/after script execute timeout</summary>
        public TimeSpan ScriptTimeout { get; set; }

        #region Store
        public void Store(TextWriter writer)
        {
            writer.WriteLine(" " + (string.IsNullOrEmpty(Root) ? Filter : (Root + "\\" + Filter)) + " { ");
            writer.WriteLine("   " + RotateType.ToString().ToLower() + " " + RotateArguments);
            writer.WriteLine("   rotate " + Rotate);
            writer.WriteLine("   compress " + (Compress ? "on" : "off"));
            writer.WriteLine("   delaycompress " + DelayCompress);
            writer.WriteLine("   scripttimeout {0:c}", ScriptTimeout);
            if (PreScripts.Length > 0)
            {
                writer.WriteLine();
                writer.WriteLine("   prerotate");
                foreach (var script in PreScripts) writer.WriteLine("      " + script);
                writer.WriteLine("   endscript");
            }
            if (PostScripts.Length > 0)
            {
                writer.WriteLine();
                writer.WriteLine("   postrotate");
                foreach (var script in PostScripts) writer.WriteLine("      " + script);
                writer.WriteLine("   endscript");
            }
            if (IncludeSubDirs) writer.WriteLine("   includesubdirs");
            writer.WriteLine(" }");
        }
        #endregion
    }
}
