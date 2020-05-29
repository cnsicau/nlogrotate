using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace logrotate
{
    public class YearlyLogRotater : LogRotater
    {
        private readonly DateTime[] times;

        public YearlyLogRotater(LogRotateOptions options) : base(RotateType.Yearly, options)
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
                    if (!DateTime.TryParseExact(args[i].Trim(), "M-d H:m:s", null, DateTimeStyles.None, out times[i])
                        && !DateTime.TryParseExact(args[i].Trim(), "M-d H:m", null, DateTimeStyles.None, out times[i])
                        && !DateTime.TryParseExact(args[i].Trim(), "M-d", null, DateTimeStyles.None, out times[i]))
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
                if (times[i].Month == dateTime.Month
                    && times[i].Day == dateTime.Day
                    && times[i].Hour == dateTime.Hour
                    && times[i].Minute == dateTime.Minute
                    && times[i].Second == dateTime.Second) return true;
            }
            return false;
        }

        protected override string GetRotateSuffix(int rotateSize)
        {
            return rotateTime.AddYears(rotateSize).ToString("yyyy");
        }
        protected override bool IsLogrotatedFile(string fileName)
        {
            return Regex.IsMatch(fileName, @"\d{4}(.gz)?$");
        }
    }
}
