using System;
using System.Collections.Generic;
using System.Text;

namespace logrotate
{
    public enum RotateType
    {
        Unknown,
        Minutely,
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }
}
