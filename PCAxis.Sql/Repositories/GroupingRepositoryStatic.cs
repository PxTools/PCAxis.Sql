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

            InfoForDbConnection info = config.GetInfoForDbConnection(config.GetDefaultConnString());
            var cmd = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);

            foreach (var language in LanguagesInDbConfig)
            {
                GetQueries(language, out string sqlGrouping, out string sqlValues, out string sqlGroupingExistsInLang, config, cmd);
                List<string> sqlquerys = new List<string> { sqlGrouping, sqlValues, sqlGroupingExistsInLang };
                SqlquerysByLanguage[language] = sqlquerys;
            }

        }

        internal static Models.Grouping GetGrouping(string name, string language)
        {
            //validate input
            if (name == null || language == null)
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
            parameters[0] = cmd.GetStringParameter("aGrouping", name);

            var groupingDS = cmd.ExecuteSelect(sqlGrouping, parameters);
            var valuesDS = cmd.ExecuteSelect(sqlValues, parameters);

            DataSet extraLangsDS = String.IsNullOrEmpty(sqlGroupingExistsInLang) ? null : cmd.ExecuteSelect(sqlGroupingExistsInLang, parameters);


            Grouping grouping = Parse(groupingDS, valuesDS, extraLangsDS);

            //Adding langs we know exists without checking the DB 
            grouping.AvailableLanguages.Add(config.MainLanguage.code);
            if (!config.MainLanguage.code.Equals(language))
            {
                grouping.AvailableLanguages.Add(language);
            }

            return grouping;
        }

        private static void GetQueries(string language, out string sqlGrouping, out string sqlValues, out string sqlGroupingExistsInLang, SqlDbConfig config, PxSqlCommand sqlCommand)
        {
            sqlGrouping = string.Empty;
            sqlValues = string.Empty;
            sqlGroupingExistsInLang = string.Empty;

            if (config.MetaModel.Equals("2.1"))
            {
                SqlDbConfig_21 cfg = config as SqlDbConfig_21;
                sqlGrouping = QueryLib_21.Queries.GetGroupingQuery(cfg, language, sqlCommand);
                sqlValues = QueryLib_21.Queries.GetGroupingValuesQuery(cfg, language, sqlCommand);

                string glue = String.Empty;
                foreach (var lang in LanguagesInDbConfig)
                {
                    if (!lang.Equals(config.MainLanguage.code) && !lang.Equals(language))
                    {
                        //skips: config.MainLanguage has to exist and language will fail in GetValueSetQuery if vaulset is not translated

                        sqlGroupingExistsInLang += glue + QueryLib_21.Queries.GetGroupingExistsIn((SqlDbConfig_21)config, lang, sqlCommand);
                        glue = " UNION ";
                    }
                }

            }
            else if (config.MetaModel.Equals("2.2"))
            {
                SqlDbConfig_22 cfg = config as SqlDbConfig_22;
                sqlGrouping = QueryLib_22.Queries.GetGroupingQuery(cfg, language, sqlCommand);
                sqlValues = QueryLib_22.Queries.GetGroupingValuesQuery(cfg, language, sqlCommand);

                string glue = String.Empty;
                foreach (var lang in LanguagesInDbConfig)
                {
                    if (!lang.Equals(config.MainLanguage.code) && !lang.Equals(language))
                    {
                        sqlGroupingExistsInLang += glue + QueryLib_22.Queries.GetGroupingExistsIn((SqlDbConfig_22)config, lang, sqlCommand);
                        glue = " UNION ";
                    }
                }
            }
            else if (config.MetaModel.Equals("2.3"))
            {
                //var meta = new QueryLib_23.MetaQuery((SqlDbConfig_23)config, config.GetInfoForDbConnection("", ""));
                //meta.LanguageCodes = config.GetAllLanguages();
                SqlDbConfig_23 cfg = config as SqlDbConfig_23;
                sqlGrouping = QueryLib_23.Queries.GetGroupingQuery(cfg, language, sqlCommand);
                sqlValues = QueryLib_23.Queries.GetGroupingValuesQuery(cfg, language, sqlCommand);

                string glue = String.Empty;
                foreach (var lang in LanguagesInDbConfig)
                {
                    if (!lang.Equals(config.MainLanguage.code) && !lang.Equals(language))
                    {
                        //skips: config.MainLanguage has to exist and language will fail in GetValueSetQuery if vaulset is not translated

                        sqlGroupingExistsInLang += glue + QueryLib_23.Queries.GetGroupingExistsIn((SqlDbConfig_23)config, lang, sqlCommand);
                        glue = " UNION ";
                    }
                }

            }
            else if (config.MetaModel.Equals("2.4"))
            {
                SqlDbConfig_24 cfg = config as SqlDbConfig_24;
                sqlGrouping = QueryLib_24.Queries.GetGroupingQuery(cfg, language, sqlCommand);
                sqlValues = QueryLib_24.Queries.GetGroupingValuesQuery(cfg, language, sqlCommand);

                string glue = String.Empty;
                foreach (var lang in LanguagesInDbConfig)
                {
                    if (!lang.Equals(config.MainLanguage.code) && !lang.Equals(language))
                    {
                        //skips: config.MainLanguage has to exist and language will fail in GetValueSetQuery if vaulset is not translated

                        sqlGroupingExistsInLang += glue + QueryLib_24.Queries.GetGroupingExistsIn((SqlDbConfig_24)config, lang, sqlCommand);
                        glue = " UNION ";
                    }
                }
            }

        }

        private static Models.Grouping Parse(DataSet valueGroup, DataSet vsValue, DataSet extraLangsDS)
        {
            //Make sure we have a grouping
            if (valueGroup.Tables.Count == 0 || valueGroup.Tables[0].Rows.Count < 1 || vsValue.Tables.Count == 0)
            {
                throw new ApplicationException("Bad Grouping");
            }

            var grouping = new PCAxis.Sql.Models.Grouping();
            grouping.Id = valueGroup.Tables[0].Rows[0][0].ToString();
            grouping.Name = valueGroup.Tables[0].Rows[0][1].ToString();


            var values = new Dictionary<string, GroupedValue>();
            for (int i = 0; i < vsValue.Tables[0].Rows.Count; i++)
            {
                string groupCode = vsValue.Tables[0].Rows[i][0].ToString();
                GroupedValue gValue;

                if (values.ContainsKey(groupCode))
                {
                    gValue = values[groupCode];
                }
                else
                {
                    gValue = new GroupedValue();
                    gValue.Code = groupCode;
                    gValue.Text = vsValue.Tables[0].Rows[i][2] == DBNull.Value ? vsValue.Tables[0].Rows[i][3].ToString() : vsValue.Tables[0].Rows[i][2].ToString();
                    grouping.Values.Add(gValue);
                    values.Add(groupCode, gValue);
                }
                gValue.Codes.Add(vsValue.Tables[0].Rows[i][1].ToString());
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
