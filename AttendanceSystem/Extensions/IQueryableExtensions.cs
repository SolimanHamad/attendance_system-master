using System.Linq;
using AttendanceSystem.Data.QueryFilter;

namespace AttendanceSystem.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Where<T>(this IQueryable<T> query, string propertyName, ComparisonType comparisonType, string value, string value2 = null)
        {
            foreach (var predict in ExpressionUtils.BuildPredicate<T>(propertyName, comparisonType, value, value2))
            {
                query = query.Where(predict);
            }

            return query;
        }
    }
}
