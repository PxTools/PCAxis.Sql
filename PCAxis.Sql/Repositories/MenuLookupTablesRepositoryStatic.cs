using System;
using System.Collections.Generic;
using System.Data;

using PCAxis.Sql.DbClient;
using PCAxis.Sql.DbConfig;
using PCAxis.Sql.Models;

namespace PCAxis.Sql.Repositories
{
    internal static class MenuLookupTablesRepositoryStatic
    {
        private static readonly List<string> LanguagesInDbConfig;

        private static readonly Dictionary<string, string> MenuLookupTablesQueryByLanguage;
        static MenuLookupTablesRepositoryStatic()
        {
            //Prepares 1 sql per language. 
            MenuLookupTablesQueryByLanguage = new Dictionary<string, string>();

            var config = SqlDbConfigsStatic.DefaultDatabase;
            AbstractQueries queries = AbstractQueries.GetSqlqueries(config);

            LanguagesInDbConfig = config.ListAllLanguages();

            foreach (var language in LanguagesInDbConfig)
            {
                MenuLookupTablesQueryByLanguage[language] = queries.GetMenuLookupTablesQuery(language);
            }

        }

        internal static Models.MenuLookupTables GetMenuLookupTables(string language)
        {
            var menuLookup = new Models.MenuLookupTables();

            string sqlQuery = MenuLookupTablesQueryByLanguage[language];

            var config = SqlDbConfigsStatic.DefaultDatabase;
            InfoForDbConnection info = config.GetInfoForDbConnection(config.GetDefaultConnString());

            var cmd = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);

            var dataSet = cmd.ExecuteSelect(sqlQuery);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                string key = row[2].ToString()?.ToUpper() ?? string.Empty;
                //PR reviewers: better to throw something if key is empty?

                string menu = row[0].ToString();
                string selection = row[1].ToString();

                if (!menuLookup.ContainsKey(key))
                {
                    var item = new MenuSelectionItem();
                    item.Menu = menu;
                    item.Selection = selection;

                    menuLookup.Add(key, item); // Key always uppercase
                }
                else
                {
                    // TODO: Log that this is a duplicate key
                    //PR reviewers: better to throw something?
                    Console.WriteLine(row[0] + " " + row[1]);
                }
            }

            if (menuLookup.Count < 1)
            {
                throw new ApplicationException("No tables found for " + sqlQuery);
            }

            return menuLookup;
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









