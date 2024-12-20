using System;

using PCAxis.Sql.DbConfig;

namespace PCAxis.Sql.Repositories
{
    internal abstract class AbstractQueries
    {
        internal abstract string GetValueSetExistsIn(string lang, PxSqlCommand sqlCommand);

        internal abstract string GetValueSetQuery(string lang, PxSqlCommand sqlCommand);

        internal abstract string GetValueSetValuesQuery(string lang, PxSqlCommand sqlCommand);

        internal abstract string GetGroupingExistsIn(string lang, PxSqlCommand sqlCommand);

        internal abstract string GetGroupingQuery(string lang, PxSqlCommand sqlCommand);

        internal abstract string GetGroupingValuesQuery(string lang, PxSqlCommand sqlCommand);

        internal static AbstractQueries GetSqlqueries(SqlDbConfig config)
        {
            if (config.MetaModel.Equals("2.1"))
            {
                return new PCAxis.Sql.QueryLib_21.Queries(config);
            }
            else if (config.MetaModel.Equals("2.2"))
            {
                return new PCAxis.Sql.QueryLib_22.Queries(config);
            }
            else if (config.MetaModel.Equals("2.3"))
            {
                return new PCAxis.Sql.QueryLib_23.Queries(config);
            }
            else if (config.MetaModel.Equals("2.4"))
            {
                return new PCAxis.Sql.QueryLib_24.Queries(config);
            }
            else
            {
                throw new NotImplementedException("Unknown MetaModel version: " + config.MetaModel);
            }


        }
    }
}
