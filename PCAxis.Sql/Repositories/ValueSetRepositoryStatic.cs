using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using PCAxis.Sql.DbClient;
using PCAxis.Sql.DbConfig;

namespace PCAxis.Sql.Repositories
{
    internal static class ValueSetRepositoryStatic
    {
        private static readonly List<string> LanguagesInDbConfig;
        private static readonly Dictionary<string, string> ValuesQueryByLanguage = new Dictionary<string, string>();
        private static readonly string ExistsInLangQuery;
        private static readonly AbstractQueries Queries;

        static ValueSetRepositoryStatic()
        {
            LanguagesInDbConfig = SqlDbConfigsStatic.DefaultDatabase.ListAllLanguages();

            var config = SqlDbConfigsStatic.DefaultDatabase;
            Queries = AbstractQueries.GetSqlqueries(config);

            InfoForDbConnection info = config.GetInfoForDbConnection(config.GetDefaultConnString());
            var sqlCommand = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);

            ExistsInLangQuery = GetValuesetExistsInLangSql(Queries, sqlCommand);

            foreach (var language in LanguagesInDbConfig)
            {
                ValuesQueryByLanguage[language] = Queries.GetValueSetValuesQuery(language, sqlCommand);
            }
        }

        internal static Models.ValueSet GetValueSet(string valuesetId, string language)
        {
            //validate input
            if (valuesetId == null || language == null)
            {
                return null;
            }

            var config = SqlDbConfigsStatic.DefaultDatabase;
            InfoForDbConnection info = config.GetInfoForDbConnection(config.GetDefaultConnString());
            var cmd = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);

            DbParameter[] parameters = GetParameters(valuesetId, cmd);
            DataSet ExistsInLangDS = cmd.ExecuteSelect(ExistsInLangQuery, parameters);
            var availableLanguages = ParseLangs(ExistsInLangDS);

            if (!availableLanguages.Contains(language))
            {
                throw new ApplicationException("ValueSet " + valuesetId + " not found for language " + language);
            }

            Models.ValueSet myOut = Queries.GetPartialValueset(language, valuesetId);
            myOut.AvailableLanguages.AddRange(availableLanguages);


            //Add the list of values
            string sqlValues = ValuesQueryByLanguage[language];
            //mssql needs a refresh of the parameters (oracle does not).  
            parameters = GetParameters(valuesetId, cmd);

            DataSet valuesDS = cmd.ExecuteSelect(sqlValues, parameters);
            myOut.Values = ParseValues(valuesetId, valuesDS);

            return myOut;
        }

        private static DbParameter[] GetParameters(string valuesetId, PxSqlCommandForTempTables cmd)
        {
            System.Data.Common.DbParameter[] parameters = new System.Data.Common.DbParameter[1];
            parameters[0] = cmd.GetStringParameter("aValueSet", valuesetId);
            return parameters;
        }

        private static string GetValuesetExistsInLangSql(AbstractQueries queries, PxSqlCommand sqlCommand)
        {
            string sqlValuesetExistsInLang = string.Empty;

            string glue = String.Empty;
            foreach (var lang in LanguagesInDbConfig)
            {
                sqlValuesetExistsInLang += glue + queries.GetValueSetExistsIn(lang, sqlCommand);
                glue = " UNION ";
            }
            return sqlValuesetExistsInLang;
        }


        private static List<string> ParseLangs(DataSet langsDS)
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


        private static List<Models.Value> ParseValues(string valuesetId, DataSet valuesDS)
        {
            if (valuesDS.Tables.Count == 0)
            {
                throw new ApplicationException("ValueSet " + valuesetId + " empty");
            }
            List<Models.Value> myOut = new List<Models.Value>();

            for (int i = 0; i < valuesDS.Tables[0].Rows.Count; i++)
            {
                var v = new Models.Value();
                v.Code = valuesDS.Tables[0].Rows[i][0].ToString();
                v.Text = valuesDS.Tables[0].Rows[i][3] == DBNull.Value ? valuesDS.Tables[0].Rows[i][4].ToString() : valuesDS.Tables[0].Rows[i][3].ToString();
                myOut.Add(v);
            }

            return myOut;
        }

    }

}









