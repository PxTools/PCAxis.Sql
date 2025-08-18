using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using PCAxis.Sql.DbClient;
using PCAxis.Sql.DbConfig;
using PCAxis.Sql.Models;
using PCAxis.Sql.Repositories;

namespace PCAxis.Sql.ApiUtils
{
    //For things not found in PXSQLBuilder
    //needed by Pxwebapi2
    //returned data should be defined in PCAxis.Sql.Models if complex

    //In Pxwebapi2 only 1 database is possible, so SqlDbConfigsStatic.DefaultDatabase is the only one.
    //It is therefore simpler to prepare some of the sqls at startup.

    public static class ApiUtilStatic
    {

        private static readonly List<string> LanguagesInDbConfig;
        static ApiUtilStatic()
        {
            Console.WriteLine("Start ApiUtilStatic");
            var config = SqlDbConfigsStatic.DefaultDatabase;
            LanguagesInDbConfig = config.ListAllLanguages();
        }

        //Exceptions ?  What if the valueset only exists in another language: Exceptions!
        static public ValueSet GetValueSet(string valueSetId, string language)
        {
            //validate input
            string okValueSetId = ValidateIdString(valueSetId);
            string languageCode = ValidateLangCodeString(language);

            return ValueSetRepositoryStatic.GetValueSet(okValueSetId, languageCode);
        }

        static public Grouping GetGrouping(string groupingId, string language)
        {
            //validate input
            string okGroupingId = ValidateIdString(groupingId);
            string languageCode = ValidateLangCodeString(language);

            return GroupingRepositoryStatic.GetGrouping(okGroupingId, languageCode);
        }

        /// <summary>
        /// returns an entry for all tables in MenuSelection table that are CompletelyTranslatedCol.
        /// No checks on any other colums (like "has data") 
        ///
        /// ToDo: Is MenuSelectionItem.Menu used anywhere ?
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static MenuLookupTables GetMenuLookupTables(string language)
        {
            string languageCode = ValidateLangCodeString(language);
            return MenuLookupTablesRepositoryStatic.GetMenuLookupTables(languageCode);
        }

        /// <summary>
        /// SELECT MENU ,SELECTION , SELECTION FROM MENUSELECTION  WHERE LEVELNO NOT = 6
        /// SELECT MENU, SELECTION , SELECTION FROM MENUSELECTION_ENG JOIN MENUSELECTION ON  MENU = MENU AND SELECTION = SELECTION WHERE LEVELNO NOT = 6
        ///
        /// For converting a "url-id" to a "backend-id"
        /// Lists all entries regardless of if they are decendats of "the root" (means there can be multiples roots)
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static MenuLookupFolders GetMenuLookupFolders(string language)
        {
            string languageCode = ValidateLangCodeString(language);
            return MenuLookupFoldersRepositoryStatic.GetMenuLookupFolders(languageCode);
        }


        /// <summary> The intended for the (Lucene) indexer in the IndexDatabase endpoint. 
        /// Returns a list of maintable.tableid for  tables where content.published is in the intervall [from,to]
        /// Does not restrict list to DB.MainTable.TableStatusCol.Is("'A'")  or DB.MainTable.PresCategoryCol.Is("'O'"), since
        /// we want to run it with internal DBs.
        /// </summary>
        /// <param name="from">Earliest. MinDate. Inclusive</param>
        /// <param name="to">Lastest. MaxDate. Inclusive</param>
        /// <returns>a list of maintable.tableid which may be empty</returns>
        public static List<string> GetTablesPublishedBetween(DateTime from, DateTime to)
        {
            if (from > to)
                throw new ArgumentException("'from' date cannot be later than 'to' date.");

            return TablesPublishedBetweenRepositoryStatic.GetTablesPublishedBetween(from, to);
        }

        public static bool IsDbConnectionHealthy(string queryText)
        {
            try
            {
                // Try to get the default database config and check if it can connect
                var config = SqlDbConfigsStatic.DefaultDatabase;
                InfoForDbConnection info = config.GetInfoForDbConnection(config.GetDefaultConnString());
                using (var cmd = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString))
                {
                    cmd.ExecuteSelect(queryText);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private static string ValidateLangCodeString(string input)
        {
            if (input == null)
            {
                throw new ArgumentException("The language cannot be null.");
            }
            if (!LanguagesInDbConfig.Contains(input))
            {
                throw new ArgumentException("Cant find language in config.");
            }

            return input;

        }
        private static string ValidateIdString(string input)
        {
            if (input == null)
            {
                throw new ArgumentException("The id string cannot be null.");
            }

            if (!Regex.IsMatch(input, @"^[\w\t \-:.]+$", RegexOptions.None, TimeSpan.FromSeconds(2)))
            {
                throw new ArgumentException("The string contains invalid characters. Only letters, digits, underscores, tabs, spaces, hyphens, colons and periods are allowed.");
            }
            return input;
        }
    }
}
