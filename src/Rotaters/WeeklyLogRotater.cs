using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace logrotate
{
    public class WeeklyLogRotater : LogRotater
    {
        private readonly int day, hour, minute, second;
        public WeeklyLogRotater(LogRotateOptions options) : base(RotateType.Weekly, options)
        {
            DateTime time;
            if (string.IsNullOrEmpty(options.RotateArguments))
            {
                day = hour = minute = second = 0;
            }
            else if (
               DateTime.TryParseExact(options.RotateArguments, "d H:m:s", null, DateTimeStyles.None, out time)
               || DateTime.TryParseExact(options.RotateArguments, "d H:m", null, DateTimeStyles.None, out time)
               || DateTime.TryParseExact(options.RotateArguments, "d", null, DateTimeStyles.None, out time))
            {
                if (time.Day >= 7) throw new InvalidOperationException("valid date is 0 - 6");
                day = time.Day;
                hour = time.Hour;
                minute = time.Minute;
                second = time.Second;
            }
            else
            {
                throw new NotSupportedException("invalid arguments: " + options.RotateArguments);
            }
        }

        protected override bool IsMatch(DateTime dateTime)
        {
            return dateTime.Second == second
                    && dateTime.Minute == minute
                    && dateTime.Hour == hour
                    && dateTime.DayOfWeek == (DayOfWeek)day;
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
