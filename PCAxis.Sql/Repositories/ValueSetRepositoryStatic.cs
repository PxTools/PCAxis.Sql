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
            AbstractQueries queries = AbstractQueries.GetSqlqueries(config);

            InfoForDbConnection info = config.GetInfoForDbConnection(config.GetDefaultConnString());
            var sqlCommand = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);

            foreach (var language in LanguagesInDbConfig)
            {
                string sqlValueset = queries.GetValueSetQuery(language, sqlCommand);
                string sqlValues = queries.GetValueSetValuesQuery(language, sqlCommand);
                string sqlValuesetExistsInLang = GetOtherLanguagesSql(queries, language, config, sqlCommand);
                List<string> sqlquerys = new List<string> { sqlValueset, sqlValues, sqlValuesetExistsInLang };
                SqlquerysByLanguage[language] = sqlquerys;
            }

        }

        internal static Models.ValueSet GetValueSet(string valuesetId, string language)
        {
            //validate input
            if (valuesetId == null || language == null)
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
            parameters[0] = cmd.GetStringParameter("aValueSet", valuesetId);

            var valuesetDS = cmd.ExecuteSelect(sqlValueset, parameters);

            //Oracle works without the 2 next lines, but mssql does not.  
            parameters = new System.Data.Common.DbParameter[1];
            parameters[0] = cmd.GetStringParameter("aValueSet", valuesetId);

            var valuesDS = cmd.ExecuteSelect(sqlValues, parameters);

            //Oracle works without the 2 next lines, but mssql does not.  
            parameters = new System.Data.Common.DbParameter[1];
            parameters[0] = cmd.GetStringParameter("aValueSet", valuesetId);

            DataSet extraLangsDS = String.IsNullOrEmpty(sqlValuesetExistsInLang) ? null : cmd.ExecuteSelect(sqlValuesetExistsInLang, parameters);

            ValueSet valueset = Parse(valuesetId, valuesetDS, valuesDS, extraLangsDS);

            //Adding langs we know exists without checking the DB 
            valueset.AvailableLanguages.Add(config.MainLanguage.code);
            if (!config.MainLanguage.code.Equals(language))
            {
                valueset.AvailableLanguages.Add(language);
            }
            return valueset;
        }

        private static string GetOtherLanguagesSql(AbstractQueries queries, string language, SqlDbConfig config, PxSqlCommand sqlCommand)
        {
            string sqlValuesetExistsInLang = string.Empty;

            string glue = String.Empty;
            foreach (var lang in LanguagesInDbConfig)
            {
                if (!lang.Equals(config.MainLanguage.code) && !lang.Equals(language))
                {
                    //skips: config.MainLanguage has to exist and language will fail in GetValueSetQuery if vaulset is not translated

                    sqlValuesetExistsInLang += glue + queries.GetValueSetExistsIn(lang, sqlCommand);
                    glue = " UNION ";
                }
            }
            return sqlValuesetExistsInLang;

        }

        private static Models.ValueSet Parse(string valuesetId, DataSet valuesetDS, DataSet valuesDS, DataSet extraLangsDS)
        {
            if (valuesetDS.Tables.Count == 0 || valuesetDS.Tables[0].Rows.Count < 1 || valuesDS.Tables.Count == 0)
            {
                throw new ApplicationException("ValueSet " + valuesetId + " not found or empty");
            }

            ValueSet valueset = new ValueSet();
            valueset.Id = valuesetId;
            valueset.Label = valuesetDS.Tables[0].Rows[0][1].ToString();


            //PresText came in version 2.1 and is optional  ...  desciption is up to 200 chars
            if (String.IsNullOrEmpty(valueset.Label))
            {
                var asPresText = valuesetDS.Tables[0].Rows[0][2].ToString(); ;
                int gridPosition = asPresText.IndexOf('#');
                if (gridPosition > 0)
                {
                    asPresText = asPresText.Substring(0, gridPosition);
                }
                valueset.Label = asPresText;
            }




            for (int i = 0; i < valuesDS.Tables[0].Rows.Count; i++)
            {
                var v = new Models.Value();
                v.Code = valuesDS.Tables[0].Rows[i][0].ToString();
                v.Text = valuesDS.Tables[0].Rows[i][3] == DBNull.Value ? valuesDS.Tables[0].Rows[i][4].ToString() : valuesDS.Tables[0].Rows[i][3].ToString();
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









