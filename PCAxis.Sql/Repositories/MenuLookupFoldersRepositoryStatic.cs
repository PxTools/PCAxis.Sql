using System;
using System.Collections.Generic;
using System.Data;

using PCAxis.Sql.DbClient;
using PCAxis.Sql.DbConfig;
using PCAxis.Sql.Models;

namespace PCAxis.Sql.Repositories
{
    internal static class MenuLookupFoldersRepositoryStatic
    {
        private static readonly List<string> LanguagesInDbConfig;

        private static readonly Dictionary<string, string> MenuLookupFoldersQueryByLanguage;
        static MenuLookupFoldersRepositoryStatic()
        {
            //Prepares 1 sql per language. 
            MenuLookupFoldersQueryByLanguage = new Dictionary<string, string>();

            var config = SqlDbConfigsStatic.DefaultDatabase;
            AbstractQueries queries = AbstractQueries.GetSqlqueries(config);

            LanguagesInDbConfig = config.ListAllLanguages();

            foreach (var language in LanguagesInDbConfig)
            {
                MenuLookupFoldersQueryByLanguage[language] = queries.GetMenuLookupFolderQuery(language);
            }

        }

        internal static Models.MenuLookupFolders GetMenuLookupFolders(string language)
        {
            var menuLookup = new Models.MenuLookupFolders();

            string sqlQuery = MenuLookupFoldersQueryByLanguage[language];

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
                    // In some cases the same folder is present twice in the menu structure. This will lead to duplicate keys here, but thats ok.
                    // BKIANL ->  (bb02 bkianl) skipping (pp02 bkianl) (tr01 bkianl) 
                    // Since the rows of bkianl (bkianl *)  does not know their ansestor.  
                    Console.WriteLine("duplicate key: " + key + ", skipping new value " + menu + " " + selection + ". Current value:" + menuLookup[key].Menu + " " + menuLookup[key].Selection);

                }
            }

            if (menuLookup.Count < 1)
            {
                throw new ApplicationException("No folders found for " + sqlQuery);
            }

            // Hmm, is this good? /needed / soucre of infinite loop:
            if (!menuLookup.ContainsKey("START"))
            {
                var item = new MenuSelectionItem();
                item.Menu = "START";
                item.Selection = "START";
                menuLookup.Add("START", item);
            }

            return menuLookup;
        }


    }

}









