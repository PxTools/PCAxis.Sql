﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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


        //TODO:
        // liste med tabellid og publ dato
        // tabellid er unik, mens publ dato er det vi spørr mot

        //GetMenuLookupTables
        //GetMenuLookupFolders
        /*namespace PxWeb.Code.Api2.DataSource.Cnmm
            {
                public static class SqlDbConfigExtensions
                {
                    public static Dictionary<string, ItemSelection>? GetMenuLookupTables(this SqlDbConfig DB, string language, IOptions<PxApiConfigurationOptions> configOptions)
                    {
                        return GetMenuLookup(DB, language, false, configOptions);
                    }
                    public static Dictionary<string, ItemSelection>? GetMenuLookupFolders(this SqlDbConfig DB, string language, IOptions<PxApiConfigurationOptions> configOptions)
                    {
                        return GetMenuLookup(DB, language, true, configOptions);
                    }



                    private static Dictionary<string, ItemSelection>? GetMenuLookup(this SqlDbConfig DB, string language, bool folders, IOptions<PxApiConfigurationOptions> configOptions)
                    {
                        // Check language to avoid SQL injection
                        if (!configOptions.Value.Languages.Any(l => l.Id == language))
                        {
                            throw new ArgumentException($"Illegal language {language}");

                        }

        */

        //dump to pxfile ?

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
