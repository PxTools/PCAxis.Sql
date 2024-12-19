using System;
using System.Collections.Generic;
using System.Data;

using PCAxis.Sql.DbClient;
using PCAxis.Sql.DbConfig;
using PCAxis.Sql.Models;

namespace PCAxis.Sql.Repositories
{
    internal static class ValueSetRepositoryStatic
    {
        private static readonly List<string> LanguagesInDbConfig;

        private static readonly Dictionary<string, List<string>> SqlquerysByLanguage;
        static ValueSetRepositoryStatic()
        {
            LanguagesInDbConfig = SqlDbConfigsStatic.DefaultDatabase.ListAllLanguages();

            //Prepares the sqls. 3 per language. 
            SqlquerysByLanguage = new Dictionary<string, List<string>>();

            var config = SqlDbConfigsStatic.DefaultDatabase;

            InfoForDbConnection info = config.GetInfoForDbConnection(config.GetDefaultConnString());
            var cmd = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);

            foreach (var language in LanguagesInDbConfig)
            {
                GetQueries(language, out string sqlValueset, out string sqlValues, out string sqlValuesetExistsInLang, config, cmd);
                List<string> sqlquerys = new List<string> { sqlValueset, sqlValues, sqlValuesetExistsInLang };
                SqlquerysByLanguage[language] = sqlquerys;
            }

        }

        internal static Models.ValueSet GetValueSet(string name, string language)
        {
            //validate input
            if (name == null || language == null)
            {
                return null;
            }

            string sqlValueset = SqlquerysByLanguage[language][0];
            string sqlValues = SqlquerysByLanguage[language][1];
            string sqlValuesetExistsInLang = SqlquerysByLanguage[language][2];

            var config = SqlDbConfigsStatic.DefaultDatabase;
            InfoForDbConnection info = config.GetInfoForDbConnection(config.GetDefaultConnString());
            var cmd = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);

            System.Data.Common.DbParameter[] parameters = new System.Data.Common.DbParameter[1];
            parameters[0] = cmd.GetStringParameter("aValueSet", name);

            var valuesetDS = cmd.ExecuteSelect(sqlValueset, parameters);
            var valueDS = cmd.ExecuteSelect(sqlValues, parameters);

            DataSet extraLangsDS = String.IsNullOrEmpty(sqlValuesetExistsInLang) ? null : cmd.ExecuteSelect(sqlValuesetExistsInLang, parameters);

            ValueSet valueset = Parse(name, valuesetDS, valueDS, extraLangsDS);

