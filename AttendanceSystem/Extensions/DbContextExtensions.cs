using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Extensions
{
    public static class DbContextExtensions
    {
        public static async Task<int> IntFromSQL(this DbContext context, string sql )
        {
            int count;
            context.Database.OpenConnection();
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                string result = (await command.ExecuteScalarAsync()).ToString();
                int.TryParse(result, out count);
            }
            return count;
        }

        public static string GetTableName<T>(this DbContext context,bool includeSchemeName = false) where T : class
        {
            string tableName = context.Model.FindEntityType(typeof(T)).Relational().TableName;
            return includeSchemeName ? $"{context.GetSchemaName()}.{tableName}" : tableName;
        }

        public static string GetSchemaName(this DbContext context)
        {
            return context.Database.GetDbConnection().Database;
        }
    }
}