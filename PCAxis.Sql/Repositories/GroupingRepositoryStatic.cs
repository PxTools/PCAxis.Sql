using System;
using System.Collections.Generic;
using System.Data;

using PCAxis.Sql.DbClient;
using PCAxis.Sql.DbConfig;
using PCAxis.Sql.Models;

namespace PCAxis.Sql.Repositories
{
    internal static class GroupingRepositoryStatic
    {
        private static readonly List<string> LanguagesInDbConfig;

        private static readonly Dictionary<string, List<string>> SqlquerysByLanguage;
        static GroupingRepositoryStatic()
        {
            LanguagesInDbConfig = SqlDbConfigsStatic.DefaultDatabase.ListAllLanguages();

            //Prepares the sqls. 3 per language. 
            SqlquerysByLanguage = new Dictionary<string, List<string>>();

            var config = SqlDbConfigsStatic.DefaultDatabase;
            AbstractQueries queries = AbstractQueries.GetSqlqueries(config);

            InfoForDbConnection info = config.GetInfoForDbConnection(config.GetDefaultConnString());
            var sqlCommand = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);

            foreach (var language in LanguagesInDbConfig)
            {
                string sqlGrouping = queries.GetGroupingQuery(language, sqlCommand);
                string sqlValues = queries.GetGroupingValuesQuery(language, sqlCommand); ;
                string sqlGroupingExistsInLang = GetOtherLanguagesSql(queries, language, config, sqlCommand);
                List<string> sqlquerys = new List<string> { sqlGrouping, sqlValues, sqlGroupingExistsInLang };
                SqlquerysByLanguage[language] = sqlquerys;
            }

        }

        internal static Models.Grouping GetGrouping(string groupingId, string language)
        {
            //validate input
            if (groupingId == null || language == null)
            {
                return null;
            }

            string sqlGrouping = SqlquerysByLanguage[language][0];
            string sqlValues = SqlquerysByLanguage[language][1];
            string sqlGroupingExistsInLang = SqlquerysByLanguage[language][2];

            var config = SqlDbConfigsStatic.DefaultDatabase;
            InfoForDbConnection info = config.GetInfoForDbConnection(config.GetDefaultConnString());
            var cmd = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);

            System.Data.Common.DbParameter[] parameters = new System.Data.Common.DbParameter[1];
            parameters[0] = cmd.GetStringParameter("aGrouping", groupingId);

            var groupingDS = cmd.ExecuteSelect(sqlGrouping, parameters);
            parameters = new System.Data.Common.DbParameter[1];
            parameters[0] = cmd.GetStringParameter("aGrouping", groupingId);
            var valuesDS = cmd.ExecuteSelect(sqlValues, parameters);

            DataSet extraLangsDS = String.IsNullOrEmpty(sqlGroupingExistsInLang) ? null : cmd.ExecuteSelect(sqlGroupingExistsInLang, parameters);


            Models.Grouping grouping = Parse(groupingId, groupingDS, valuesDS, extraLangsDS);

            //Adding langs we know exists without checking the DB 
            grouping.AvailableLanguages.Add(config.MainLanguage.code);
            if (!config.MainLanguage.code.Equals(language))
            {
                grouping.AvailableLanguages.Add(language);
            }

            return grouping;
        }

        private static string GetOtherLanguagesSql(AbstractQueries queries, string language, SqlDbConfig config, PxSqlCommand sqlCommand)
        {
            string sqlGroupingExistsInLang = String.Empty;
            string glue = String.Empty;
            foreach (var lang in LanguagesInDbConfig)
            {
                if (!lang.Equals(config.MainLanguage.code) && !lang.Equals(language))
                {
                    //skips: config.MainLanguage has to exist and language will fail in GetValueSetQuery if vaulset is not translated

                    sqlGroupingExistsInLang += glue + queries.GetGroupingExistsIn(lang, sqlCommand);
                    glue = " UNION ";
                }
            }
            return sqlGroupingExistsInLang;


        }

        private static Models.Grouping Parse(string groupingId, DataSet groupingDS, DataSet valuesDS, DataSet extraLangsDS)
        {
            //Make sure we have a grouping
            if (groupingDS.Tables.Count == 0 || groupingDS.Tables[0].Rows.Count < 1 || valuesDS.Tables.Count == 0)
            {
                throw new ApplicationException("Grouping " + groupingId + " not found or empty");
            }

            var grouping = new PCAxis.Sql.Models.Grouping();
            grouping.Id = groupingDS.Tables[0].Rows[0][0].ToString();
            grouping.Label = groupingDS.Tables[0].Rows[0][1].ToString();


            var values = new Dictionary<string, GroupedValue>();
            for (int i = 0; i < valuesDS.Tables[0].Rows.Count; i++)
            {
                string groupCode = valuesDS.Tables[0].Rows[i][0].ToString();
                GroupedValue gValue;

                if (values.ContainsKey(groupCode))
                {
                    gValue = values[groupCode];
                }
                else
                {
                    gValue = new GroupedValue();
                    gValue.Code = groupCode;
                    gValue.Text = valuesDS.Tables[0].Rows[i][2] == DBNull.Value ? valuesDS.Tables[0].Rows[i][3].ToString() : valuesDS.Tables[0].Rows[i][2].ToString();
                    grouping.Values.Add(gValue);
                    values.Add(groupCode, gValue);
                }
                gValue.Codes.Add(valuesDS.Tables[0].Rows[i][1].ToString());
            }

            if (extraLangsDS != null)
            {

                for (int i = 0; i < extraLangsDS.Tables[0].Rows.Count; i++)
                {
                    var lang = extraLangsDS.Tables[0].Rows[i][0].ToString();
                    grouping.AvailableLanguages.Add(lang);
                }
            }


            return grouping;
        }


    }
}
