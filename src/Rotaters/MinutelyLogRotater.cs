using System;
using System.Text.RegularExpressions;

namespace logrotate
{
    public class MinutelyLogRotater : LogRotater
    {
        private readonly int second;
        public MinutelyLogRotater(LogRotateOptions options) : base(RotateType.Minutely, options)
        {
            if (string.IsNullOrEmpty(options.RotateArguments))
            {
                second = 0;
            }
            else if (!int.TryParse(options.RotateArguments, out second) || second >= 60)
            {
                throw new NotSupportedException("invalid arguments: " + options.RotateArguments);
            }
        }

        protected override bool IsMatch(DateTime dateTime)
        {
            return dateTime.Second == second;
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
