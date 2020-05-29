using System;
using System.Text.RegularExpressions;

namespace logrotate
{
    public class MinutelyLogRotater : LogRotater
    {
        private readonly int[] seconds;
        public MinutelyLogRotater(LogRotateOptions options) : base(RotateType.Minutely, options)
        {
            if (string.IsNullOrEmpty(options.RotateArguments))
            {
                seconds = new int[0];
            }
            else
            {
                var args = options.RotateArguments.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                seconds = new int[args.Length];
                for (int i = 0; i < seconds.Length; i++)
                {
                    if (!int.TryParse(args[i].Trim(), out seconds[i]) || seconds[i] >= 60)
                    {
                        throw new NotSupportedException("invalid arguments: " + options.RotateArguments);
                    }
                }
            }
        }

        protected override bool IsMatch(DateTime dateTime)
        {
            return Array.IndexOf(seconds, dateTime.Second) != -1;
        }

        protected override string GetRotateSuffix(int rotateSize)
        {
            return rotateTime.AddMinutes(rotateSize).ToString("yyyyMMddHHmm");
        }

        protected override bool IsLogrotatedFile(string fileName)
        {
            return Regex.IsMatch(fileName, @"\d{12}(.gz)?$");
        }
    }
}
