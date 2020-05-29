using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace logrotate
{
    public class DailyLogRotater : LogRotater
    {
        private readonly DateTime[] times;
        public DailyLogRotater(LogRotateOptions options) : base(RotateType.Daily, options)
        {
            if (string.IsNullOrEmpty(options.RotateArguments))
            {
                times = new DateTime[] { DateTime.MinValue };
            }
            else
            {
                var args = options.RotateArguments.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                times = new DateTime[args.Length];
                for (int i = 0; i < times.Length; i++)
                {
                    if (!DateTime.TryParseExact(args[i].Trim(), "H:m:s", null, DateTimeStyles.None, out times[i])
                        && !DateTime.TryParseExact(args[i].Trim(), "H:m", null, DateTimeStyles.None, out times[i]))
                    {
                        throw new NotSupportedException("invalid arguments: " + options.RotateArguments);
                    }
                }
            }
        }

        protected override bool IsMatch(DateTime dateTime)
        {
            for (int i = 0; i < times.Length; i++)
            {
                if (times[i].Hour == dateTime.Hour
                    && times[i].Minute == dateTime.Minute
                    && times[i].Second == dateTime.Second) return true;
            }
            return false;
        }

        protected override string GetRotateSuffix(int rotateSize)
        {
            return rotateTime.AddDays(rotateSize).ToString("yyyyMMdd");
        }

        protected override bool IsLogrotatedFile(string fileName)
        {
            return Regex.IsMatch(fileName, @"\d{8}(.gz)?$");
        }
    }
}
