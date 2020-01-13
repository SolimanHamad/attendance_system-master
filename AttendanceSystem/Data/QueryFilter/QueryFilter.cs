using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Data.QueryFilter
{
    [ModelBinder(BinderType = typeof(QueryFilterModelBinder))]
    public class QueryFilter
    {
        public int Start { get; set; } = 1;
        public int Length { get; set; } = 25;
        public string OrderBy { get; set; }
        public string OrderDirection { get; set; }
        public List<FilterModel> Filters { get; set; }
        public long TotalCount { get; private set; }

        public async Task<IQueryable<T>> ApplyFilter<T>(IQueryable<T> query, HttpContext httpContext)
        {
            httpContext.Items.Add("filter", this);
            foreach (var filter in Filters.Where(f => !string.IsNullOrEmpty(f.Value)))
            {
                query = query.Where(filter.Property, filter.ComparisonType, filter.Value,filter.Value2);
            }
            
            if (!OrderBy.IsNullOrEmpty())
                query = query.OrderBy(OrderBy, OrderDirection == "desc");
            TotalCount = await query.CountAsync();
            IQueryable<T> filteredQuery = query.Skip(Start).Take(Length);
            return filteredQuery;
        }
    }
}