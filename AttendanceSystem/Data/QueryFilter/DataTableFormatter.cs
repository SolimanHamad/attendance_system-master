using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace AttendanceSystem.Data.QueryFilter
{
    public class DataTableFormatter : TextOutputFormatter
    {
        public DataTableFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/datatable"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(IList).IsAssignableFrom(type);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            QueryFilter filter = context.HttpContext.Items["filter"] as QueryFilter;
            if(context.Object is IList obj)
                await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    draw = context.HttpContext.Request.Query["draw"],
                    recordsFiltered = filter?.TotalCount ?? obj.Count,
                    recordsTotal = filter?.TotalCount ?? obj.Count,
                    data = obj,
                }));
        }
    }
}
