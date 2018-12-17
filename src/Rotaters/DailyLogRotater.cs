using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace logrotate
{
    public class DailyLogRotater : LogRotater
    {
        private readonly int hour, minute, second;
        public DailyLogRotater(LogRotateOptions options) : base(RotateType.Daily, options)
        {
            DateTime time;
            if (string.IsNullOrEmpty(options.RotateArguments))
            {
                hour = minute = second = 0;
            }
            else if (
               DateTime.TryParseExact(options.RotateArguments, "H:m:s", null, DateTimeStyles.None, out time)
               || DateTime.TryParseExact(options.RotateArguments, "H:m", null, DateTimeStyles.None, out time))
            {
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
                    && dateTime.Hour == hour;
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
