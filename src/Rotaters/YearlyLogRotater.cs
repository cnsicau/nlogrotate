using System;

namespace logrotate
{
    public class YearlyLogRotater : LogRotater
    {
        private string time;
        public YearlyLogRotater(LogRotateOptions options) : base(RotateType.Yearly, options)
        {
            this.time = options.RotateArguments ?? "1.1";
        }

        protected override bool IsMatch(DateTime dateTime)
        {
            return dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0
                    && dateTime.ToString("M.d") == time;
        }

        protected override string GetRotateSuffix(int rotateSize)
        {
            return rotateTime.AddYears(rotateSize).ToString("yyyy");
        }
    }
}
