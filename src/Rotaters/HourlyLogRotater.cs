using System;

namespace logrotate
{

    public class HourlyLogRotater : LogRotater
    {
        private string time;
        public HourlyLogRotater(LogRotateOptions options) : base(RotateType.Hourly, options)
        {
            this.time = options.RotateArguments ?? "00:00";
        }

        protected override bool IsMatch(DateTime dateTime)
        {
            return dateTime.ToString("mm:ss") == time;
        }

        protected override string GetRotateSuffix(int rotateSize)
        {
            return rotateTime.AddHours(rotateSize).ToString("yyyyMMddHH");
        }
    }

}
