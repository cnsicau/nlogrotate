using System;
using System.Text.RegularExpressions;

namespace logrotate
{
    public class MinutelyLogRotater : LogRotater
    {
        private string time;
        public MinutelyLogRotater(LogRotateOptions options) : base(RotateType.Minutely, options)
        {
            this.time = options.RotateArguments ?? "0";
        }

        protected override bool IsMatch(DateTime dateTime)
        {
            return dateTime.Second.ToString() == time;
        }

        protected override string GetRotateSuffix(int rotateSize)
        {
            return rotateTime.AddMinutes(rotateSize).ToString("yyyyMMddHHmm");
        }

        protected override bool IsLogrotatedFile(string fileName)
        {
            return Regex.IsMatch(fileName, @"\d{12}(.gz)?$");
        }
    }
}
