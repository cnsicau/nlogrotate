using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace logrotate
{
    public class YearlyLogRotater : LogRotater
    {
        private readonly int month, day, hour, minute, second;

        public YearlyLogRotater(LogRotateOptions options) : base(RotateType.Yearly, options)
        {
            DateTime time;
            if (string.IsNullOrEmpty(options.RotateArguments))
            {
                month = day = 1;
                hour = minute = second = 0;
            }
            else if (
               DateTime.TryParseExact(options.RotateArguments, "M-d H:m:s", null, DateTimeStyles.None, out time)
               || DateTime.TryParseExact(options.RotateArguments, "M-d", null, DateTimeStyles.None, out time))
            {
                month = time.Month;
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
                    && dateTime.Day == day
                    && dateTime.Month == month;
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
