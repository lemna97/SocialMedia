using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Highever.SocialMedia.Common
{
    public static class EntityFrameworkCoreExtensions
    {
        public static void UseTransaction(this DbContext @DbContext, Action action)
        {
            using (var transaction = @DbContext.Database.BeginTransaction())
            {
                action();
                @DbContext.SaveChanges();
                transaction.Commit();
            }
        }
        public static async Task UseTransactionAsync(this DbContext @DbContext, Action action)
        {
            using (var transaction = await @DbContext.Database.BeginTransactionAsync())//使用Using语句的时候事务操作无需手动捕获异常进行回滚
            {
                action();
                await @DbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }
        public static IEnumerable<dynamic> SqlQueryDynamic(this DbContext db, string Sql, params SqlParameter[] parameters)
        {
            using (var cmd = db.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = Sql;

                if (cmd.Connection.State != ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }

                foreach (var p in parameters)
                {
                    var dbParameter = cmd.CreateParameter();
                    dbParameter.DbType = p.DbType;
                    dbParameter.ParameterName = p.ParameterName;
                    dbParameter.Value = p.Value;
                    cmd.Parameters.Add(dbParameter);
                }

                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var row = new ExpandoObject() as IDictionary<string, object>;
                        for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                        {
                            row.Add(dataReader.GetName(fieldCount), dataReader[fieldCount]);
                        }
                        yield return row;
                    }
                }
            }
        }
        private static DbCommand CreateCommand(DatabaseFacade facade, string sql, out DbConnection connection, params object[] parameters)
        {
            var conn = facade.GetDbConnection();
            connection = conn;
            conn.Open();
            var cmd = conn.CreateCommand();
            if (facade.IsSqlServer())
            {
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(parameters);
            }
            return cmd;
        }
        public static DataTable SqlQuery(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out DbConnection conn, parameters);
            var reader = command.ExecuteReader();
            var dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            conn.Close();
            return dt;
        }
        public static List<TEntity> SqlQuery<TEntity>(this DatabaseFacade facade, string sql, params object[] parameters) where TEntity : class, new()
        {
            var dt = SqlQuery(facade, sql, parameters);
            return dt.ToList<TEntity>();
        }
        public static List<TEntity> ToList<TEntity>(this DataTable dt) where TEntity : class, new()
        {
            var propertyInfos = typeof(TEntity).GetProperties();
            var list = new List<TEntity>();
            foreach (DataRow row in dt.Rows)
            {
                var t = new TEntity();
                foreach (PropertyInfo p in propertyInfos)
                {
                    if (dt.Columns.IndexOf(p.Name) != -1 && row[p.Name] != DBNull.Value)
                        p.SetValue(t, row[p.Name], null);
                }
                list.Add(t);
            }
            return list;
        }
    }
}
