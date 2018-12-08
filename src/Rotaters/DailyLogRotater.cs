﻿using System;

namespace logrotate
{
    public class DailyLogRotater : LogRotater
    {
        private string time;
        public DailyLogRotater(LogRotateOptions options) : base(RotateType.Daily, options)
        {
            this.time = options.RotateArguments ?? "0:00:00";
        }

        protected override bool IsMatch(DateTime dateTime)
        {
            return dateTime.ToString("H:mm:ss") == time;
        }

        protected override string GetRotateSuffix(int rotateSize)
        {
            return rotateTime.AddDays(rotateSize).ToString("yyyyMMdd");
        }
    }
}