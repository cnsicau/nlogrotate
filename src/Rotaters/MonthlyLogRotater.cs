using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace logrotate
{
    public class MonthlyLogRotater : LogRotater
    {
        private readonly int day, hour, minute, second;

        public MonthlyLogRotater(LogRotateOptions options) : base(RotateType.Monthly, options)
        {
            DateTime time;
            if (string.IsNullOrEmpty(options.RotateArguments))
            {
                day = 1;
                hour = minute = second = 0;
            }
            else if (
               DateTime.TryParseExact(options.RotateArguments, "d H:m:s", null, DateTimeStyles.None, out time)
               || DateTime.TryParseExact(options.RotateArguments, "d H:m", null, DateTimeStyles.None, out time)
               || DateTime.TryParseExact(options.RotateArguments, "d", null, DateTimeStyles.None, out time))
            {
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
                    && dateTime.Day == day;
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
