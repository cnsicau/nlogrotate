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
    }
}
