using System;

namespace AttendanceSystem.Extensions
{
    public static class GUIDExtensions
    {
        public static string ToStringWithNoDashes(this Guid guid)
        {
            return guid.ToString().Replace("-", "");
        }
    }
}
