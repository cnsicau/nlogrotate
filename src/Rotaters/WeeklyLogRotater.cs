using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace logrotate
{
    public class WeeklyLogRotater : LogRotater
    {
        private readonly DateTime[] times;
        public WeeklyLogRotater(LogRotateOptions options) : base(RotateType.Weekly, options)
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
                        throw new NotSupportedException("invalid arguments: " + options.RotateArguments);
                    }
                    else if (times[i].Day >= 7) throw new InvalidOperationException("valid date is 1 - 7");
                }
            }
        }

        protected override bool IsMatch(DateTime dateTime)
        {
            for (int i = 0; i < times.Length; i++)
            {
                if (times[i].Day % 7 == (int)dateTime.DayOfWeek
                    && times[i].Hour == dateTime.Hour
                    && times[i].Minute == dateTime.Minute
                    && times[i].Second == dateTime.Second) return true;
            }
            return false;
        }

        protected override string GetRotateSuffix(int rotateSize)
        {
            var date = rotateTime.AddDays(7 * rotateSize);
            // 2018W07
            return date.Year + "W" + Math.Ceiling(date.DayOfYear / 7.0).ToString().PadLeft(2, '0');
        }
        protected override bool IsLogrotatedFile(string fileName)
        {
            return Regex.IsMatch(fileName, @"\d{4}W\d{2}(.gz)?$");
        }
    }
}
