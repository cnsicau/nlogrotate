using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace logrotate
{

    public class HourlyLogRotater : LogRotater
    {
        private readonly int minute, second;
        public HourlyLogRotater(LogRotateOptions options) : base(RotateType.Hourly, options)
        {
            DateTime time;
            if (string.IsNullOrEmpty(options.RotateArguments))
            {
                minute = second = 0;
            }
            else if (
               DateTime.TryParseExact(options.RotateArguments, "m:s", null, DateTimeStyles.None, out time)
               || DateTime.TryParseExact(options.RotateArguments, "m", null, DateTimeStyles.None, out time))
            {
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
                    && dateTime.Minute == minute;
        }

        protected override string GetRotateSuffix(int rotateSize)
        {
            return rotateTime.AddHours(rotateSize).ToString("yyyyMMddHH");
        }
        protected override bool IsLogrotatedFile(string fileName)
        {
            return Regex.IsMatch(fileName, @"\d{10}(.gz)?$");
        }
    }

}
