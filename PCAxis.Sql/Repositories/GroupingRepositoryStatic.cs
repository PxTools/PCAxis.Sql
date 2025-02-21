using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using PCAxis.Sql.DbClient;
using PCAxis.Sql.DbConfig;
using PCAxis.Sql.Models;

namespace PCAxis.Sql.Repositories
{
    internal static class GroupingRepositoryStatic
    {
        private static readonly List<string> LanguagesInDbConfig;

        private static readonly Dictionary<string, string> ValuesQueryByLanguage = new Dictionary<string, string>();
        private static readonly string ExistsInLangQuery;
        private static readonly AbstractQueries Queries;



        static GroupingRepositoryStatic()
        {
            LanguagesInDbConfig = SqlDbConfigsStatic.DefaultDatabase.ListAllLanguages();

            var config = SqlDbConfigsStatic.DefaultDatabase;
            Queries = AbstractQueries.GetSqlqueries(config);

            InfoForDbConnection info = config.GetInfoForDbConnection(config.GetDefaultConnString());
            var sqlCommand = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);

            ExistsInLangQuery = GetValuesetExistsInLangSql(Queries, sqlCommand);

            foreach (var language in LanguagesInDbConfig)
            {
                ValuesQueryByLanguage[language] = Queries.GetGroupingValuesQuery(language, sqlCommand);
            }

        }

        internal static Models.Grouping GetGrouping(string groupingId, string language)
        {
            //validate input
            if (groupingId == null || language == null)
            {
                return null;
            }

            var config = SqlDbConfigsStatic.DefaultDatabase;
            InfoForDbConnection info = config.GetInfoForDbConnection(config.GetDefaultConnString());
            var cmd = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);

            DbParameter[] parameters = GetParameters(groupingId, cmd);
            DataSet ExistsInLangDS = cmd.ExecuteSelect(ExistsInLangQuery, parameters);
            var availableLanguages = AbstractQueries.ParseLangs(ExistsInLangDS);

            if (!availableLanguages.Contains(language))
            {
                throw new ApplicationException("Grouping " + groupingId + " not found for language " + language);
            }

            string sqlValues = ValuesQueryByLanguage[language];
            parameters = GetParameters(groupingId, cmd);
            var valuesDS = cmd.ExecuteSelect(sqlValues, parameters);

            List<GroupedValue> groupedValuesMaybe = ParseValues(groupingId, valuesDS);

            Models.Grouping myOut = Queries.FixGrouping(language, groupingId, groupedValuesMaybe);

            myOut.AvailableLanguages.AddRange(availableLanguages);

            return myOut;
        }

        private static DbParameter[] GetParameters(string groupingId, PxSqlCommandForTempTables cmd)
        {
            System.Data.Common.DbParameter[] parameters = new System.Data.Common.DbParameter[1];
            parameters[0] = cmd.GetStringParameter("aGrouping", groupingId);
            return parameters;
        }

        private static string GetValuesetExistsInLangSql(AbstractQueries queries, PxSqlCommand sqlCommand)
        {
            string sqlGroupingExistsInLang = String.Empty;
            string glue = String.Empty;
            foreach (var lang in LanguagesInDbConfig)
            {
                sqlGroupingExistsInLang += glue + queries.GetGroupingExistsIn(lang, sqlCommand);
                glue = " UNION ";

            }
            return sqlGroupingExistsInLang;
        }

        private static List<GroupedValue> ParseValues(string groupingId, DataSet valuesDS)
        {
            //Make sure we have a grouping
            if (valuesDS.Tables.Count == 0)
            {
                throw new ApplicationException("Grouping " + groupingId + " empty");
            }

            List<GroupedValue> myOut = new List<GroupedValue>();

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
                    myOut.Add(gValue);
                    values.Add(groupCode, gValue);
                }
                gValue.Codes.Add(valuesDS.Tables[0].Rows[i][1].ToString());
            }




            return myOut;
        }


    }
}
