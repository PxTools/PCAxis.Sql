using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using PCAxis.Sql.DbClient;
using PCAxis.Sql.DbConfig;

namespace PCAxis.Sql.Repositories
{
    internal static class TablesPublishedBetweenRepositoryStatic
    {

        private static readonly string TheSql;

        static TablesPublishedBetweenRepositoryStatic()
        {
            var config = SqlDbConfigsStatic.DefaultDatabase;
            AbstractQueries queries = AbstractQueries.GetSqlqueries(config);

            InfoForDbConnection info = config.GetInfoForDbConnection(config.GetDefaultConnString());
            var sqlCommand = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);

            TheSql = queries.GetTablesPublishedSinceQuery(sqlCommand);
        }


        internal static List<string> GetTablesPublishedBetween(DateTime from, DateTime to)
        {
            List<string> UrlTableIds = new List<string>();

            var config = SqlDbConfigsStatic.DefaultDatabase;
            InfoForDbConnection info = config.GetInfoForDbConnection(config.GetDefaultConnString());

            var cmd = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);
            DbParameter[] parameters = GetParameters(from, to, cmd);
            var dataSet = cmd.ExecuteSelect(TheSql, parameters);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                string tableId = row[0].ToString();
                if (string.IsNullOrEmpty(tableId))
                {
                    throw new InvalidOperationException("Cannot read from database: value in row[0] IsNullOrEmpty.");
                }
                UrlTableIds.Add(tableId);
            }

            return UrlTableIds;
        }

        private static DbParameter[] GetParameters(DateTime from, DateTime to, PxSqlCommandForTempTables cmd)
        {
            System.Data.Common.DbParameter[] parameters = new System.Data.Common.DbParameter[2];
            parameters[0] = cmd.GetDateParameter("aFrom", from);
            parameters[1] = cmd.GetDateParameter("aTo", to);
            return parameters;
        }


    }

}









