using System;
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
    public class ApiUtil
    {
        readonly List<string> _languagesInDbConfig;
        public ApiUtil()
        {
            var config = SqlDbConfigsStatic.DefaultDatabase;
            _languagesInDbConfig = config.ListAllLanguages();
        }

        //Exceptions ?  What if the valueset only exists in another language: Exceptions!
        public ValueSet GetValueSet(string name, string language)
        {
            //validate input
            string valueSetId = ValidateIdString(name);
            string languageCode = ValidateLangCodeString(language, _languagesInDbConfig);

            ValueSetRepository mValueSetRepository = new ValueSetRepository();
            return mValueSetRepository.GetValueSet(valueSetId, languageCode);
        }

        public Grouping GetGrouping(string name, string language)
        {
            //validate input
            string groupingId = ValidateIdString(name);
            string languageCode = ValidateLangCodeString(language, _languagesInDbConfig);

            GroupingRepository mGroupingRepository = new GroupingRepository();
            return mGroupingRepository.GetGrouping(groupingId, languageCode);
        }


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

        private static string ValidateLangCodeString(string input, List<string> languagesInDbConfig)
        {
            if (input == null)
            {
                throw new ArgumentException("The language cannot be null.");
            }
            if (!languagesInDbConfig.Contains(input))
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
