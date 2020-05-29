using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace logrotate
{
    public class MonthlyLogRotater : LogRotater
    {
        private readonly DateTime[] times;

        public MonthlyLogRotater(LogRotateOptions options) : base(RotateType.Monthly, options)
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
                    if (!DateTime.TryParseExact(args[i].Trim(), "d H:m:s", null, DateTimeStyles.None, out times[i])
                        && !DateTime.TryParseExact(args[i].Trim(), "d H:m", null, DateTimeStyles.None, out times[i]))
                    {
                        int d;
                        if(!int.TryParse(args[i].Trim(), out d) || d > 31) throw new NotSupportedException("invalid arguments: " + options.RotateArguments);
                        times[i] = new DateTime(0, 0, d);
                    }
                }
            }
        }

        protected override bool IsMatch(DateTime dateTime)
        {
            for (int i = 0; i < times.Length; i++)
            {
                if (times[i].Day == dateTime.Day
                    && times[i].Hour == dateTime.Hour
                    && times[i].Minute == dateTime.Minute
                    && times[i].Second == dateTime.Second) return true;
            }
            return false;
        }

        protected override string GetRotateSuffix(int rotateSize)
        {
            return rotateTime.AddMonths(rotateSize).ToString("yyyyMM");
        }
        protected override bool IsLogrotatedFile(string fileName)
        {
            return Regex.IsMatch(fileName, @"\d{6}(.gz)?$");
        }
    }
}
