using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AttendanceSystem.Data.QueryFilter;

namespace AttendanceSystem.Extensions
{
    public static class ExpressionUtils
    {
        public static List<Expression<Func<T, bool>>> BuildPredicate<T>(string propertyName, ComparisonType comparison, string value,string value2)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var left = propertyName.Split('.').Aggregate((Expression)parameter, Expression.Property);
            return MakeComparison(left, comparison, value,value2).Select(e => Expression.Lambda<Func<T, bool>>(e, parameter)).ToList();
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string sortColumn, bool descending)
        {
            var parameter = Expression.Parameter(typeof(T), "p");

            string command = "OrderBy";

            if (descending)
            {
                command = "OrderByDescending";
            }

            try
            {
                // to avoid ordering by none existing columns in some view models, if it fails just return
                var propertyAccess = sortColumn.Split('.').Aggregate((Expression)parameter, Expression.Property);
                
                var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            
                Expression resultExpression = Expression.Call(typeof(Queryable), command, new[] { typeof(T), propertyAccess.Type },
                    query.Expression, Expression.Quote(orderByExpression));

                return query.Provider.CreateQuery<T>(resultExpression);
            }
            catch (Exception)
            {
                return query;
            }
        }

        private static List<Expression> MakeComparison(Expression left, ComparisonType comparison, string value,string value2)
        {
            switch (comparison)
            {
                case ComparisonType.Equal:
                    return new List<Expression> {MakeBinary(ExpressionType.Equal, left, value)};
                case ComparisonType.Range:
                    return new List<Expression> {MakeBinary(ExpressionType.GreaterThanOrEqual, left, value),MakeBinary(ExpressionType.LessThanOrEqual, left, value2)};
                default:
                    throw new NotSupportedException($"Invalid comparison operator '{comparison}'.");
            }
        }

        private static Expression MakeBinary(ExpressionType type, Expression left, string value)
        {
            object typedValue = value;
            if (left.Type != typeof(string))
            {
                if (string.IsNullOrEmpty(value))
                {
                    typedValue = null;
                    if (Nullable.GetUnderlyingType(left.Type) == null)
                        left = Expression.Convert(left, typeof(Nullable<>).MakeGenericType(left.Type));
                }
                else
                {
                    var valueType = Nullable.GetUnderlyingType(left.Type) ?? left.Type;
                    if (valueType.IsEnum && valueType.IsEnumDefined(int.Parse(value)))
                        typedValue = Enum.ToObject(valueType, int.Parse(value));
                    else
                        typedValue = valueType == typeof(Guid) ? Guid.Parse(value) : Convert.ChangeType(value, valueType);
                }
            }
            var right = Expression.Constant(typedValue, left.Type);
            return Expression.MakeBinary(type, left, right);
        }
    }
}