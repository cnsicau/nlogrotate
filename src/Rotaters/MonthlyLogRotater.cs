using System;

namespace logrotate
{
    public class MonthlyLogRotater : LogRotater
    {
        private string time;
        public MonthlyLogRotater(LogRotateOptions options) : base(RotateType.Monthly, options)
        {
            this.time = options.RotateArguments ?? "1";
        }

        protected override bool IsMatch(DateTime dateTime)
        {
            return dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0
                    && dateTime.ToString("d") == time;
        }

        protected override string GetRotateSuffix(int rotateSize)
        {
            return rotateTime.AddMonths(rotateSize).ToString("yyyyMMdd");
        }
    }
}