            //Adding langs we know exists without checking the DB 
            valueset.AvailableLanguages.Add(config.MainLanguage.code);
            if (!config.MainLanguage.code.Equals(language))
            {
                valueset.AvailableLanguages.Add(language);
            }
            return valueset;
        }

        private static void GetQueries(string language, out string sqlValueset, out string sqlValues, out string sqlValuesetExistsInLang, SqlDbConfig config, PxSqlCommand sqlCommand)
        {
            sqlValueset = string.Empty;
            sqlValues = string.Empty;
            sqlValuesetExistsInLang = string.Empty;

            if (config.MetaModel.Equals("2.1"))
            {
                sqlValueset = QueryLib_21.Queries.GetValueSetQuery((SqlDbConfig_21)config, language, sqlCommand);
                sqlValues = QueryLib_21.Queries.GetValueSetValuesQuery((SqlDbConfig_21)config, language, sqlCommand);
                string glue = String.Empty;
                foreach (var lang in LanguagesInDbConfig)
                {
                    if (!lang.Equals(config.MainLanguage.code) && !lang.Equals(language))
                    {
                        //skips: config.MainLanguage has to exist and language will fail in GetValueSetQuery if vaulset is not translated

                        sqlValuesetExistsInLang += glue + QueryLib_21.Queries.GetValueSetExistsIn((SqlDbConfig_21)config, lang, sqlCommand);
                        glue = " UNION ";
                    }
                }
            }
            else if (config.MetaModel.Equals("2.2"))
            {
                sqlValueset = QueryLib_22.Queries.GetValueSetQuery((SqlDbConfig_22)config, language, sqlCommand);
                sqlValues = QueryLib_22.Queries.GetValueSetValuesQuery((SqlDbConfig_22)config, language, sqlCommand);

                string glue = String.Empty;
                foreach (var lang in LanguagesInDbConfig)
                {
                    if (!lang.Equals(config.MainLanguage.code) && !lang.Equals(language))
                    {
                        //skips: config.MainLanguage has to exist and language will fail in GetValueSetQuery if vaulset is not translated

                        sqlValuesetExistsInLang += glue + QueryLib_22.Queries.GetValueSetExistsIn((SqlDbConfig_22)config, lang, sqlCommand);
                        glue = " UNION ";
                    }
                }
            }
            else if (config.MetaModel.Equals("2.3"))
            {
                sqlValueset = QueryLib_23.Queries.GetValueSetQuery((SqlDbConfig_23)config, language, sqlCommand);
                sqlValues = QueryLib_23.Queries.GetValueSetValuesQuery((SqlDbConfig_23)config, language, sqlCommand);

                string glue = String.Empty;
                foreach (var lang in LanguagesInDbConfig)
                {
                    if (!lang.Equals(config.MainLanguage.code) && !lang.Equals(language))
                    {
                        //skips: config.MainLanguage has to exist and language will fail in GetValueSetQuery if vaulset is not translated

                        sqlValuesetExistsInLang += glue + QueryLib_23.Queries.GetValueSetExistsIn((SqlDbConfig_23)config, lang, sqlCommand);
                        glue = " UNION ";
                    }
                }

            }
            else if (config.MetaModel.Equals("2.4"))
            {
                sqlValueset = QueryLib_24.Queries.GetValueSetQuery((SqlDbConfig_24)config, language, sqlCommand);
                sqlValues = QueryLib_24.Queries.GetValueSetValuesQuery((SqlDbConfig_24)config, language, sqlCommand);

                string glue = String.Empty;
                foreach (var lang in LanguagesInDbConfig)
                {
                    if (!lang.Equals(config.MainLanguage.code) && !lang.Equals(language))
                    {
                        //skips: config.MainLanguage has to exist and language will fail in GetValueSetQuery if vaulset is not translated

                        sqlValuesetExistsInLang += glue + QueryLib_24.Queries.GetValueSetExistsIn((SqlDbConfig_24)config, lang, sqlCommand);
                        glue = " UNION ";
                    }
                }
            }
        }

        private static Models.ValueSet Parse(string name, DataSet valuesetDS, DataSet vsValue, DataSet extraLangsDS)
        {
            if (valuesetDS.Tables.Count == 0 || valuesetDS.Tables[0].Rows.Count < 1 || vsValue.Tables.Count == 0)
            {
                throw new ApplicationException("Bad ValueSet");
            }

            ValueSet valueset = new ValueSet();
            valueset.Id = name;
            valueset.Name = valuesetDS.Tables[0].Rows[0][1].ToString();


            //PresText came in version 2.1 and is optional  ...  desciption is up to 200 chars
            if (String.IsNullOrEmpty(valueset.Name))
            {
                var asPresText = valuesetDS.Tables[0].Rows[0][2].ToString(); ;
                int gridPosition = asPresText.IndexOf('#');
                if (gridPosition > 0)
                {
                    asPresText = asPresText.Substring(0, gridPosition);
                }
                valueset.Name = asPresText;
            }




            for (int i = 0; i < vsValue.Tables[0].Rows.Count; i++)
            {
                var v = new Models.Value();
                v.Code = vsValue.Tables[0].Rows[i][0].ToString();
                v.Text = vsValue.Tables[0].Rows[i][3] == DBNull.Value ? vsValue.Tables[0].Rows[i][4].ToString() : vsValue.Tables[0].Rows[i][3].ToString();
                valueset.Values.Add(v);
            }

            if (extraLangsDS != null)
            {

                for (int i = 0; i < extraLangsDS.Tables[0].Rows.Count; i++)
                {
                    var lang = extraLangsDS.Tables[0].Rows[i][0].ToString();
                    valueset.AvailableLanguages.Add(lang);
                }
            }

            return valueset;
        }


    }

}









