using System;
using System.Collections.Generic;
using System.Data;

using PCAxis.Sql.DbConfig;

namespace PCAxis.Sql.Repositories
{
    internal abstract class AbstractQueries
    {
        internal abstract string GetValueSetExistsIn(string lang, PxSqlCommand sqlCommand);

        //internal abstract string GetValueSetQuery(string lang, PxSqlCommand sqlCommand);

        internal abstract Models.ValueSet GetPartialValueset(string lang, string myValueSetId);

        internal abstract string GetValueSetValuesQuery(string lang, PxSqlCommand sqlCommand);

        internal abstract string GetGroupingExistsIn(string lang, PxSqlCommand sqlCommand);

        internal abstract string GetGroupingQuery(string lang, PxSqlCommand sqlCommand);

        internal abstract string GetGroupingValuesQuery(string lang, PxSqlCommand sqlCommand);


        internal abstract string GetMenuLookupTablesQuery(string lang);

        internal abstract string GetMenuLookupFolderQuery(string lang);

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


        internal static List<string> ParseLangs(DataSet langsDS)
        {
            List<string> myOut = new List<string>();
            if (langsDS != null)
            {
                for (int i = 0; i < langsDS.Tables[0].Rows.Count; i++)
                {
                    var lang = langsDS.Tables[0].Rows[i][0].ToString();
                    myOut.Add(lang);
                }
            }
            return myOut;
        }
    }
}
