using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace logrotate
{

    public class HourlyLogRotater : LogRotater
    {
        private readonly DateTime[] times;
        public HourlyLogRotater(LogRotateOptions options) : base(RotateType.Hourly, options)
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
                    if (!DateTime.TryParseExact(args[i].Trim(), "m:s", null, DateTimeStyles.None, out times[i]))
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
                if (times[i].Minute == dateTime.Minute
                    && times[i].Second == dateTime.Second) return true;
            }
            return false;
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
