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


    }

}









