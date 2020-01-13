using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace AttendanceSystem.Extensions
{
    public static class HttpRequestExtensions
    {
        private static readonly Regex MobileCheck = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        public static bool IsMobileRequest(this HttpContext httpContext)
        {
            string agent = httpContext.Request.Headers["User-Agent"].ToString().ToLower();
            return MobileCheck.IsMatch(agent);
        }
        
        public static string GetSubDomain(this HttpContext httpContext)
        {
            var subDomain = string.Empty;

            var host = httpContext.Request.Host.Host;

            if (!string.IsNullOrWhiteSpace(host))
            {
                subDomain = host.Split('.')[0];
            }

            return subDomain.Trim().ToLower();
        }
    }
}
