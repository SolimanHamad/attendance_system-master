using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace AttendanceSystem.Data.QueryFilter
{
    public class QueryFilterModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            QueryFilter queryFilter = new QueryFilter();
            IQueryCollection query = bindingContext.ActionContext.HttpContext.Request.Query;
            if (query.ContainsKey("start"))
                queryFilter.Start = int.Parse(query["start"]);
            if (query.ContainsKey("length"))
                queryFilter.Length = int.Parse(query["length"]);
            if (query.ContainsKey("filters"))
                queryFilter.Filters = JsonConvert.DeserializeObject<List<FilterModel>>(query["filters"]);
            if (query.ContainsKey("orderBy"))
                queryFilter.OrderBy = query["orderBy"].ToString();
            if (query.ContainsKey("orderDirection"))
                queryFilter.OrderDirection = query["orderDirection"].ToString();
            bindingContext.Result = ModelBindingResult.Success(queryFilter);
            return Task.CompletedTask;
        }
    }
}
