using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace logrotate
{
    public class WeeklyLogRotater : LogRotater
    {
        private string time;
        public WeeklyLogRotater(LogRotateOptions options) : base(RotateType.Weekly, options)
        {
            this.time = options.RotateArguments ?? "0";
        }

        protected override bool IsMatch(DateTime dateTime)
        {
            return dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0
                    && ((int)dateTime.DayOfWeek).ToString() == time;
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
